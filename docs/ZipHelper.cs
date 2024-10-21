using GEARAPI.Application.Extensions;
using System.IO.Compression;

namespace GEARAPI.Application.Helpers;


#pragma warning disable IDE0063 // Use simple 'using' statement
public static class ZipHelper
{
    public static byte[] Zip(byte[] inputByteArray, string fileNameInsideZip)
    {
        using (MemoryStream zipArchiveStream = new())
        {
            using (ZipArchive zipArchive = new(zipArchiveStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(fileNameInsideZip);
                using (Stream zipArchiveEntryStream = zipArchiveEntry.Open())
                {
                    zipArchiveEntryStream.Write(inputByteArray, 0, inputByteArray.Length);                    
                }
            }
            return zipArchiveStream.ToArray();
        }
    }


public static string? Zip(List<string> inputFilePaths, string outputZipFilePath)
    {
        ValidateParameters(inputFilePaths, outputZipFilePath);

        try
        {
            outputZipFilePath.DeleteIfExistent();

            using (ZipArchive zipArchive = ZipFile.Open(outputZipFilePath, ZipArchiveMode.Create))
            {
                foreach (string inputFilePath in inputFilePaths)
                {
                    zipArchive.CreateEntryFromFile(inputFilePath, Path.GetFileName(inputFilePath));
                }
            }

            return File.Exists(outputZipFilePath) ? outputZipFilePath : string.Empty;
        }
        catch
        {
            return null;
        }
    }


    public static void Unzip(string zipFilePath, string destinationFolderPath)
    {
        if (!File.Exists(zipFilePath))
        {
            throw new FileNotFoundException($"The file '{zipFilePath}' does not exist.");
        }

        if (!Directory.Exists(destinationFolderPath))
        {
            Directory.CreateDirectory(destinationFolderPath);
        }

        ZipFile.ExtractToDirectory(zipFilePath, destinationFolderPath);
    }


    private static void ValidateParameters(List<string> inputFilePaths, string outputZipFilePath)
    {
        if (inputFilePaths.IsNullOrEmpty())
        {
            throw new ArgumentException("Parameter inputFilePaths: non-empty List<string> is required.");
        }
        else
        {
            var details = inputFilePaths.CheckExistenceOfAllFilePaths();
            if (!details.IsNullOrEmpty())
            {
                throw new ArgumentException("Parameter inputFilePaths: all files must exist.\n" +
                    $"Details:\n{string.Join('\n', details!.Select(det => '\t' + det))}");
            }
        }

        if (string.IsNullOrWhiteSpace(outputZipFilePath))
        {
            throw new ArgumentException("Parameter outputZipFilePath: non-empty string is required.");
        }
        else if (!outputZipFilePath.IsValidPath())
        {
            throw new ArgumentException("Parameter outputZipFilePath: invalid path.");
        }
    }
}
#pragma warning restore IDE0063 // Use simple 'using' statement
