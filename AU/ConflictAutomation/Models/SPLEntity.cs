
namespace ConflictAutomation.Models
{
    public class SPLEntity
    {
        public string vesionDetails {  get; set; }
        public List<SPLList> splLists { get; set; }
        public List<CommercialSensitivitiesList> commercialSensitivitiesLists { get; set; }
    }
    public class SPLList
    {
        public int SNo { get; set; }
        public string Area { get; set; }
        public string Region { get; set; }
        public string Category { get; set; }
        public string Entity { get; set; }
        public string SPLInstructions { get; set; }
        public string GUP { get; set; }
        public string Attachment { get; set; }
        public string ContactPerson { get; set; }
        public string Onshore { get; set; }
        public string DateOfEntry { get; set; }
        public string TentativeDateOfExpiry { get; set; }
        public string ValidityStatus { get; set; }
        public string LastFollowUpDate { get; set; }
        public string FollowUpComments { get; set; }
        public string Requestor { get; set; }
    }
    public class CommercialSensitivitiesList
    {
        public int SNo { get; set; }
        public string Area { get; set; }
        public string Region { get; set; }
        public string Category { get; set; }
        public string Entity { get; set; }
        public string SPLInstructions { get; set; }
        public string Attachment { get; set; }
        public string ContactPerson { get; set; }
        public string Onshore { get; set; }
        public string DateOfEntry { get; set; }
        public string TentativeDateOfExpiry { get; set; }
        public string ValidityStatus { get; set; }
        public string LastFollowUpDate { get; set; }
        public string FollowUpComments { get; set; }
        public string Requestor { get; set; }
    }
}
