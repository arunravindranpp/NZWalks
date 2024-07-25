#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0028 // Simplify collection initialization

using ConflictAutomation.Models.FinScan;
using ConflictAutomation.Models.FinScan.SubClasses;
using ConflictAutomation.Models.FinScan.SubClasses.enums;
using Nest;

namespace ConflictAutomation.Services.FinScan;

public class FinScanRequestFactory(
                    string organizationName,
                    string userName,
                    string password,
                    string applicationId
                )
{
    public FinScanRequest MakeRequest(
        SearchTypeEnum individualOrEntity, 
        ClientSearchCodeEnum fullOrPartialName, 
        string clientId, 
        string searchTerm, 
        string country)
    {
        // Grant F asked to do FinScan API searches with no country.
        _ = country;  
        string countryForAPISearch = string.Empty;


        FinScanRequest request = new()
        {
            organizationName = organizationName,
            userName = userName,
            password = password,
            applicationId = applicationId,

            lists = new List<ComplianceList>() {
                        new ComplianceList() {
                            listName = "Dow Jones WatchList",
                            listId = "DJWL",
                            listCategories = new List<ComplianceListCategory>() {
                                                new ComplianceListCategory() {
                                                    categoryName = "DJ_Sanction_ENTITY",
                                                    isSelected = (int)YesNoEnum.Yes,
                                                    isMandatory = (int)YesNoEnum.No,
                                                    isFullSource = (int)YesNoEnum.No
                                                },
                                                new ComplianceListCategory() {
                                                    categoryName = "DJ_Sanction_INDIVIDUAL",
                                                    isSelected = (int)YesNoEnum.Yes,
                                                    isMandatory = (int)YesNoEnum.No,
                                                    isFullSource = (int)YesNoEnum.No
                                                }
                            },
                            listType = 0
                        },
                        new ComplianceList() {
                            listName = "KH50",
                            listId = "KH50",
                            listCategories = new List<ComplianceListCategory>() {
                                                new ComplianceListCategory() {
                                                    categoryName = "Full Source",
                                                    isSelected = (int)YesNoEnum.Yes,
                                                    isMandatory = (int)YesNoEnum.No,
                                                    isFullSource = (int)YesNoEnum.No
                                                }
                            },
                            listType = 0
                        },
                        new ComplianceList() {
                            listName = "KHCO",
                            listId = "KHCO",
                            listCategories = new List<ComplianceListCategory>() {
                                                new ComplianceListCategory() {
                                                    categoryName = "Full Source",
                                                    isSelected = (int)YesNoEnum.Yes,
                                                    isMandatory = (int)YesNoEnum.No,
                                                    isFullSource = (int)YesNoEnum.No
                                                }
                            },
                            listType = 0
                        },
                        new ComplianceList() {
                            listName = "Dow Jones State Owned Companies List",
                            listId = "DJSOC",
                            listCategories = new List<ComplianceListCategory>() {
                                                new ComplianceListCategory() {
                                                    categoryName = "DJSOC::ENT_RUS_BLR",
                                                    isSelected = (int)YesNoEnum.Yes,
                                                    isMandatory = (int)YesNoEnum.No,
                                                    isFullSource = (int)YesNoEnum.No
                                                },
                                                new ComplianceListCategory() {
                                                    categoryName = "DJSOC::IND_RUS_BLR",
                                                    isSelected = (int)YesNoEnum.Yes,
                                                    isMandatory = (int)YesNoEnum.No,
                                                    isFullSource = (int)YesNoEnum.No
                                                }
                            },
                            listType = 0
                        }
            },

            searchType = individualOrEntity,
            clientId = clientId,
            clientStatus = ClientStatusEnum.Active, 
            gender = GenderEnum.Unknown,            
            nameLine = searchTerm,
            alternateNames = null,
            addressLine1 = string.Empty,
            addressLine2 = string.Empty,
            addressLine3 = string.Empty,
            addressLine4 = string.Empty,
            addressLine5 = string.Empty,
            addressLine6 = string.Empty,
            addressLine7 = null,
            specificElement = null,
            adverseMediaRequested = YesNoEnum.No,
            clientSearchCode = fullOrPartialName,
            userFieldsSearch = null,
            userField1Label = "Country",
            userField1Value = countryForAPISearch,
            userField2Label = "DUNS Number",            
            userField2Value = string.Empty,
            userField3Label = "Date Of Birth",
            userField3Value = string.Empty,
            userField4Label = "City",
            userField4Value = string.Empty,
            userField5Label = "Research scope",
            userField5Value = string.Empty,
            comment = null,
            passthrough = null,
            customStatus = null,

            options = new RequestOptions() {
                addClient = YesNoEnum.No,
                sendToReview = YesNoEnum.No,
                returnCategory = YesNoEnum.Yes,
                returnSearchComplianceRecords = YesNoEnum.Yes,
                returnComplianceRecords = YesNoEnum.Yes,
                returnSourceLists = YesNoEnum.Yes,
                returnSearchDetails = YesNoEnum.Yes,
                generateclientId = YesNoEnum.No,
                updateUserFields = YesNoEnum.Yes,
                skipSearch = YesNoEnum.No,
                processUBO = YesNoEnum.No,
                searchUBOMembers = YesNoEnum.No,
                skipClientUpdate = YesNoEnum.No
            }, 

            UBO_Id = null,
            userFieldsCountry = [],
            generateSearchReports = SearchReportTypeEnum.None,
            reportTypeCode = SearchReportTypeCodeEnum.PDF,
            premiumFields = null
        };

        return request;
    }
}
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0090 // Use 'new(...)'
