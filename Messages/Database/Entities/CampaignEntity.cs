namespace Messages.Database.Entities;

public class CampaignEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public long GmUserId { get; set; }
    public required string InviteCode { get; set; }
    public bool InviteActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserEntity GmUser { get; set; } = null!;
    public ICollection<CampaignMemberEntity> Members { get; set; } = [];
    public ICollection<CharacterEntity> Characters { get; set; } = [];
    public ICollection<MatchEntity> Matches { get; set; } = [];
    public ICollection<SideQuestEntity> SideQuests { get; set; } = [];
    public ICollection<SessionEntity> Sessions { get; set; } = [];
}
