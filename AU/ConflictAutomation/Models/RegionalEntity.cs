
namespace ConflictAutomation.Models
{
    public class RegionalEntity
    {
        public List<RegionalDeviations> regionalDeviations  { get; set; }
        public List<SupplementaryGuide> supplementaryGuides { get; set; }
    }
    public class RegionalDeviations
    {
        public string Area { get; set; }
        public string Region { get; set; }
        public string WorkAllocationByGDS { get; set; }
        public string AdditionalLocalDB { get; set; }
        public string GCSPConsultaion { get; set; }
        public string SignoffGDS { get; set; }
        public string OtherNuances { get; set; }
    }
    public class SupplementaryGuide
    {
        public int SNo { get; set; }
        public string Area { get; set; }
        public string Region { get; set; }
        public string ServiceLine { get; set; }
        public string WorkAllocation { get; set; }
        public string PreScreening { get; set; }
        public string InfoRequestET { get; set; }
        public string Research { get; set; }
        public string Counterparty { get; set; }
        public string Conclusion { get; set; }
        public string Signoff { get; set; }
        public string SupportingDocuments { get; set; }
        public string ContactNames { get; set; }
    }
}
