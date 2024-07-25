using ConflicAutomation.Tests.Builders;
using ConflictAutomation.Constants;
using ConflictAutomation.Models;
using ConflictAutomation.Services.ConclusionChecking;
using ConflictAutomation.Services.ConclusionChecking.enums;
using FluentAssertions;

namespace ConflicAutomation.Tests;

public class ConclusionCheckerTests
{
    private readonly ConclusionChecker _conclusionCheckerA = MakeConclusionCheckerA();
    private readonly ConclusionChecker _conclusionCheckerB = MakeConclusionCheckerB();
    private readonly ConclusionChecker _conclusionCheckerC = MakeConclusionCheckerC();
    private readonly ConclusionChecker _conclusionCheckerD = MakeConclusionCheckerD();
    private readonly ConclusionChecker _conclusionCheckerE = MakeConclusionCheckerE();
    private readonly ConclusionChecker _conclusionCheckerF = MakeConclusionCheckerF();
    private readonly ConclusionChecker _conclusionCheckerG = MakeConclusionCheckerG();
    private readonly ConclusionChecker _conclusionCheckerH = MakeConclusionCheckerH();
    private readonly ConclusionChecker _conclusionCheckerI = MakeConclusionCheckerI();
    private readonly ConclusionChecker _conclusionCheckerJ = MakeConclusionCheckerJ();
    private readonly ConclusionChecker _conclusionCheckerK = MakeConclusionCheckerK();


    [Fact]
    public void ListResearchSummary_CaseA_ReturnTotalElements()
    {
        _conclusionCheckerA.ListResearchSummary.Count.Should().Be(8);
    }

    [Fact]
    public void ListResearchSummaryClientSide_StdCaseClients_ReturnClientsOnly()
    {
        List<ResearchSummary> list = _conclusionCheckerA.ListResearchSummaryClientSide;
        list.Count.Should().Be(4);
        list[0].SummaryRowNo.Should().Be(1);
        list[1].SummaryRowNo.Should().Be(2);
        list[2].SummaryRowNo.Should().Be(3);
        list[3].SummaryRowNo.Should().Be(4);
    }

    [Fact]
    public void ListResearchSummaryNonClientSide_StdCaseNoClients_ReturnNonClientsOnly()
    {
        List<ResearchSummary> list = _conclusionCheckerA.ListResearchSummaryNonClientSide;
        list.Count.Should().Be(4);
        list[0].SummaryRowNo.Should().Be(5);
        list[1].SummaryRowNo.Should().Be(6);
        list[2].SummaryRowNo.Should().Be(7);
        list[3].SummaryRowNo.Should().Be(8);
    }
    
    [Fact]
    public void ListResearchSummaryWithMultipleCloseMatchesAsPerGis_CaseI_ReturnMultCloseMatchesOnly()
    {
        List<ResearchSummary> list = _conclusionCheckerI.ListResearchSummaryWithMultipleCloseMatchesAsPerGis;
        list.Count.Should().Be(1);
        list[0].SummaryRowNo.Should().Be(3);
        _conclusionCheckerH.ThereIsAtLeastOneRecordWithMultipleCloseMatchesAsPerGis().Should().BeFalse();
        _conclusionCheckerI.ThereIsAtLeastOneRecordWithMultipleCloseMatchesAsPerGis().Should().BeTrue();
    }

    [Fact]
    public void ListResearchSummaryWithMultipleCloseMatchesAsPerGisOrCer_CaseA_ReturnMultCloseMatchesOnly()
    {
        List<ResearchSummary> list = _conclusionCheckerA.ListResearchSummaryWithMultipleCloseMatchesAsPerGisOrCer;
        list.Count.Should().Be(2);
        list[0].SummaryRowNo.Should().Be(4);
        list[1].SummaryRowNo.Should().Be(5);
        _conclusionCheckerA.ThereIsAtLeastOneRecordWithMultipleCloseMatchesAsPerGisOrCer().Should().BeTrue();
    }

