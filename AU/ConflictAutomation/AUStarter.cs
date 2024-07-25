namespace ConflictAutomation
{
    public class AUStarter
    {
        public bool ProcessCheckWithouteNate { get; set; }
        public bool CaseCreation { get; set; }
        public bool ProcesseNateRequest { get; set; }
        public bool ReseteNatePassword { get; set; }
        public bool AddeNateCaseToQueue { get; set; }
        public bool DeleteOldFiles { get; set; }
        public bool CopyFiles { get; set; }
        public bool SaveToExcel { get; set; }

        public AUStarter()
        {
            ProcessCheckWithouteNate = false;
            CaseCreation = false;
            ProcesseNateRequest = false;
            ReseteNatePassword = false;
            DeleteOldFiles =false;
            CopyFiles =false;
            SaveToExcel=false;
        }
    }
}
