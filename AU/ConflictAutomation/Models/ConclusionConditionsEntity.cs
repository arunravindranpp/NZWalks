namespace ConflictAutomation.Models
{
    public class ConclusionConditionsEntity
    {
        public int Id { get; set; }
        public int Cases { get; set; }
        public string Condition1 { get; set; }
        public string Condition2 { get; set; }
        public string ConflictConclusion { get; set; }
        public string DisclaimerIndependence { get; set; }
        public string RationaleInstructions { get; set; }
    }
}
