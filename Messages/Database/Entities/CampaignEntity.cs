using Messages.Worlds;

namespace Messages.Database.Entities;

public class CampaignEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public long GmUserId { get; set; }
    public required string InviteCode { get; set; }
    public bool InviteActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string World { get; set; } = WorldIds.Stormlight;
    public string? Era { get; set; }
    /// <summary>Cuándo el director inició la campaña; <c>null</c> = en preparación (sesión 0). Ver <c>CierreCampana</c>.</summary>
    public DateTime? IniciadaEn { get; set; }

    public UserEntity GmUser { get; set; } = null!;
    public ICollection<CampaignMemberEntity> Members { get; set; } = [];
    public ICollection<CharacterEntity> Characters { get; set; } = [];
    public ICollection<SessionEntity> Sessions { get; set; } = [];
    public ICollection<SessionProposalEntity> Proposals { get; set; } = [];
}
