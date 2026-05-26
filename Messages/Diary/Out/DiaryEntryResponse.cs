namespace Messages.Diary.Out;

public class DiaryMentionDto
{
    public string Raw { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class DiaryEntryResponse
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public int Number { get; set; }
    public required string Title { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<string> Participants { get; set; } = [];
    public List<DiaryMentionDto> Mentions { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
