using PACE;

namespace ConflictAutomation.Models
{
    public class ProcessedChecks : ICheckInfo
    {
        public long LogID { get; set; }
        public string CheckId { get; set; }
        public string ProcessStart { get; set; }
        public string PACEExtractionEnd { get; set; }
        public string AUUnitGridStart { get; set; }
        public string EntityCount { get; set; }
        public string EntitiesList { get; set; }
        public string KeyGenCount { get; set; }
        public string KeyGenStart { get; set; }
        public string Keywords { get; set; }
        public string CRRStart { get; set; }
        public string GISStart { get; set; }
        public string MercuryStart { get; set; }
        public string FinscanStart { get; set; }
        public string SPLStart { get; set; }
        public string ProcessEnd { get; set; }
        public bool IsErrored { get; set; }
        public string ErrorMessage { get; set; }
        public string Environment { get; set; }
        public ConflictCheck conflictCheck { get; set; }
        public string ReWork { get; set; }
        public bool MultiEntity { get; set; }
    }
}
