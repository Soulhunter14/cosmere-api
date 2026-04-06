using Messages.Notes.In;
using Messages.Notes.Out;

namespace Services.Notes;

public interface INoteService
{
    Task<List<NoteResponse>> GetNotesAsync(long campaignId, long userId);
    Task<NoteResponse> CreateNoteAsync(long campaignId, CreateNoteRequest request, long userId);
    Task MarkAsReadAsync(long noteId, long campaignId, long userId);
}
