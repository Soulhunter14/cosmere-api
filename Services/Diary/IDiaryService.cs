using Messages.Diary.Out;

namespace Services.Diary;

public interface IDiaryService
{
    Task<List<DiaryEntryResponse>> GetEntriesAsync(long campaignId, long userId);
    Task<DiaryEntryResponse> GetEntryAsync(long entryId, long campaignId, long userId);
}
