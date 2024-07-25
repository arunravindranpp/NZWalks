using ConflictAutomation.Models.FinScan.SubClasses.enums;

namespace ConflictAutomation.Services.FinScan;

[Serializable]
public class FinScanException : Exception
{
    public ResultTypeEnum FinScanStatus { get; private set; }
    public int FinScanCode { get; private set; }
    public string FinScanMessage { get; private set; }

    public FinScanException(ResultTypeEnum status, int code, string finScanMessage, string message) : base(message)
    {
        FinScanStatus = status;
        FinScanCode = code;
        FinScanMessage = finScanMessage;
    }

    public FinScanException(ResultTypeEnum status, int code, string finScanMessage , string message, Exception innerException) : base(message, innerException)
    {
        FinScanStatus = status;
        FinScanCode = code;
        FinScanMessage = finScanMessage;
    }
}