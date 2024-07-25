using ConflictAutomation.Extensions;
using ConflictAutomation.Models.PreScreening.SubClasses;
using PACE;

namespace ConflictAutomation.Mappers;

public static class NoteMapper
{
    private static Note CreateFrom(NoteModel noteModel) => new()
    {
        Created = (noteModel.CreatedDate > DateTime.MinValue)
                        ? noteModel.CreatedDate.TimestampWithTimezoneFromUtc()
                        : string.Empty,
        CreatedBy = noteModel.UserName, 
        Category = noteModel.NoteTypeName, 
        Comments = noteModel.Note
    };


    public static List<Note> CreateFrom(List<NoteModel> noteModels) =>
        noteModels.IsNullOrEmpty() ? [] : noteModels.Select(CreateFrom).ToList();
}
