using ConflictAutomation.Models.FinScan.SubClasses;

namespace ConflictAutomation.Services.FinScan;

public static class FinScanSearchMatchFilter
{ 
    public static bool ByRankScore(SearchMatch searchMatch) => 
        (Program.FinScanMinRankScore <= searchMatch.rankScore)
        && (searchMatch.rankScore <= Program.FinScanMaxRankScore);
}
