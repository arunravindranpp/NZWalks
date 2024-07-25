using ConflictAutomation.Services.ConclusionChecking.enums;

namespace ConflictAutomation.Models.ConclusionChecking;

public class Conclusion
{
    public ConclusionScenarioEnum Scenario { get; init; }

    public string Case { get; init; }
    public string Issue { get; init; }
    public string Outcome { get; init; }

    public string ConflictConclusion { get; init ; }
    public string DisclaimerIndependence { get; init; }
    public string RationaleInstructions { get; init; }
    public string ConditionDescription { get; init; }

    public bool? ResearchFailedInAnyOfTheDataSources { get; init; }

    public List<string> Attachments { get; init; }


    public string DetailsTemplateString() => 
        (Scenario == ConclusionScenarioEnum.Unidentified) ? string.Empty : 
            $"{ConflictConclusion}\n\n" + $"{DisclaimerIndependence}\n\n" + $"{RationaleInstructions}";
}
