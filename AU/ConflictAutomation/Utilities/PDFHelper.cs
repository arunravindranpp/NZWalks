using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;

namespace ConflictAutomation.Utilities;

public static class PDFHelper
{
    public static string CreatePDFFilefromHTML(
        string inputHtmlContents,
        string outputPdfFilePath,
        ConverterProperties converterProperties = null)
    {
        try
        {
            if (File.Exists(outputPdfFilePath))
            {
                File.Delete(outputPdfFilePath);
            }

            converterProperties ??= new ConverterProperties();
            converterProperties.SetFontProvider(new DefaultFontProvider(true, true, true));

            using FileStream pdfDest = File.Open(outputPdfFilePath, FileMode.OpenOrCreate);
            HtmlConverter.ConvertToPdf(inputHtmlContents, pdfDest, converterProperties);
        }
        catch (Exception)
        {
            throw;
        }

        string result = File.Exists(outputPdfFilePath) ? outputPdfFilePath : string.Empty;
        return result;
    }


    public static FileInfo CreatePDFFilefromHTML(
        FileInfo inputHtmlFileInfo,
        FileInfo outputPdfFileInfo,
        ConverterProperties converterProperties = null)
    {
        try
        {
            if (outputPdfFileInfo.Exists)
            {
                File.Delete(outputPdfFileInfo.FullName);
            }

            converterProperties ??= new ConverterProperties();
            converterProperties.SetFontProvider(new DefaultFontProvider(true, true, true));

            HtmlConverter.ConvertToPdf(inputHtmlFileInfo, outputPdfFileInfo, converterProperties);
        }
        catch (Exception)
        {
            throw;
        }

        return outputPdfFileInfo.Exists ? outputPdfFileInfo : null;
    }
}
