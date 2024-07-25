using ConflictAutomation.Models;

namespace ConflicAutomation.Tests.Builders;

internal class ListResearchSummaryBuilder
{
    private readonly List<ResearchSummary> _listResearchSummary;


    public ListResearchSummaryBuilder(List<ResearchSummary>? listResearchSummary = null)
    {
        if (listResearchSummary is null)
        {
            _listResearchSummary = [];
        }
        else
        {
            _listResearchSummary = listResearchSummary;
        }
    }


    public ListResearchSummaryBuilder Add(ResearchSummary rs)
    {
        ArgumentNullException.ThrowIfNull(rs);

        _listResearchSummary.Add(rs);

        return this;
    }


    public List<ResearchSummary> Build()
    {
        return _listResearchSummary;
    }
}