    [Fact]
    public void ListResearchSummaryNonClientSideLackingUidAsPerGisOrCer_CaseA_ReturnNonClientSideLackingUidOnly()
    {
        List<ResearchSummary> list = _conclusionCheckerA.ListResearchSummaryNonClientSideLackingUidAsPerGisOrCer;
        list.Count.Should().Be(3);
        list[0].SummaryRowNo.Should().Be(5);
        list[1].SummaryRowNo.Should().Be(6);
        list[2].SummaryRowNo.Should().Be(7);
        _conclusionCheckerA.AllNonClientSideRecordsLackUidAsPerGisOrCer().Should().BeFalse();        
    }
    

    [Fact]
    public void NoEYRelationshipWithCounterparties_CasesAandB_ReturnNoEYRelationshipWithCounterparties()
    {
        _conclusionCheckerA.NoEYRelationshipWithCounterparties().Should().BeFalse();
        _conclusionCheckerB.NoEYRelationshipWithCounterparties().Should().BeTrue();
    }

    [Fact]
    public void ClientSideIsSanctioned_CaseA_ReturnClientSideWithSanctionsOnly()
    {
        List<ResearchSummary> list = _conclusionCheckerA.ListResearchSummaryClientSideWithSanctions;
        list.Count.Should().Be(1);
        list[0].SummaryRowNo.Should().Be(1);
        _conclusionCheckerA.ClientSideIsSanctioned().Should().BeTrue();
        _conclusionCheckerA.IsSanctioned().Should().BeTrue();
    }

    [Fact]
    public void NonClientSideIsSanctioned_CaseB_ReturnNonClientSideWithSanctionsOnly()
    {
        List<ResearchSummary> list = _conclusionCheckerB.ListResearchSummaryNonClientSideWithSanctions;
        list.Count.Should().Be(0);
        _conclusionCheckerB.NonClientSideIsSanctioned().Should().BeFalse();
        _conclusionCheckerB.IsSanctioned().Should().BeFalse();
    }

    [Fact]
    public void GetScenarioForNonClientSide_CaseC_ReturnScenario_1()
    {
        ConclusionScenarioEnum scenario = _conclusionCheckerC.GetScenarioForNonClientSide();
        scenario.Should().Be(ConclusionScenarioEnum.NoSanctions);
    }

    [Fact]
    public void GetScenarioForNonClientSide_CaseD_ReturnScenario_2()
    {
        ConclusionScenarioEnum scenario = _conclusionCheckerD.GetScenarioForNonClientSide();
        scenario.Should().Be(ConclusionScenarioEnum.ClientSideSanctions_MainRoles);
    }

    [Fact]
    public void GetScenarioForNonClientSide_CaseE_ReturnScenario_4()
    {
        ConclusionScenarioEnum scenario = _conclusionCheckerE.GetScenarioForNonClientSide();
        scenario.Should().Be(ConclusionScenarioEnum.NonClientSideSanctions);
    }

    [Fact]
    public void GetScenarioForClientSide_CaseI_ReturnScenario_0()
    {
        ConclusionScenarioEnum scenario = _conclusionCheckerI.GetScenarioForClientSide();
        scenario.Should().Be(ConclusionScenarioEnum.Unidentified);
    }

    [Fact]
    public void GetScenarioForClientSide_CaseH_ReturnScenario_1()
    {
        ConclusionScenarioEnum scenario = _conclusionCheckerH.GetScenarioForClientSide();
        scenario.Should().Be(ConclusionScenarioEnum.NoSanctions);
    }

    [Fact]
    public void GetScenarioForClientSide_CaseJ_ReturnScenario_2()
    {
        ConclusionScenarioEnum scenario = _conclusionCheckerJ.GetScenarioForClientSide();
        scenario.Should().Be(ConclusionScenarioEnum.ClientSideSanctions_MainRoles);
    }

    [Fact]
    public void GetScenarioForNonClientSide_CaseK_ReturnScenario_0()  // This corresponds to Issue #1022669
    {
        ConclusionScenarioEnum scenario = _conclusionCheckerK.GetScenarioForNonClientSide();
        scenario.Should().Be(ConclusionScenarioEnum.Unidentified);
    }

