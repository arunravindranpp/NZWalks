using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Models.PreScreening.SubClasses;

namespace ConflictAutomation.Mappers;

public class AdditionalPartyMapper
{
    private static AdditionalParty CreateFrom(QuestionnaireAdditionalParties paceAdditionalParty) =>
    new()
    {
        Name = paceAdditionalParty.Name,
        Position = paceAdditionalParty.Position,
        OtherInformation = paceAdditionalParty.OtherInformation
    };

    public static List<AdditionalParty> CreateFrom(List<QuestionnaireAdditionalParties> listAdditionalParties) =>
        listAdditionalParties.IsNullOrEmpty() ? [] : listAdditionalParties.Select(CreateFrom).ToList();
}
