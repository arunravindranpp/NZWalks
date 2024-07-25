using System.Diagnostics.Eventing.Reader;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConflictAutomation.Models
{
    public class CaseCreationLogModel
    {
        public long ID { get; set; }
        public long ConflictCheckID { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string SLName { get; set; }
        public string CheckCategory { get; set; }
        public int NoOfEntities { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }     
        public bool IsErrored { get; set; }
        public string Environment { get; set; }       
    }
}
