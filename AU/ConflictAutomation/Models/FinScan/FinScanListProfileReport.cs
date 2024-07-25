using ConflictAutomation.Extensions;
using ConflictAutomation.Models.FinScan.SubClasses;

namespace ConflictAutomation.Models.FinScan;

public class FinScanListProfileReport(Func<SearchMatch, bool> searchMatchfilter, int searchMatchesThreshold = 99)
{
    private readonly Func<SearchMatch, bool> _searchMatchfilter = searchMatchfilter;
    private readonly int _searchMatchesThreshold = searchMatchesThreshold;
    
    public List<string> NamesSearched { get; set; } = [];
    public List<FinScanListProfileReportItem> Items { get; private set; } = [];
    public List<SearchMatch> SearchMatches { get; private set; } = [];    

    
    public string ItemIndex(FinScanListProfileReportItem finScanListProfileReportItem) =>
        $"REPORT {Items.IndexOf(finScanListProfileReportItem) + 1} OF {Items.Count}";


    public void AddReport(FinScanListProfileReport additionalListProfileReport)
    {
        if (additionalListProfileReport is null)
        {
            return;
        }

        NamesSearched.AddRange(additionalListProfileReport.NamesSearched);
        NamesSearched = NamesSearched.Distinct().ToList();

        Items.AddRange(additionalListProfileReport.Items);
        Items = Items.DistinctBy(item => $"{item.ListId}::{item.Uid}").ToList();

        SearchMatches.AddRange(additionalListProfileReport.SearchMatches);
        SearchMatches = SearchMatches.DistinctBy(
            searchMatch => $"{searchMatch.listId}::{searchMatch.listProfileId}").ToList();
    }


    public bool AppendItemsFrom(string searchTerm, FinScanResponse finScanResponse)
    {
        bool result = false;
        
        if (finScanResponse.NoResult())
        {
            return result;
        }

        foreach (var searchResult in finScanResponse.searchResults)
        {
            if (searchResult.searchMatches.IsNullOrEmpty())
            {
                return result;
            }

            var searchMatches = searchResult.searchMatches.Where(_searchMatchfilter).ToList();

            foreach (var searchMatch in searchMatches)
            {
                if (Items.Count == _searchMatchesThreshold)
                {
                    break;
                }

                var finScanListProfileReportItem = finScanResponse.MakeFinScanListProfileReportItem(searchMatch); 
                if (finScanListProfileReportItem is null)
                {
                    continue;
                }

                AddReportItem(searchTerm, finScanListProfileReportItem, searchMatch);
            }
        }

        return result;
    }


    private void AddReportItem(string nameSearched, FinScanListProfileReportItem finScanListProfileReportItem, SearchMatch searchMatch)
    {
        NamesSearched.Add(nameSearched);
        NamesSearched = NamesSearched.Distinct().ToList();

        Items.AppendNewObjectIfInexistent<FinScanListProfileReportItem>(finScanListProfileReportItem);
        SearchMatches.AppendNewObjectIfInexistent<SearchMatch>(searchMatch);
    }
}
