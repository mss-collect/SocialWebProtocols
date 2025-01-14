using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityPub.Persistence.Objects;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// The post repository.
/// </summary>
/// <param name="db">The database.</param>
public class PostRepository(
    TokiDatabaseContext db,
    IOptions<InstanceConfiguration> opts)
{
    /// <summary>
    /// Checks if a post exists by a remote id.
    /// </summary>
    /// <param name="remoteId">Said remote id.</param>
    /// <returns>Whether that post exists.</returns>
    public async Task<bool> PostRemoteIdExists(string remoteId)
    {
        return await db.Posts
            .AnyAsync(p => p.RemoteId == remoteId);
    }
    
    /// <summary>
    /// Finds a post by its remote id.
    /// </summary>
    /// <param name="remoteId">The remote id of the post.</param>
    /// <returns>The post if it exists.</returns>
    public async Task<Post?> FindByRemoteId(string remoteId)
    {
        var post = await db.Posts
            .Include(post => post.Attachments)
            .Include(post => post.Author)
            .Include(post => post.Boosting)
            .FirstOrDefaultAsync(post => post.RemoteId == remoteId);
        
        // TODO: I honestly don't know whether this is the best idea but whatever.
        if (post is null &&
            remoteId.StartsWith($"https://{opts.Value.Domain}/posts"))
        {
            var handle = remoteId.Split('/')
                .Last();
            
            if (Ulid.TryParse(handle, out var id))
                return await FindById(id);
        }

        return post;
    }
    
    /// <summary>
    /// Finds a like by the id pair.
    /// </summary>
    /// <param name="postId">The post id.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>The post, if it exists.</returns>
    public async Task<PostLike?> FindLikeByIds(
        Ulid postId,
        Ulid userId)
    {
        return await db.PostLikes
            .FirstOrDefaultAsync(like => like.LikingUserId == userId &&
                                         like.PostId == postId);
    }
    
    /// <summary>
    /// Finds a post by its id.
    /// </summary>
    /// <param name="id">The id of the post.</param>
    /// <returns>The post if it exists.</returns>
    public async Task<Post?> FindById(Ulid id)
    {
        return await db.Posts
            .Include(post => post.Attachments)
            .Include(post => post.Author)
            .Include(post => post.Parent)
            .ThenInclude(parent => parent!.Author)
            .FirstOrDefaultAsync(post => post.Id == id);
    }

    /// <summary>
    /// Finds a boost by its id and author.
    /// </summary>
    /// <param name="author">The author.</param>
    /// <param name="id">The id of the boosted post.</param>
    /// <returns>The boost, if it exists.</returns>
    public async Task<Post?> FindBoostByIdAndAuthor(
        User author,
        Ulid id)
    {
        return await db.Posts
            .Include(post => post.Boosting)
            .Include(post => post.Boosting!.Author)
            .Include(post => post.Boosting!.Attachments)
            .FirstOrDefaultAsync(post => post.BoostingId == id && post.AuthorId == author.Id);
    }

    /// <summary>
    /// Finds the pinned post given the post itself.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The pinned post, if one exists.</returns>
    public async Task<PinnedPost?> FindPinnedPost(
        Post post)
    {
        return await db.PinnedPosts
            .FirstOrDefaultAsync(pp => pp.PostId == post.Id && pp.UserId == post.AuthorId);
    }

    /// <summary>
    /// Finds the pinned posts for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Their pinned posts.</returns>
    public async Task<IReadOnlyList<PinnedPost>> FindPinnedPostsForUser(
        User user)
    {
        return await db.PinnedPosts
            .OrderByDescending(pp => pp.Id)
            .Where(pp => pp.UserId == user.Id)
            .Include(pp => pp.Post)
            .Include(pp => pp.Post.Attachments)
            .Include(pp => pp.Post.Author)
            .Include(pp => pp.Post.Parent)
            .ThenInclude(parent => parent!.Author)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets the inboxes for all of the people a post is relevant to.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The inboxes.</returns>
    public async Task<IEnumerable<string>> GetInboxesForRelevantPostUsers(Post post)
    {
        List<Ulid> ids =
        [
            post.AuthorId, 
            ..post.Mentions?.Select(Ulid.Parse)
        ];
        
        return await db.Users
            .Include(u => u.ParentInstance)
            .Where(f => f.IsRemote)
            .Where(u => ids.Contains(u.Id))
            .Select(f => f.ParentInstance != null && f.ParentInstance.SharedInbox != null ? f.ParentInstance.SharedInbox! : f.Inbox! )
            .Distinct()
            .ToListAsync();
    }

    /// <summary>
    /// Gets the bookmarks for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The paged view into bookmarks.</returns>
    public PagedView<BookmarkedPost> GetBookmarksForUser(
        User user)
    {
        var query = db.BookmarkedPosts.Where(b => b.UserId == user.Id)
            .Include(b => b.Post)
            .Include(b => b.Post.Author)
            .Include(b => b.Post.Attachments)
            .Include(b => b.Post.Parent)
            .Include(b => b.Post.Parent!.Author)
            .OrderByDescending(b => b.Id);

        return new PagedView<BookmarkedPost>(query);
    }

    /// <summary>
    /// Finds a bookmark given the user and the post.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="post">The post.</param>
    /// <returns>The resulting bookmark, if any.</returns>
    public async Task<BookmarkedPost?> FindBookmarkByUserAndPost(
        User user,
        Post post)
    {
        return await db.BookmarkedPosts
            .FirstOrDefaultAsync(b => b.PostId == post.Id && b.UserId == user.Id);
    }

    /// <summary>
    /// Gets all the likes for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The paginated view into the likes list.</returns>
    public PagedView<PostLike> GetLikesForPost(Post post)
    {
        return new PagedView<PostLike>(
            db.PostLikes
                .Include(like => like.LikingUser)
                .Where(like => like.PostId == post.Id)
                .OrderByDescending(like => like.Id));
    }
    
    /// <summary>
    /// Gets all the boosts for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The paginated view into the boosts list.</returns>
    public PagedView<Post> GetBoostsForPost(Post post)
    {
        return new PagedView<Post>(
            db.Posts
                .Include(p => p.Author)
                .Where(p => p.BoostingId == post.Id)
                .OrderByDescending(p => p.Id));
    }

    /// <summary>
    /// Adds a post to the database.
    /// </summary>
    /// <param name="post">The post.</param>
    public async Task Add(Post post)
    {
        db.Posts.Add(post);

        // Update the post count.
        post.Author.PostCount++;
        db.Users.Update(post.Author);
        
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Adds a pinned post to the database.
    /// </summary>
    /// <param name="post">The pinned post.</param>
    public async Task AddPinnedPost(PinnedPost post)
    {
        db.PinnedPosts.Add(post);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Adds a bookmarked post to the database.
    /// </summary>
    /// <param name="post">The bookmarked post.</param>
    public async Task AddBookmark(BookmarkedPost post)
    {
        db.BookmarkedPosts.Add(post);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Deletes a post from the database.
    /// </summary>
    /// <param name="post">The post.</param>
    public async Task Delete(Post post)
    {
        db.Posts.Remove(post);

        // Update the post count.
        post.Author.PostCount--;
        db.Users.Update(post.Author);
        
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Updates a post
    /// </summary>
    /// <param name="post">The post.</param>
    public async Task Update(
        Post post)
    {
        db.Posts.Update(post);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Deletes a pinned post from the database.
    /// </summary>
    /// <param name="post">The pinned post.</param>
    public async Task DeletePinnedPost(PinnedPost post)
    {
        db.PinnedPosts.Remove(post);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Adds a like to the post likes collection.
    /// </summary>
    /// <param name="like">The like.</param>
    /// <param name="post">The post.</param>
    public async Task AddLike(
        PostLike like,
        Post post)
    {
        // Increment the amount of likes for this post.
        post.LikeCount++;
        db.Posts.Update(post);
        
        db.PostLikes.Add(like);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes a like from a post.
    /// </summary>
    /// <param name="like">The like.</param>
    public async Task RemoveLike(
        PostLike like)
    {
        var post = like.Post;
        post.LikeCount--;
        db.Posts.Update(post);
        
        db.PostLikes.Remove(like);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes a boost from a post.
    /// </summary>
    /// <param name="boost">The boost.</param>
    public async Task RemoveBoost(
        Post boost)
    {
        if (boost.Boosting is null)
            return;
        
        var post = boost.Boosting!;
        post.BoostCount--;
        db.Posts.Update(post);
        
        db.Posts.Remove(boost);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes a bookmark.
    /// </summary>
    /// <param name="bookmark">The bookmark.</param>
    public async Task RemoveBookmark(
        BookmarkedPost bookmark)
    {
        db.BookmarkedPosts.Remove(bookmark);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Adds a boost to the post.
    /// </summary>
    /// <param name="boost">The boost.</param>
    /// <param name="post">The post.</param>
    public async Task AddBoost(
        Post boost,
        Post post)
    {
        // Increment the amount of likes for this post.
        post.BoostCount++;
        db.Posts.Update(post);
        
        db.Posts.Add(boost);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a custom query.
    /// </summary>
    /// <returns>The query.</returns>
    public IQueryable<Post> CreateCustomQuery() =>
        db.Posts;

    /// <summary>
    /// Creates a custom query for post likes.
    /// </summary>
    /// <returns>The post likes query.</returns>
    public IQueryable<PostLike> CreateCustomLikeQuery() =>
        db.PostLikes;

    /// <summary>
    /// Creates a custom query for bookmarked posts.
    /// </summary>
    /// <returns>The bookmarked posts query.</returns>
    public IQueryable<BookmarkedPost> CreateCustomBookmarkedPostQuery() =>
        db.BookmarkedPosts;
    
    /// <summary>
    /// Creates a detached attachment (one without a parent <see cref="Post"/>).
    /// </summary>
    /// <param name="url">The url of the file.</param>
    /// <param name="description">Its description.</param>
    /// <param name="mime">The mime type of the file.</param>
    /// <returns>The attachment.</returns>
    public async Task<PostAttachment> CreateDetachedAttachment(
        string url,
        string? description,
        string? mime)
    {
        var attachment = new PostAttachment
        {
            Id = Ulid.NewUlid(),
            Url = url,
            Description = description,
            Mime = mime
        };

        db.PostAttachments.Add(attachment);
        await db.SaveChangesAsync();

        return attachment;
    }

    /// <summary>
    /// Returns multiple attachments by their ids.
    /// </summary>
    /// <param name="ids">The id list.</param>
    /// <returns>The list of attachments.</returns>
    public async Task<IList<PostAttachment>?> FindMultipleAttachmentsByIds(
        IList<Ulid> ids)
    {
        return await db.PostAttachments
            .Where(pa => ids.Contains(pa.Id))
            .ToListAsync();
    }

    /// <summary>
    /// Finds an attachment by its id.
    /// </summary>
    /// <param name="id">The id of the attachment.</param>
    /// <returns>The attachment, or nothing.</returns>
    public async Task<PostAttachment?> FindAttachmentById(
        Ulid id)
    {
        return await db.PostAttachments
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Updates an attachment
    /// </summary>
    /// <param name="attachment">The attachment.</param>
    public async Task UpdateAttachment(
        PostAttachment attachment)
    {
        db.PostAttachments.Update(attachment);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Imports attachments for a given post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <param name="attachments">The attachments.</param>
    public async Task ImportAttachmentsForNote(
        Post post,
        IEnumerable<ASDocument> attachments)
    {
        foreach (var document in attachments)
        {
            if (document.Url is null)
                continue;
            
            var attachment = new PostAttachment()
            {
                Id = Ulid.NewUlid(),

                Parent = post,
                ParentId = post.Id,

                Description = document.Name,
                Mime = document.MediaType,
                Url = document.Url
            };
            
            db.PostAttachments.Add(attachment);
        }

        await db.SaveChangesAsync();
    }
}