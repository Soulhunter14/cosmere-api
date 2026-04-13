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
}

public class CreateArmorRequest
{
    public required string Name { get; set; }
    public int ArmorTypeId { get; set; }
    public int Desvio { get; set; }
    public List<int> TraitIds { get; set; } = [];
    public List<int> ExpertTraitIds { get; set; } = [];
}
