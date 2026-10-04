namespace Messages.Catalog.In;

public class CreateWeaponRequest
{
    public required string Name { get; set; }
    public int WeaponTypeId { get; set; }
    public int SkillId { get; set; }
    public int DamageDiceCount { get; set; }
    public int DamageDiceValue { get; set; }
    public int DamageTypeId { get; set; }
    public int RangeId { get; set; }
    public List<int> TraitIds { get; set; } = [];
    public List<int> ExpertTraitIds { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }

    /// <summary>Campaign the item is created for. Nullable: a cached client without it keeps creating Stormlight items.</summary>
    public long? CampaignId { get; set; }
    public double? Price { get; set; }
    public bool IsRewardOnly { get; set; }
}

public class CreateArmorRequest
{
    public required string Name { get; set; }
    public int ArmorTypeId { get; set; }
    public int Desvio { get; set; }
    public List<int> TraitIds { get; set; } = [];
    public List<int> ExpertTraitIds { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }

    /// <summary>Campaign the item is created for. Nullable: a cached client without it keeps creating Stormlight items.</summary>
    public long? CampaignId { get; set; }
    public double? Price { get; set; }
    public bool IsRewardOnly { get; set; }
}

public class UpdateDescriptionRequest
{
    public string Description { get; set; } = string.Empty;
}
