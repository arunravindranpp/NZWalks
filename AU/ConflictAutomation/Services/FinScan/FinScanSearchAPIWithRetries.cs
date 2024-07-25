using ConflictAutomation.Extensions;
using ConflictAutomation.Models.FinScan;
using ConflictAutomation.Utilities;
using Newtonsoft.Json;

namespace ConflictAutomation.Services.FinScan;

public class FinScanSearchAPIWithRetries
{
    private Action<Exception, string> _logAction { get; init; }
    private string _url { get; init; }
    private int _maxTries { get; init; }
    private int _millisecondsBetweenRetries { get; init; }

    private readonly string _clientIdPrefix = "AU" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    private int _clientIdLastSuffix = 0;


    public FinScanSearchAPIWithRetries(string url, Action<Exception, string> logAction = null, int maxTries = 3, int millisecondsBetweenRetries = 10000)
    {
        if (maxTries < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTries), $"FinScanSearchAPIWithRetries.ctor: {nameof(maxTries)} argument must be greater than zero");
        }

        if (millisecondsBetweenRetries < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(millisecondsBetweenRetries), $"FinScanSearchAPIWithRetries.ctor: {nameof(millisecondsBetweenRetries)} argument must be greater than zero");
        }

        _url = url;
        _logAction = logAction;
        _maxTries = maxTries;
        _millisecondsBetweenRetries = millisecondsBetweenRetries;
    }


    public FinScanResponse Execute(FinScanRequest finScanRequest, string additionalInfoOnError)
    {
        int triesLeft = _maxTries;
        string responseBody = string.Empty;

        while (triesLeft > 0)
        {
            try
            {
                responseBody = HttpClientReq.Post(_url, finScanRequest)!;
                break;
            }
            catch (Exception ex)
            {
                if (--triesLeft < 1)
                {
                    throw new Exception($"FinScanSearchAPI.Execute() failed {additionalInfoOnError}".FullTrim(), ex);
                }
            }

            Thread.Sleep(_millisecondsBetweenRetries);
            if(_logAction != null)
            {
                var retryMsg = $"FinScanSearchAPI call - Retry #{_maxTries - triesLeft} {additionalInfoOnError}".FullTrim();
                _logAction(new Exception(retryMsg), retryMsg);
            }         
        }

        return JsonConvert.DeserializeObject<FinScanResponse>(responseBody)!;
    }


    public string GetNextClientId() => $"{_clientIdPrefix}{++_clientIdLastSuffix:D4}";    
}
