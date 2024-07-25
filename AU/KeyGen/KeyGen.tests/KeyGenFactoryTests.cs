using KeyGen.lib.Enums;
using KeyGen.tests.Fixtures;

namespace KeyGen.tests;

public class KeyGenFactoryTests(KeyGenFixture keyGenFixture) : IClassFixture<KeyGenFixture>
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly KeyGenFixture _keyGenFixture = keyGenFixture;
#pragma warning restore IDE0052 // Remove unread private members
    private readonly KeyGenFactory _keyGenFactory = KeyGenFixture.KeyGenFactory;

    
    [Theory]
    [InlineData("MOLINA GALLEGOS & ASOCIADOS.", "", "", "", SubjectTypeEnum.Entity)]
    [InlineData("Souza & Silva Engenheiros Associados", "", "", "", SubjectTypeEnum.Entity)]
    public void GetSubjectType_btcIssue1022541_ReturnsTypeEntity
        (string name, string dunsNumber, string gisId, string paceApgLocation, SubjectTypeEnum expected)
    {
        SubjectTypeEnum result = _keyGenFactory.GetSubjectType(name, dunsNumber, gisId, paceApgLocation);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("A.N.P.Pritzker Trusts", "",          "16774702", "United States", SubjectTypeEnum.Entity)]
    [InlineData("A.N.P.Pritzker Trust",  "",          "16774702", "United States", SubjectTypeEnum.Entity)]
    [InlineData("A.N.P.Pritzker Trusts", "602495579", "9968120",  "United States", SubjectTypeEnum.Entity)]
    public void GetSubjectType_qaBug102166_ReturnsTypeEntity
        (string name, string dunsNumber, string gisId, string paceApgLocation, SubjectTypeEnum expected)
    {
        SubjectTypeEnum result = _keyGenFactory.GetSubjectType(name, dunsNumber, gisId, paceApgLocation);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("MBS Construction Chemicals Egypt (SAE)", "", "56002215", "Egypt", SubjectTypeEnum.Entity)]
    public void GetSubjectType_devNameWithLegalExtensionInsideSpecialCharsAndGisIdAndPaceApgLocationIdButNoDunsNumber_ReturnsTypeEntity
        (string name, string dunsNumber, string gisId, string paceApgLocation, SubjectTypeEnum expected)
    {
        SubjectTypeEnum result = _keyGenFactory.GetSubjectType(name, dunsNumber, gisId, paceApgLocation);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "", "", "", SubjectTypeEnum.UnableToDecide)]
    [InlineData("Lieutenant, Dan D.", "", "1234567", "", SubjectTypeEnum.Individual)]
    [InlineData("a.c.e., unknown", "", "1234567", "", SubjectTypeEnum.Entity)]
    [InlineData("ACME Gadgets", "150483782", "", "", SubjectTypeEnum.Entity)]
    [InlineData("Apple Inc.", "", "", "", SubjectTypeEnum.Entity)]
    [InlineData("Bank Unknown", "", "", "", SubjectTypeEnum.Entity)]
    [InlineData("Bank, Unknown", "", "", "", SubjectTypeEnum.Entity)]
    [InlineData("John Smith", "", "1307422", "", SubjectTypeEnum.Individual)]
    [InlineData("Mrs. Jane Doe", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Swift, Taylor", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Swift ,Taylor", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Swift , Taylor", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Swift, taylor A", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Swift ,taylor a", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Swift , taylor A", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("swift, Taylor a.", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("swift ,Taylor A.", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("swift , Taylor a.", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Abracadabra", "", "", "", SubjectTypeEnum.UnableToDecide)]
    [InlineData("Hugo Boss", "", "", "", SubjectTypeEnum.UnableToDecide)]
    public void GetSubjectType_devUsualCases_ReturnsType(string name, string dunsNumber, string gisId, string paceApgLocation, SubjectTypeEnum expected)
    {
        SubjectTypeEnum result = _keyGenFactory.GetSubjectType(name, dunsNumber, gisId, paceApgLocation);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("O'Gwen,   Christopher  A.", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Zatykó,  Peter", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("D'Angelo,  Peter", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("D'Amazzio    ,    Alexander F", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("D’Amazzio,  Alexander  F", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("D`Amazzio  ,Alexander  F.", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("D´Amazzio  ,  Alexander   F.", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Drašler,  Marjan", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Arson, Cécile", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Björg-Christensen ,  Hans", "", "", "", SubjectTypeEnum.Individual)]
    public void GetSubjectType_qaIndividualNamesWithDiacritics_ReturnsType(string name, string dunsNumber, string gisId, string paceApgLocation, SubjectTypeEnum expected)
    {
        SubjectTypeEnum result = _keyGenFactory.GetSubjectType(name, dunsNumber, gisId, paceApgLocation);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Parent, James", "", "123456789", "", SubjectTypeEnum.Entity)]
    [InlineData("Trust, John", "", "123456789", "", SubjectTypeEnum.Entity)]
    [InlineData("Capital, Alessandro", "", "123456789", "", SubjectTypeEnum.Entity)]
    [InlineData("Bank, Christian", "", "123456789", "", SubjectTypeEnum.Entity)]
    public void GetSubjectType_qaBug1020453_ReturnsType(string name, string dunsNumber, string gisId, string paceApgLocation, SubjectTypeEnum expected)
    {
        SubjectTypeEnum result = _keyGenFactory.GetSubjectType(name, dunsNumber, gisId, paceApgLocation);
        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("Young , Christie ", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData(" Young, Christie    L", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("Young , Christie  L. ", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("   OHara, Christie   ", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("O'Hara ,O'Donovan A", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("O’Hara , Christie O’Donovan A", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("O`Hara  ,   Christie O`Donovan Arlene", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("O´Hara  ,   Christie-Mary O´Donovan Arlene M", "", "", "", SubjectTypeEnum.Individual)]
    [InlineData("O´Hara  ,   Christie Lee-Anne Arlene P.", "", "", "", SubjectTypeEnum.Individual)]
    public void GetSubjectType_btcUserStory1019255IndirectNamePatterns_ReturnsIndividual(string name, string dunsNumber, string gisId, string paceApgLocation, SubjectTypeEnum expected)
    {
        SubjectTypeEnum result = _keyGenFactory.GetSubjectType(name, dunsNumber, gisId, paceApgLocation);
        Assert.Equal(expected, result);
    }
}
