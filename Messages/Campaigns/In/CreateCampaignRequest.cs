using Messages.Worlds;

namespace Messages.Campaigns.In;

public class CreateCampaignRequest
{
    public required string Name { get; set; }

    /// <summary>Mundo de la campaña (<see cref="WorldIds"/>); si el cliente no lo envía, Stormlight.</summary>
    public string World { get; set; } = WorldIds.Stormlight;

    /// <summary>Era (<c>era1</c> | <c>era2</c>): obligatoria en Nacidos de la bruma y ignorada (<c>null</c>) en Stormlight.</summary>
    public string? Era { get; set; }
}

public class JoinCampaignRequest
{
    public required string InviteCode { get; set; }
}

public class UpdateInviteRequest
{
    public bool InviteActive { get; set; }
}
