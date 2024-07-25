using ConflictAutomation.Extensions;
using ConflictAutomation.Models.FinScan;
using ConflictAutomation.Models.FinScan.SubClasses;
using ConflictAutomation.Models.FinScan.SubClasses.enums;

namespace ConflictAutomation.Services.FinScan;

#pragma warning disable IDE0305 // Simplify collection initialization

public class FinScanParser(FinScanRequest finScanRequest, List<SearchMatch> finScanSearchMatches)
{
    private FinScanRequest Request { get; init; } = finScanRequest;
    private List<SearchMatch> SearchMatches { get; init; } = finScanSearchMatches;

    private static readonly string SanctionsToConsider = 
        Program.KeyValuePairs.GetValue("FINSCAN_SANCTIONS_TO_CONSIDER", 
                                       defaultValue: FinScanConstants.Description2_SANCTIONS_LISTS);

    private static readonly List<string> ListSanctionsToConsider = SanctionsToConsider.Split('|').ToList();


    public FinScanMatchReport GetMatchReport() =>
        new()
        {
            User = Request.userName,
            Organization = Request.organizationName,
            CreatedDateTime = DateTime.Now,
            ApplicationName = Request.applicationId,  // How to convert from Id to Name?
            SearchType = Request.IndividualOrEntity() switch
            {
                SearchTypeEnum.Organization => "Organization",
                SearchTypeEnum.Individual => "Individual",
                _ => string.Empty
            },
            ClientSearchCode = Request.clientSearchCode switch
            {
                ClientSearchCodeEnum.FullName => "Full Name",
                ClientSearchCodeEnum.PartialOnly => "Partial Only",
                _ => string.Empty
            },
            Name = Request.nameLine,
            ClientId = Request.clientId,
            Address = string.Join("\n",
                                  [Request.addressLine1,
                                      Request.addressLine2,
                                      Request.addressLine3,
                                      Request.addressLine4,
                                      Request.addressLine5,
                                      Request.addressLine6,
                                      Request.addressLine7]).RemovePrefixes("\n"),
            Notes = Request.comment ?? string.Empty,

            ListCategories = ListCategories(),

            ListFullResultSet = GetListFullResultSet()
        };


    private List<Category> ListCategories() =>
        Request.lists.SelectMany(list =>
            list.listCategories.Select(category =>
                new Category
                {
                    ListName = list.listName,
                    CategoryName = category.categoryName
                }
            )
        ).ToList();

    
    private List<MatchReportResult> GetListFullResultSet() =>
        SearchMatches.Select(searchMatch =>
            new MatchReportResult
            {
                ListName = searchMatch.listName,
                ListProfileId = searchMatch.listProfileId,
                ClientNameAndAddress = searchMatch.displayLine,
                Country = searchMatch.dynamicFields.listCountries,
                Version = searchMatch.version,
                MatchString = searchMatch.rankString,
                RecordType = searchMatch.recordType.Equals("O", StringComparison.OrdinalIgnoreCase) ? "E" : searchMatch.recordType
            }).ToList();


    public bool? AtLeastOneSearchMatchIsSanctioned(out List<string> additionalKeywords)
    {
        bool? result = false;

        if ((Request is null) || SearchMatches.IsNullOrEmpty())
        {
            additionalKeywords = null;
            return result;
        }

        additionalKeywords = [];
        foreach (var searchMatch in SearchMatches)
        {
            bool? searchMatchIsSanctioned = SearchMatchIsSanctioned(searchMatch, out List<string> moreAdditionalKeywords);

            // Makes result true if at least one searchMatch is sanctioned.
            if (searchMatchIsSanctioned.HasValue && searchMatchIsSanctioned.Value)
            {
                result = true;
            }

            // The loop CANNOT be stopped as soon as a sanctioned searchMatch is found because 
            // each sanction checking can lead to further FinScan searches (for Parent|Employer companies).
            if (!moreAdditionalKeywords.IsNullOrEmpty())
            {
                additionalKeywords.AddRange(moreAdditionalKeywords);
                additionalKeywords = additionalKeywords.Distinct().ToList();
            }
        }
        return result;
    }


