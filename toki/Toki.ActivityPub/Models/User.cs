using Toki.ActivityPub.Models.Users;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Models;

/// <summary>
/// A user.
/// </summary>
public class User : RemoteableModel
{
    /// <summary>
    /// The display name of the user.
    /// </summary>
    public required string DisplayName { get; set; }
    
    /// <summary>
    /// The handle of this user. (For remote users it's of the form @someone@something.tld)
    /// </summary>
    public required string Handle { get; set; }

    /// <summary>
    /// The last update time of this user.
    /// </summary>
    public DateTimeOffset LastUpdateTime { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);
    
    /// <summary>
    /// The instance this user is a part of.
    /// </summary>
    public RemoteInstance? ParentInstance { get; set; }
    
    /// <summary>
    /// The id of the parent instance.
    /// </summary>
    public Ulid? ParentInstanceId { get; set; }
    
    /// <summary>
    /// Does this user require follow approval?
    /// </summary>
    public bool RequiresFollowApproval { get; set; }
    
    /// <summary>
    /// The bio of this user.
    /// </summary>
    public string? Bio { get; set; }
    
    /// <summary>
    /// Is this user remote?
    /// </summary>
    public bool IsRemote { get; set; }
    
    /// <summary>
    /// The inbox of this user. (Can be null for local users.)
    /// </summary>
    public string? Inbox { get; set; }
    
    /// <summary>
    /// The url of this user's avatar.
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// The url of this user's banner.
    /// </summary>
    public string? BannerUrl { get; set; }
    
    /// <summary>
    /// The keypair of the user.
    /// </summary>
    public Keypair? Keypair { get; init; }
    
    /// <summary>
    /// The follower relations of this user.
    /// </summary>
    public ICollection<FollowerRelation>? FollowerRelations { get; set; }
    
    /// <summary>
    /// The pinned posts of this user.
    /// </summary>
    public ICollection<PinnedPost>? PinnedPosts { get; set; }
    
    /// <summary>
    /// The follower count of this user. (DENORMALIZED)
    /// </summary>
    public int FollowerCount { get; set; }
    
    /// <summary>
    /// The following count of this user. (DENORMALIZED)
    /// </summary>
    public int FollowingCount { get; set; }
    
    /// <summary>
    /// The amount of posts this user has made. (DENORMALIZED)
    /// </summary>
    public int PostCount { get; set; }
    
    /// <summary>
    /// The profile fields. (DENORMALIZED, jsonb)
    /// </summary>
    public IList<UserProfileField>? Fields { get; set; }
}