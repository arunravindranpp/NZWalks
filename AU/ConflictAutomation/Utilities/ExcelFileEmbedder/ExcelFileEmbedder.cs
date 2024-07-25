using ConflictAutomation.Services;
using ConflictAutomation.Utilities.ExcelFileEmbedder.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using InteropExcel = Microsoft.Office.Interop.Excel;
// To use Interop.Excel, you must add the NuGet package Microsoft.Office.Interop.Excel
// and manually add the following .DLLs to the project:
//    C:\Windows\assembly\GAC_MSIL\Microsoft.Vbe.Interop\15.0.0.0__71e9bce111e9429c\Microsoft.Vbe.Interop.dll
//      and
//    C:\Windows\assembly\GAC_MSIL\office\15.0.0.0__71e9bce111e9429c\OFFICE.DLL


// *** IMPORTANT WARNING *** 
// 
// Do NOT use OneDrive locations, like the "My Documents" folder, to save the files.
// Use a non-OneDrive location instead, like C:\Test
//     Otherwise, you will this exception intermittently: 
//         Exception from HRESULT: 0x800A03EC
//           System.Runtime.InteropServices.COMException (0x800A03EC): Microsoft Excel cannot access the file '...'
// 
// Source: https://stackoverflow.com/questions/58189325/error-message-when-trying-to-save-my-created-excel-file-in-c-sharp
//         See post from speedyps on Jul 20, 2022 at 2:42

namespace ConflictAutomation.Utilities.ExcelFileEmbedder;

public partial class ExcelFileEmbedder : IDisposable
{
    private readonly InteropExcel.Application excelApplication;
    private readonly Process excelProcess;
    private readonly InteropExcel.Workbooks excelWorkbooks;
    private readonly InteropExcel.Workbook excelWorkbook;
    private bool disposed = false;


    public ExcelFileEmbedder(string targetWorkbookFilePath)
    {
        excelApplication = new()
        {
            Visible = false,
            DisplayAlerts = false
        };

        excelProcess = GetExcelProcess();

        excelWorkbooks = excelApplication.Workbooks;
        excelWorkbook = excelWorkbooks.Open(targetWorkbookFilePath);
    }


    public void EmbedDetailFiles(List<FileEmbedding> listFileEmbeddings, string homeSheetName = "Summary")
    {
        if (listFileEmbeddings == null || listFileEmbeddings.Count == 0)
        {
            return;
        }
        try
        {
            foreach (var fileEmbedding in listFileEmbeddings)
            {
                var excelWorksheet = (InteropExcel.Worksheet)excelWorkbook.Sheets[fileEmbedding.TargetSheetName];
                SelectRange(excelWorksheet, fileEmbedding.TargetCell);

                string iconLabel = Path.GetFileName(fileEmbedding.FileToBeEmbeddedFullPath);

                var oleObjects = (InteropExcel.OLEObjects)excelWorksheet.OLEObjects(Type.Missing);
                oleObjects.Add(
                    ClassType: Type.Missing,
                    Filename: fileEmbedding.FileToBeEmbeddedFullPath,
                    Link: false,
                    DisplayAsIcon: true,
                    IconFileName: fileEmbedding.IconFileFullPath,
                    IconIndex: fileEmbedding.IconIndex,
                    IconLabel: iconLabel
                ).Select();

                SelectRange(excelWorksheet, "A1");
            }

            var homeWorksheet = (InteropExcel.Worksheet)excelWorkbook.Sheets[homeSheetName];
            SelectRange(homeWorksheet, "A1");

            excelWorkbook.Save();
        }
        catch(Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }

    
    private static InteropExcel.Worksheet SelectRange(InteropExcel.Worksheet excelWorksheet, string reference)
    {
        excelWorksheet.Activate();

        var range = excelWorksheet.Range[reference];
        range.Select();

        return excelWorksheet;
    }


    #region OBJECT DESTRUCTION
    public void Dispose()
    {
        disposed = true;
        TerminateExcelObjects();
        GC.SuppressFinalize(this);
    }


    ~ExcelFileEmbedder()
    {
        if (!disposed)
        {
            Dispose();
        }
    }
    #endregion OBJECT DESTRUCTION


    #region KILL EXCEL PROCESS
    private void TerminateExcelObjects()
    {
        try
        {
            excelApplication.Workbooks.Close();
            excelApplication.Quit();
            KillExcelProcess();
        }
        catch(Exception ex) 
        {
            LoggerInfo.LogException(ex);
        }
    }


    // The three methods below, intended to kill EXCEL.EXE process, are based on:
    // https://www.codeproject.com/Questions/74980/Close-Excel-Process-with-Interop

    public Process GetExcelProcess()
    {
        // The _ = is a discard, to tell the compiler that the return value does not matter.
        _ = GetWindowThreadProcessId(new IntPtr(excelApplication.Hwnd), out uint excelProcessId);

        return Process.GetProcessById((int)excelProcessId);
    }


    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


    private void KillExcelProcess()
    {
        if (excelProcess is null)
        {
            return;
        }

        try
        {
            excelProcess.CloseMainWindow();
            excelProcess.Refresh();
            excelProcess.Kill();
        }
        catch
        {
            // Process was already killed
        }

        for (int i = 0; i < 2; i++)
        {
            if (excelProcess.HasExited)
            {
                break;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(200);
        }
    }
    #endregion KILL EXCEL PROCESS
}
