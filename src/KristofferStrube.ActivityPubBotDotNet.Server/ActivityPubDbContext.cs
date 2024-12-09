//using Microsoft.EntityFrameworkCore;

using System.Security.Cryptography.Xml;

namespace KristofferStrube.ActivityPubBotDotNet.Server;


public static class EFExtensions
{
    public static UserInfo? Find(this List<UserInfo> users, string filter)
    {
        throw new NotImplementedException("NI001");
        //return null;
    }

    public static FollowRelation? Find(this List<FollowRelation> relations, string filter1, string filter2)
    {
        throw new NotImplementedException("NI001");
        //return null;
    }



}
public class ActivityPubDbContext //: DbContext
{
    //public ActivityPubDbContext(DbContextOptions<ActivityPubDbContext> options) : base(options) { }

    public void Add(FollowRelation rel)
    {
        throw new NotImplementedException("NI1");
    }

    public void Add(UserInfo user)
    {
        throw new NotImplementedException("NI12");
    }


    public void SaveChanges()
    {
        throw new NotImplementedException("NI");
    }
    public List<UserInfo> Users = new List<UserInfo>();

    public List<FollowRelation> FollowRelations = new List<FollowRelation>();
    //public DbSet<UserInfo> Users => Set<UserInfo>();
    //public DbSet<FollowRelation> FollowRelations => Set<FollowRelation>();

    //protected override void OnModelCreating(ModelBuilder builder)
    //{
    //    builder.Entity<UserInfo>()
    //        .HasKey(u => u.Id);

    //    builder.Entity<FollowRelation>()
    //        .HasKey(f => new { f.FollowerId, f.FollowedId });

    //    builder.Entity<FollowRelation>()
    //        .HasOne(f => f.Follower)
    //        .WithMany(u => u.Followers)
    //        .HasForeignKey(f => f.FollowerId);

    //    builder.Entity<FollowRelation>()
    //        .HasOne(f => f.Followed)
    //        .WithMany(u => u.Following)
    //        .HasForeignKey(f => f.FollowedId);

    //}
}
