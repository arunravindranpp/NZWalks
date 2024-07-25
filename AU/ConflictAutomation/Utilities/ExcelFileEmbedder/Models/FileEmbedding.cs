namespace ConflictAutomation.Utilities.ExcelFileEmbedder.Models;

public class FileEmbedding
{
    public string FileToBeEmbeddedFullPath { get; init; }
    public string TargetSheetName { get; init; }
    public string TargetCell { get; init; }
    public string IconFileFullPath { get; init; }
    public int IconIndex { get; init; }


    public FileEmbedding(
        string fileToBeEmbeddedFullPath,
        string targetSheetName,
        string targetCell,
        string iconFileFullPath = null,
        int iconIndex = 0)
    {
        this.FileToBeEmbeddedFullPath = fileToBeEmbeddedFullPath;
        this.TargetSheetName = targetSheetName;
        this.TargetCell = targetCell;

        if (string.IsNullOrWhiteSpace(iconFileFullPath))
        {
            this.IconFileFullPath = Path.Combine(Directory.GetCurrentDirectory(), "XLICONS.EXE");
        }
        else
        {
            this.IconFileFullPath = iconFileFullPath;
        }

        this.IconIndex = iconIndex;
    }
}