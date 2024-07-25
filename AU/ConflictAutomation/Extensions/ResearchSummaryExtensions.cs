using ConflictAutomation.Constants;
using ConflictAutomation.Models;

namespace ConflictAutomation.Extensions;

public static class ResearchSummaryExtensions
{
    public static ResearchSummary GetMainClientItem(this List<ResearchSummary> listResearchSummary) =>
        listResearchSummary
            .FirstOrDefault(rs => rs.OrderNo.Equals(1));

    public static ResearchSummary GetItemByTabName(this List<ResearchSummary> listResearchSummary, string tabName) =>
        listResearchSummary.FirstOrDefault(rs => rs.WorksheetNo.Equals(tabName, StringComparison.OrdinalIgnoreCase));


    public static List<ResearchSummary> GetNonClientSideItems(this List<ResearchSummary> listResearchSummary) =>
        listResearchSummary.Where(rs => rs.IsNonClientSide()).ToList();

    public static List<ResearchSummary> GetClientSideItems(this List<ResearchSummary> listResearchSummary) =>
        listResearchSummary.Where(rs => rs.IsClientSide()).ToList();


    private static bool IsNonClientSide(this ResearchSummary rs) => (rs is not null) && (!rs.IsClientSide);

    private static bool IsClientSide(this ResearchSummary rs) => (rs is not null) && (rs.IsClientSide);


    public static string MakeClientRelationshipSummaryText(this ResearchSummary rs)
    {
        if (rs?.ClientRelationshipSummary is null)
        {
            return CAUConstants.MSG_NO_DATA;
        }


        List<string> contents;
        string showupDUNS = string.Empty;

        if (string.IsNullOrWhiteSpace(rs.ClientRelationshipSummary.SearchDesc))
        {
            List<string> listGisAndCerDescriptions = [rs.ClientRelationshipSummary.GISDesc?.FullTrim(),
                                                      rs.ClientRelationshipSummary.CERDesc?.FullTrim()];
            string gisAndCerDescriptions = string.Join(" | ",
                                             listGisAndCerDescriptions.Where(desc => !string.IsNullOrEmpty(desc)));

            string restrictions = string.IsNullOrWhiteSpace(rs.ClientRelationshipSummary.Restrictions?.FullTrim())
                                    ? "NA" : rs.ClientRelationshipSummary.Restrictions;

             showupDUNS = rs.ClientRelationshipSummary.Duns;

            if (string.IsNullOrEmpty(showupDUNS) || showupDUNS == "NA")
                showupDUNS = string.IsNullOrEmpty(rs.DUNSNumber) ? "NA" : rs.DUNSNumber;

            string showupLAP = string.IsNullOrEmpty(rs.ClientRelationshipSummary.LAP) ? "NA" : rs.ClientRelationshipSummary.LAP.Replace("-", "NA");

            string showupGCSP = string.IsNullOrEmpty(rs.ClientRelationshipSummary.GCSP) ? "NA" : rs.ClientRelationshipSummary.GCSP;
            contents = [
                            $"DUNS: {showupDUNS}",
                            gisAndCerDescriptions,
                            $"G/CSP: {showupGCSP}",
                            $"LAP: {showupLAP}",
                            $"Restriction(s): {restrictions}",
                            $"PIE: {rs.ClientRelationshipSummary.PIE} | " +
                                $"PIE Affiliate: {rs.ClientRelationshipSummary.PIEAffiliate}",
                            rs.ClientRelationshipSummary.GISNotes,
                            rs.ClientRelationshipSummary.CERNotes
                       ];
        }
        else
        {
            if (!string.IsNullOrEmpty(rs.DUNSNumber))
                showupDUNS = string.IsNullOrEmpty(rs.DUNSNumber) ? "NA" : rs.DUNSNumber;

            contents = [
                            rs.ClientRelationshipSummary.SearchDesc,
                            $"DUNS: {showupDUNS}",
                            "NA",
                            "G/CSP: NA",
                            "LAP: NA",
                            "Restriction(s): NA",
                            "PIE: NA | PIE Affiliate: NA"
                       ];
        }

        return string.Join(Environment.NewLine,
                           contents.Where(line => !string.IsNullOrWhiteSpace(line)));
    }


    public static void TurnFlagsOffWhenNoResearch(this ResearchSummary rs)
    {
        if ((!rs.PerformResearch) && (!rs.WorksheetNo.Contains(".A")))
        {
            rs.IsGISResearch = false;
            rs.IsCERResearch = false;
            rs.IsCRRResearch = false;
            rs.IsFinscanResearch = false;
            rs.IsSPLResearch = false;
            if (rs.GIS != "I")
            {
                rs.GIS = "X";
                rs.Mercury = "X";
                rs.Finscan = "X";
                rs.SPL = "X";
                rs.CRR = "X";
            }
        }
    }
}
