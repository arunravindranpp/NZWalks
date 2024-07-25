using System.Data;

namespace ConflictAutomation.Models
{
    public class CRREntity
    {
        public List<CRRFilter> Result { get; set; }
        public List<ColumnFilter> ColumnFilter { get; set; }
        public RPTRequest RPTRequest { get; set; }
        public bool IsreportNameChanged { get; set; }
        public CommonFilter CommonFilter { get; set; }
        public string RunOption { get; set; }
        public string PrimarySearchTerm { get; set; }
        public string PrimarySearchFilter { get; set; }
    }

    public class CRRFilter
    {
        public string FilterName { get; set; }
        public string FilterType { get; set; }
        public string FilterOperator { get; set; }
        public string FilterValue1 { get; set; }
        public string FilterValue2 { get; set; }
        public List<string> ServiceLine { get; set; }
        public List<string> SubServiceLine { get; set; }
        public List<string> StatusOfConflict { get; set; }
        public List<string> StatusOfEngagement { get; set; }
        public List<string> AssessmentType { get; set; }
        public List<string> CheckTypes { get; set; }
        public List<string> APGRoles { get; set; }
    }

    public class ColumnFilter
    {
        public int LookUpID { get; set; }
        public string Category { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefaultCategory { get; set; }
        public string TableAlias { get; set; }
        public string ColumnPrettyName { get; set; }
        public string DateFormat { get; set; }
    }

    public class RPTRequest
    {
        public int ReportId { get; set; }
        public int doCopy { get; set; }
        public string ReportDesc { get; set; }
        public string ReportName { get; set; }
        public int RequestId { get; set; }
        public string Name { get; set; }
        public string reportType { get; set; }
        public string ReportDescription { get; set; }
    }

    public class CommonFilter
    {
        public string FilterName { get; set; }
        public string FilterOperator { get; set; }
        public bool IncludeSimilarMatches { get; set; }
        public bool IncludeAPG { get; set; }
        public string TabsMode { get; set; }
    }
    public class CRRResults
    {
        public string fileName { get; set; } = "";
        public bool isError { get; set; } = false;
        public DataSet crrDataset { get; set; } = new DataSet();
    }
}
