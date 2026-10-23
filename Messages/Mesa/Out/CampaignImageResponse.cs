namespace Messages.Mesa.Out;

/// <summary>An image of the campaign's table, without its bytes (those come from <c>GET .../mesa/imagenes/{id}</c>).</summary>
public class CampaignImageResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string ContentType { get; set; } = "";
    public int Bytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
