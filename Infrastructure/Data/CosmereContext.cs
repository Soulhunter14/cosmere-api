using Messages.Database.Entities;
using Messages.Worlds;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class CosmereContext(DbContextOptions<CosmereContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<CampaignEntity> Campaigns { get; set; }
    public DbSet<CampaignMemberEntity> CampaignMembers { get; set; }
    public DbSet<CharacterEntity> Characters { get; set; }
    public DbSet<MetaEntity> Metas { get; set; }
    public DbSet<GlobalNpcEntity> GlobalNpcs { get; set; }
    public DbSet<NpcNoteEntity> NpcNotes { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }
    public DbSet<SessionProposalEntity> SessionProposals { get; set; }
    public DbSet<ProposalDateEntity> ProposalDates { get; set; }
    public DbSet<ProposalVoteEntity> ProposalVotes { get; set; }
    public DbSet<NoteEntity> Notes { get; set; }
    public DbSet<WeaponCatalogEntity> WeaponCatalog { get; set; }
    public DbSet<ArmorCatalogEntity> ArmorCatalog { get; set; }
    public DbSet<GearItemEntity> GearItems { get; set; }
    public DbSet<CatalogOptionEntity> CatalogOptions { get; set; }
    public DbSet<LockedDayEntity> LockedDays { get; set; }
    public DbSet<DiaryEntryEntity> DiaryEntries { get; set; }
    public DbSet<DiceRollEntity> DiceRolls { get; set; }

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

        // Character → Metas
        modelBuilder.Entity<MetaEntity>()
            .HasOne(m => m.Character)
            .WithMany(c => c.Metas)
            .HasForeignKey(m => m.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Campaign → NpcNotes
        modelBuilder.Entity<NpcNoteEntity>()
            .HasOne(n => n.Campaign)
            .WithMany()
            .HasForeignKey(n => n.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NpcNoteEntity>()
            .HasOne(n => n.Author)
            .WithMany()
            .HasForeignKey(n => n.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Campaign → Sessions
        modelBuilder.Entity<SessionEntity>()
            .HasOne(s => s.Campaign)
            .WithMany(c => c.Sessions)
            .HasForeignKey(s => s.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Campaign → SessionProposals
        modelBuilder.Entity<SessionProposalEntity>()
            .HasOne(p => p.Campaign)
            .WithMany(c => c.Proposals)
            .HasForeignKey(p => p.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // SessionProposal → PromotedSession (optional, no cascade to avoid cycle)
        modelBuilder.Entity<SessionProposalEntity>()
            .HasOne(p => p.PromotedSession)
            .WithMany()
            .HasForeignKey(p => p.PromotedSessionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // SessionProposal → ProposalDates
        modelBuilder.Entity<ProposalDateEntity>()
            .HasOne(d => d.Proposal)
            .WithMany(p => p.ProposedDates)
            .HasForeignKey(d => d.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProposalDate → Session created when that date (or slot) is promoted (optional, no cascade)
        modelBuilder.Entity<ProposalDateEntity>()
            .HasOne(d => d.Session)
            .WithMany()
            .HasForeignKey(d => d.SessionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // ProposalDate → ProposalVotes
        modelBuilder.Entity<ProposalVoteEntity>()
            .HasOne(v => v.ProposalDate)
            .WithMany(d => d.Votes)
            .HasForeignKey(v => v.ProposalDateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProposalVoteEntity>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique vote per user per date
        modelBuilder.Entity<ProposalVoteEntity>()
            .HasIndex(v => new { v.ProposalDateId, v.UserId })
            .IsUnique();

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

        // Catalog world filters (M3, AddWorldToCatalog): objects are filtered by World and options by (World, Category);
        // the options index replaces the former category-only index.
        modelBuilder.Entity<WeaponCatalogEntity>()
            .HasIndex(w => w.World);

        modelBuilder.Entity<ArmorCatalogEntity>()
            .HasIndex(a => a.World);

        modelBuilder.Entity<GearItemEntity>()
            .HasIndex(g => g.World);

        modelBuilder.Entity<CatalogOptionEntity>()
            .HasIndex(o => new { o.World, o.Category });

        // Global NPC world filter (M5, AddWorldToGlobalNpcs): the list is filtered by the world of the campaign.
        modelBuilder.Entity<GlobalNpcEntity>()
            .HasIndex(n => n.World);

        // Campaign → LockedDays
        modelBuilder.Entity<LockedDayEntity>()
            .HasOne(l => l.Campaign)
            .WithMany()
            .HasForeignKey(l => l.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LockedDayEntity>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One lock per user per date per campaign
        modelBuilder.Entity<LockedDayEntity>()
            .HasIndex(l => new { l.CampaignId, l.UserId, l.Date })
            .IsUnique();

        // Campaign → DiaryEntries
        modelBuilder.Entity<DiaryEntryEntity>()
            .HasOne(d => d.Campaign)
            .WithMany()
            .HasForeignKey(d => d.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiaryEntryEntity>()
            .HasIndex(d => new { d.CampaignId, d.Number })
            .IsUnique();

        modelBuilder.Entity<DiaryEntryEntity>()
            .HasIndex(d => new { d.CampaignId, d.Slug })
            .IsUnique();

        // Campaign → DiceRolls
        modelBuilder.Entity<DiceRollEntity>()
            .HasOne(r => r.Campaign)
            .WithMany()
            .HasForeignKey(r => r.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiceRollEntity>()
            .HasIndex(r => new { r.CampaignId, r.CreatedAt });

        // Database defaults for the world-specific columns. EF does not read the C# initializers when it generates a
        // migration, so the default is declared here: existing rows (and clients that never send the column) stay on
        // Stormlight. Later migrations add their own lines to this block.
        modelBuilder.Entity<CampaignEntity>()
            .Property(c => c.World)
            .HasDefaultValue(WorldIds.Stormlight);

        // Nacidos de la bruma (M2, AddCharacterMistbornFields): existing characters get empty metalborn data.
        modelBuilder.Entity<CharacterEntity>().Property(c => c.CaminoMetal).HasDefaultValue("");
        modelBuilder.Entity<CharacterEntity>().Property(c => c.CaminoInicial).HasDefaultValue("");
        modelBuilder.Entity<CharacterEntity>().Property(c => c.Poderes).HasDefaultValue("[]");
        modelBuilder.Entity<CharacterEntity>().Property(c => c.Recursos).HasDefaultValue("{}");
        modelBuilder.Entity<CharacterEntity>().Property(c => c.Bendiciones).HasDefaultValueSql("'{}'");

        // Catalog (M3, AddWorldToCatalog): existing objects and options stay on Stormlight; the migration itself moves the
        // options shared by every world to 'cosmere'.
        modelBuilder.Entity<WeaponCatalogEntity>().Property(w => w.World).HasDefaultValue(WorldIds.Stormlight);
        modelBuilder.Entity<ArmorCatalogEntity>().Property(a => a.World).HasDefaultValue(WorldIds.Stormlight);
        modelBuilder.Entity<GearItemEntity>().Property(g => g.World).HasDefaultValue(WorldIds.Stormlight);
        modelBuilder.Entity<CatalogOptionEntity>().Property(o => o.World).HasDefaultValue(WorldIds.Stormlight);

        // Global NPCs (M5, AddWorldToGlobalNpcs): the adversaries already stored (Caminapiedras) stay on Stormlight.
        modelBuilder.Entity<GlobalNpcEntity>().Property(n => n.World).HasDefaultValue(WorldIds.Stormlight);

        // Hemalurgia (M6, AddCharacterClavos): existing characters have no hemalurgic spikes.
        modelBuilder.Entity<CharacterEntity>().Property(c => c.Clavos).HasDefaultValue("[]");

        // Proposal slots (AddProposalDateSlots): each proposed date is resolved on its own; existing dates start pending and
        // the migration backfills the ones of proposals already resolved.
        modelBuilder.Entity<ProposalDateEntity>().Property(d => d.Status).HasDefaultValue("Pending");
    }
}