    [Fact]
    public void ResearchFailed_Any_CaseF_ReturnFalse()
    {
        bool researchFailedCRR = _conclusionCheckerF.ResearchFailed_CRR();
        researchFailedCRR.Should().BeFalse();

        bool researchFailedFinscan = _conclusionCheckerF.ResearchFailed_FinScan();
        researchFailedFinscan.Should().BeFalse();

        bool researchFailedGIS = _conclusionCheckerF.ResearchFailed_GIS();
        researchFailedGIS.Should().BeFalse();

        bool researchFailedMercury = _conclusionCheckerF.ResearchFailed_Mercury();
        researchFailedMercury.Should().BeFalse();

        bool researchFailedSPL = _conclusionCheckerF.ResearchFailed_SPL();
        researchFailedSPL.Should().BeFalse();

        bool researchFailedAny = _conclusionCheckerF.ResearchFailed_Any();
        researchFailedAny.Should().BeFalse();
    }

    [Fact]
    public void ResearchFailed_Any_CaseG_ReturnTrue()
    {
        bool researchFailedCRR = _conclusionCheckerG.ResearchFailed_CRR();
        researchFailedCRR.Should().BeFalse();

        bool researchFailedFinscan = _conclusionCheckerG.ResearchFailed_FinScan();
        researchFailedFinscan.Should().BeFalse();

        bool researchFailedGIS = _conclusionCheckerG.ResearchFailed_GIS();
        researchFailedGIS.Should().BeFalse();

        bool researchFailedMercury = _conclusionCheckerG.ResearchFailed_Mercury();
        researchFailedMercury.Should().BeTrue();

        bool researchFailedSPL = _conclusionCheckerG.ResearchFailed_SPL();
        researchFailedSPL.Should().BeFalse();

        bool researchFailedAny = _conclusionCheckerG.ResearchFailed_Any();
        researchFailedAny.Should().BeTrue();
    }


