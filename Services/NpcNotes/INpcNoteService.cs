using Messages.NpcNotes.In;
using Messages.NpcNotes.Out;

namespace Services.NpcNotes;

public interface INpcNoteService
{
    Task<List<NpcNoteResponse>> GetNpcNotesAsync(long campaignId, long userId);
    Task<NpcNoteResponse> CreateNpcNoteAsync(long campaignId, CreateNpcNoteRequest request, long userId);
    Task<NpcNoteResponse> UpdateNpcNoteAsync(long noteId, long campaignId, UpdateNpcNoteRequest request, long userId);
    Task DeleteNpcNoteAsync(long noteId, long campaignId, long userId);
}