    private bool? SearchMatchIsSanctioned(SearchMatch searchMatch, out List<string> moreAdditionalKeywords)
    {
        bool? result = null;
        moreAdditionalKeywords = null;

        if (searchMatch is null)
        {
            return result;
        }


        switch (searchMatch.listId.ToUpper())
        {
            case FinScanConstants.LISTID_DJWL:
                result = SearchMatchIsSanctionedDJWL(searchMatch, out List<string> moreAdditionalKeywordsDJWL);
                if (!moreAdditionalKeywordsDJWL.IsNullOrEmpty())
                {
                    moreAdditionalKeywords = [];
                    moreAdditionalKeywords.AppendNewElementsOnly(moreAdditionalKeywordsDJWL);
                }
                break;

            case FinScanConstants.LISTID_DJSOC:
                result = SearchMatchIsSanctionedDJSOC(searchMatch);
                break;

            case FinScanConstants.LISTID_KH50:
            case FinScanConstants.LISTID_KHCO:
                result = SearchMatchIsSanctionedKharonFeed(searchMatch);
                break;

            default:
                break;
        };

        return result;
    }


    // User Story #971638/Step 3 - searchMatch identified as Dow Jones Watch List (DJWL)
    private bool? SearchMatchIsSanctionedDJWL(SearchMatch searchMatch, out List<string> moreAdditionalKeywords)
    {
        bool? result = null;
        moreAdditionalKeywords = null;


        var dowJonesListRecord = searchMatch.GetDowJonesListRecord();
        if (dowJonesListRecord is null)
        {
            return result;
        }

        var dowJonesEntityOrPerson = Request.GetDowJonesEntityOrPerson(dowJonesListRecord);
        if (dowJonesEntityOrPerson is null)
        {
            return result;
        }


        if (SanctionExistsFor(dowJonesEntityOrPerson))
        {
            // User Story #971638/Step 3/Item 1
            // SanctionsReferences has at least one item with Description2 as 'Sanctions List'

            if (PermanentSanctionExistsFor(dowJonesEntityOrPerson))
            {
                // User Story #971638/Step 3/Item 1a                
                result = true;
                return result;
            }
            else if (StillEffectiveSanctionExistsFor(dowJonesEntityOrPerson))
            {
                // User Story #971638/Step 3/Item 1b
                result = true;
                return result;
            }
        }

        // User Story #971638/Step 3/Item 2 (mistakenly written in Story as a 2nd occurrence of 'Item 1')
        // Sanctions tab either:
        // - Is unavailable (null);
        // - Is available but empty;
        // - Does not contain any Sanction in effect.

        Associate parentCompanyOrEmployerCompany = GetParentCompanyOrEmployerCompany(
                                                     dowJonesListRecord, out string parentRelationship);
        if (parentCompanyOrEmployerCompany is null)
        {
            // User Story #971638/Step 3/Item 2c - There is no Parent|Employer company.
            result = false;
            moreAdditionalKeywords = null;
            return result;
        }

        // User Story #971638/Step 3/Items 2a|2b - There is a Parent|Employer company.

        // Yet an Entity|Individual may not be sanctioned, the corresponding Parent|Employer company
        // may possibly be sanctioned. This is why the Parent|Employer company must checked as well.
        // So, perform another FinScanSearch for the keywords of the Parent|Employer company.
        // This can be done by adding the Parent|Employer keywords to the list of keywords
        // to be looped through in method FinScanOperations.ProcessFinScanCheckForMultipleKeywords().
        List<string> parentCompanyOrEmployerCompanyKeywords =
            GetParentCompanyOrEmployerCompanyKeywords(parentCompanyOrEmployerCompany, parentRelationship);
        if (!parentCompanyOrEmployerCompanyKeywords.IsNullOrEmpty())
        {
            moreAdditionalKeywords = [];
            moreAdditionalKeywords.AddRange(parentCompanyOrEmployerCompanyKeywords);
        }

        return result;
    }


    // User Story #971638/Step 4 - searchMatch identified as Dow Jones State Owned Companies (DJSOC)
    public static bool? SearchMatchIsSanctionedDJSOC(SearchMatch searchMatch)
    {
        bool? result = null;


        var innovativeListRecord = searchMatch.GetInnovativeListRecord();
        if (innovativeListRecord is null)
        {
            return result;
        }

        var nameAddressRecord = innovativeListRecord.nameAddressRecord;
        if (nameAddressRecord is null)
        {
            return result;
        }


        if (!NameAddressRecordIsActive(nameAddressRecord))
        {
            result = false;
            return result;
        }

        switch (searchMatch.recordType)
        {
            case FinScanConstants.MSG_FLAG_ENTITY:
                result = ActiveSearchMatchIsSanctionedDJSOCEntity(searchMatch);
                return result;

            case FinScanConstants.MSG_FLAG_INDIVIDUAL:
                result = true;
                return result;

            default:
                break;
        }

        return result;
    }


    private static bool NameAddressRecordIsActive(NameAddressRecordType nameAddressRecord) => 
        nameAddressRecord.active.FullTrim().Equals(FinScanConstants.MSG_YES, StringComparison.OrdinalIgnoreCase);


