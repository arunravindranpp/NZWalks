using Microsoft.Extensions.Configuration;

namespace ConflictAutomation.Models
{
    public class AppConfigure
    {
        public string ConnectionString { get; set; }
        public string GISConnectionString { get; set; }
        public string logpath { get; set; }
        public string sourceFilePath { get; set; }
        public string destinationFilePath { get; set; }
        public string onshoreContact { get; set; }
        public string onshoreContactLocalPath { get; set; }
        public string SPLPath { get; set; }
        public string SPLLocalPath { get; set; }
        public string RegionalPath { get; set; }
        public string RegionalLocalPath { get; set; }
        public string Environment { get; set; }
        //DEV purpose
        public string DEVLookupTable { get; set; }
        public string InputFilePath { get; set; }

        public string ReworkFilePath {  get; set; }
        
        public IConfigurationRoot configurationRoot { get; set; }
        public bool IsTestMode { get; set; }
        public string TestModeUserName { get; set; }
        public string TestModePassword { get; set; }
        public bool IsProtoType{ get; set; }
        public bool IsGUPLaw { get; set; }
        public bool IsWorkQueue { get; set; }
        public string FileSharePath { get; set; }
        public bool SaveToFileShare { get; set; }
        public string LastCheckFromBOTs { get; set; }
        public string eNateUserName { get; set; }
        public string eNatePassword { get; set; }
        public DateTime eNatePwdLastUpdate { get; set; }
        public string MaximumDaysForFiles { get; set; }
        public bool IsManualRun { get; set; }
    }
}
