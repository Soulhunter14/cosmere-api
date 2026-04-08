using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class CosmereContext(DbContextOptions<CosmereContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<CampaignEntity> Campaigns { get; set; }
    public DbSet<CampaignMemberEntity> CampaignMembers { get; set; }
    public DbSet<CharacterEntity> Characters { get; set; }
    public DbSet<GlobalNpcEntity> GlobalNpcs { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }
    public DbSet<NoteEntity> Notes { get; set; }
    public DbSet<WeaponCatalogEntity> WeaponCatalog { get; set; }
    public DbSet<ArmorCatalogEntity> ArmorCatalog { get; set; }
    public DbSet<GearItemEntity> GearItems { get; set; }
    public DbSet<CatalogOptionEntity> CatalogOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CampaignMember composite key
        modelBuilder.Entity<CampaignMemberEntity>()
            .HasKey(m => new { m.CampaignId, m.UserId });

        // Campaign → GM User
        modelBuilder.Entity<CampaignEntity>()
            .HasOne(c => c.GmUser)
            .WithMany()
            .HasForeignKey(c => c.GmUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Campaign → Members
        modelBuilder.Entity<CampaignMemberEntity>()
            .HasOne(m => m.Campaign)
            .WithMany(c => c.Members)
            .HasForeignKey(m => m.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CampaignMemberEntity>()
            .HasOne(m => m.User)
            .WithMany(u => u.CampaignMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Campaign → Characters
        modelBuilder.Entity<CharacterEntity>()
            .HasOne(c => c.Campaign)
            .WithMany(c => c.Characters)
            .HasForeignKey(c => c.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Campaign → Sessions
        modelBuilder.Entity<SessionEntity>()
            .HasOne(s => s.Campaign)
            .WithMany(c => c.Sessions)
            .HasForeignKey(s => s.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Character → Owner (optional)
        modelBuilder.Entity<CharacterEntity>()
            .HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Campaign → Notes
        modelBuilder.Entity<NoteEntity>()
            .HasOne(n => n.Campaign)
            .WithMany()
            .HasForeignKey(n => n.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NoteEntity>()
            .HasOne(n => n.FromUser)
            .WithMany()
            .HasForeignKey(n => n.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NoteEntity>()
            .HasOne(n => n.ToUser)
            .WithMany()
            .HasForeignKey(n => n.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // InviteCode unique index
        modelBuilder.Entity<CampaignEntity>()
            .HasIndex(c => c.InviteCode)
            .IsUnique();

        // User username unique index
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // CatalogOption category index
        modelBuilder.Entity<CatalogOptionEntity>()
            .HasIndex(o => o.Category);
    }
}