    private static ConclusionChecker MakeConclusionCheckerA()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    IsClientSide = true,
                    Sanctions = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "",
                        GISDesc = "",
                        CERNotes = "", 
                        CERDesc = ""
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    IsClientSide = true,
                    Sanctions = false,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "",
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 3,
                    IsClientSide = true,
                    Sanctions = false,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "",
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 4,
                    IsClientSide = true,
                    Sanctions = false,
                    ClientRelationshipSummary = new() 
                    { 
                        GISNotes = $" {CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES}  ",
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 5,
                    IsClientSide = false,
                    Sanctions = true,
                    ClientRelationshipSummary = new() 
                    {
                        GISNotes = "", 
                        GISDesc = $" {CAUConstants.GIS_ENTITY_UNDER_AUDIT__NO_RECORD_WITH_UID}  ", 
                        CERNotes = $" {CAUConstants.CER_MULTIPLE_CLOSE_MATCHES}  ", 
                        CERDesc = ""
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 6,
                    IsClientSide = false,
                    Sanctions = false,
                    ClientRelationshipSummary = new() 
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_ENTITY_NOT_UNDER_AUDIT__NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = $" {CAUConstants.CER_NO_RECORD_WITH_UID_1}  " 
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 7,
                    IsClientSide = false,
                    Sanctions = false,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = CAUConstants.CER_NO_RECORD_WITH_UID_2
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 8,
                    IsClientSide = false,
                    Sanctions = false,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "wwwww", 
                        GISDesc = "xxxxx", 
                        CERNotes = "yyyyy", 
                        CERDesc = "zzzzz"
                    }
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerB()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    IsClientSide = true,
                    Sanctions = false,
                    ClientRelationshipSummary = new() 
                    {
                        GISNotes = "wwwww",
                        GISDesc = "xxxxx",
                        CERNotes = "yyyyy",
                        CERDesc = "zzzzz"
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    IsClientSide = true,
                    Sanctions = false,
                    ClientRelationshipSummary = new() 
                    {
                        GISNotes = "aaaaa",
                        GISDesc = "bbbbb",
                        CERNotes = "ccccc",
                        CERDesc = "ddddd"
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 3,
                    IsClientSide = false,
                    Sanctions = false,
                    ClientRelationshipSummary = new() 
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = ""
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 4,
                    IsClientSide = false,
                    Sanctions = false,
                    ClientRelationshipSummary = new() 
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_ENTITY_UNDER_AUDIT__NO_RECORD_WITH_UID,
                        CERNotes = "", 
                        CERDesc = CAUConstants.CER_NO_RECORD_WITH_UID_1,
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 5,
                    IsClientSide = false,
                    Sanctions = false,
                    ClientRelationshipSummary = new() {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = ""
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 6,
                    IsClientSide = false,
                    Sanctions = false,
                    ClientRelationshipSummary = new() 
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_ENTITY_NOT_UNDER_AUDIT__NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = ""
                    }
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerC()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    IsClientSide = true,
                    Sanctions = false,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "wwwww",
                        GISDesc = "xxxxx",
                        CERNotes = "yyyyy",
                        CERDesc = "zzzzz"
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    IsClientSide = false,
                    Sanctions = false,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = ""
                    }
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerD()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    IsClientSide = true,
                    Sanctions = true,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "wwwww",
                        GISDesc = "xxxxx",
                        CERNotes = "yyyyy",
                        CERDesc = "zzzzz"
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    IsClientSide = false,
                    Sanctions = false,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = ""
                    }
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerE()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    IsClientSide = true,
                    Sanctions = false,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "wwwww",
                        GISDesc = "xxxxx",
                        CERNotes = "yyyyy",
                        CERDesc = "zzzzz"
                    }
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    IsClientSide = false,
                    Sanctions = true,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = ""
                    }
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerF()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    CRR = "C", 
                    Finscan = "C", 
                    GIS = "C",
                    Mercury = "C",
                    SPL = "C"
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    CRR = "C",
                    Finscan = "C",
                    GIS = "C",
                    Mercury = "C",
                    SPL = "C"
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 3,
                    CRR = "C",
                    Finscan = "C",
                    GIS = "C",
                    Mercury = "C",
                    SPL = "C"
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerG()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    CRR = "C",
                    Finscan = "C",
                    GIS = "C",
                    Mercury = "C",
                    SPL = "C"
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    CRR = "C",
                    Finscan = "C",
                    GIS = "C",
                    Mercury = "F",
                    SPL = "C"
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 3,
                    CRR = "C",
                    Finscan = "C",
                    GIS = "C",
                    Mercury = "C",
                    SPL = "C"
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerH()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1, 
                    IsClientSide = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "",
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    }, 
                    Sanctions = false
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    IsClientSide = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "", 
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    },
                    Sanctions = false
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 3,
                    IsClientSide = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "", 
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    },
                    Sanctions = false
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerI()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    IsClientSide = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "", 
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    },
                    Sanctions = false
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    IsClientSide = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "", 
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    },
                    Sanctions = false
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 3,
                    IsClientSide = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES, 
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    },
                    Sanctions = false
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerJ()
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    IsClientSide = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "", 
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    },
                    Sanctions = false
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    IsClientSide = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "", 
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    },
                    Sanctions = true
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 3,
                    IsClientSide = true,
                    ClientRelationshipSummary = new() { 
                        GISNotes = "", 
                        GISDesc = "",
                        CERNotes = "",
                        CERDesc = ""
                    },
                    Sanctions = false
                })
                .Build());
    }


    private static ConclusionChecker MakeConclusionCheckerK()  // This corresponds to Issue #1022669
    {
        ListResearchSummaryBuilder listResearchSummaryBuilder = new();

        return new(
            listResearchSummaryBuilder
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 1,
                    IsClientSide = false,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "", 
                        GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = $"{CAUConstants.NON_AUDIT_CLIENT} (As per CER)(<Assurance, Tax>)"

                    },
                    Sanctions = false
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 2,
                    IsClientSide = false,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = CAUConstants.CER_NO_RECORD_WITH_UID_1
                    },
                    Sanctions = true
                })
                .Add(new ResearchSummary()
                {
                    SummaryRowNo = 3,
                    IsClientSide = false,
                    ClientRelationshipSummary = new()
                    {
                        GISNotes = "",
                        GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID,
                        CERNotes = "",
                        CERDesc = CAUConstants.CER_NO_RECORD_WITH_UID_2
                    },
                    Sanctions = false
                })
                .Build());
    }
}