    private static bool? ActiveSearchMatchIsSanctionedDJSOCEntity(SearchMatch searchMatch)
    {
        bool? result = null;

        var dynamicFields = searchMatch.dynamicFields;
        if (dynamicFields is null)
        {
            return result;
        }

        string listCountriesAsSemicolonSeparatedString = dynamicFields.listCountries;
        if (listCountriesAsSemicolonSeparatedString.IsNullOrEmpty())
        {
            return result;
        }

        List<string> listCountries = listCountriesAsSemicolonSeparatedString
                                        .Split(";")
                                        .Select(country => country.FullTrim())
                                        .ToList();
        if (listCountries.Contains(FinScanConstants.SANCTIONED_COUNTRY_RUSSIA)
            || listCountries.Contains(FinScanConstants.SANCTIONED_COUNTRY_BELARUS))
        {
            result = true;
            return result;
        }

        result = false;
        return result;
    }


    // User Story #971638/Step 5 - searchMatch identified as Kharon feed (KH50|KHCO)
    public static bool? SearchMatchIsSanctionedKharonFeed(SearchMatch searchMatch)
    {
        bool? result = null;


        var innovativeListRecord = searchMatch.GetInnovativeListRecord();
        if (innovativeListRecord is null)
        {
            return result;
        }

        var nameAddressRecord = innovativeListRecord.nameAddressRecord;
        if (nameAddressRecord is null)
        {
            return result;
        }


        result = NameAddressRecordIsActive(nameAddressRecord);
        return result;
    }


    private static bool SanctionExistsFor(DowJonesEntityOrPerson dowJonesEntityOrPerson) =>
        (!dowJonesEntityOrPerson.SanctionsReferences.IsNullOrEmpty())
        && dowJonesEntityOrPerson.SanctionsReferences.Any(IsSanctioned);


    private static bool IsSanctioned(SanctionReference sanctionReference) =>
        sanctionReference.Description2.IsMemberOf(ListSanctionsToConsider, StringComparison.OrdinalIgnoreCase);


    private static bool PermanentSanctionExistsFor(DowJonesEntityOrPerson dowJonesEntityOrPerson) =>
        dowJonesEntityOrPerson.SanctionsReferences.Any(sanction => IsPermanentSanction(sanction));


    private static bool StillEffectiveSanctionExistsFor(DowJonesEntityOrPerson dowJonesEntityOrPerson) =>
        dowJonesEntityOrPerson.SanctionsReferences.Any(sanction => IsSanctionStillInEffect(sanction));


    private Associate GetParentCompanyOrEmployerCompany
        (DowJonesListRecord dowJonesListRecord, out string parentRelationship)
    {
        string parentRelationshipAux = ParentRelationship(Request.IndividualOrEntity());
        parentRelationship = parentRelationshipAux;
        return dowJonesListRecord.Associations
                .FirstOrDefault(association =>
                                association.relationship.Equals(
                                    parentRelationshipAux, StringComparison.OrdinalIgnoreCase));
    }


    private static List<string> GetParentCompanyOrEmployerCompanyKeywords
        (Associate parentCompanyOrEmployer, string parentRelationship) =>
            Program.KeywordGeneratorForEntities
                .GenerateKey(parentCompanyOrEmployer.singleStringName)
                .Select(keyword =>
                    $"{parentRelationship}{FinScanConstants.ParentRelationship_SEPARATOR}{keyword}")
                .ToList();


    private static bool IsPermanentSanction(SanctionReference sanctionReference) =>
        sanctionReference.ToDay.Equals(string.Empty)
        && sanctionReference.ToMonth.Equals(string.Empty)
        && sanctionReference.ToYear.Equals(string.Empty);


    private static bool IsSanctionStillInEffect(SanctionReference sanctionReference)
    {
        DateTime toDate;
        try
        {
            toDate = new DateTime(sanctionReference.ToDay.TryParseIntWithDefault(),
                                  sanctionReference.ToMonth.MonthAbbrev3ToNumber(),
                                  sanctionReference.ToYear.TryParseIntWithDefault());
        }
        catch
        {
            return false;
        }

        return (DateTime.UtcNow.Date <= toDate.Date);
    }


    private static string ParentRelationship(SearchTypeEnum individualOrEntity) =>
        individualOrEntity switch
        {
            SearchTypeEnum.Organization => FinScanConstants.ParentRelationship_PARENT_COMPANY,
            SearchTypeEnum.Individual => FinScanConstants.ParentRelationship_EMPLOYER,
            _ => string.Empty,
        };
}

#pragma warning restore IDE0305 // Simplify collection initialization
