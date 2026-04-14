namespace Messages.LockedDays.In;

public class CreateLockedDayRequest
{
    public string Date { get; set; } = string.Empty; // "yyyy-MM-dd"
    public string Note { get; set; } = string.Empty;
}
