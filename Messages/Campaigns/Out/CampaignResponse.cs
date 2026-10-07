using Messages.Worlds;

namespace Messages.Campaigns.Out;

public class CampaignResponse
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NextSessionDate { get; set; }
    public string? NextSessionTitle { get; set; }
    public string World { get; set; } = WorldIds.Stormlight;
    public string? Era { get; set; }
    /// <summary><c>null</c> = en preparación; con fecha, los jugadores ya no cambian <see cref="CamposDeCierre"/>.</summary>
    public DateTime? IniciadaEn { get; set; }
    /// <summary>Campos del personaje (camelCase) que se cierran para los jugadores al iniciar la campaña (siempre la lista completa).</summary>
    public List<string> CamposDeCierre { get; set; } = [];
}

public class CampaignDetailResponse
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
    public string? InviteCode { get; set; }
    public bool InviteActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MemberResponse> Members { get; set; } = [];
    public string World { get; set; } = WorldIds.Stormlight;
    public string? Era { get; set; }
    public DateTime? IniciadaEn { get; set; }
    public List<string> CamposDeCierre { get; set; } = [];
}

public class MemberResponse
{
    public long UserId { get; set; }
    public required string DisplayName { get; set; }
    public required string Role { get; set; }
}
