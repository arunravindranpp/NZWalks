using KeyGen.tests.Extensions;
using KeyGen.tests.Fixtures;

namespace KeyGen.tests;

public class KeyGenForIndividualsTests : IClassFixture<KeyGenFixture>
{
    private readonly KeyGenFixture _keyGenFixture;
    private readonly KeyGenForIndividuals _keyGen;


    public KeyGenForIndividualsTests(KeyGenFixture keyGenFixture)
    {
        _keyGenFixture = keyGenFixture;
        _keyGen = _keyGenFixture.KeyGenForIndividuals;
    }


    [Theory]
    [InlineData("John F........... Kennedy", "Kennedy")]
    public void GenerateKey_btcManisha20240723_ReturnsProperKeyword(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("John Fitzgerald P........... Kennedy", "Kennedy, John|Kennedy, John Fitzgerald")]
    [InlineData("John F........... Kennedy", "Kennedy, John")]
    public void GenerateKeyForFinScanSearch_btcManisha20240723_ReturnsProperKeyword(string input, string expected)
    {
        var result = _keyGen.GenerateKeyForFinScanSearch(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Smith, John Patrick M.", "Smith, John|Smith, John Patrick")]
    [InlineData("Smith, John Patrick M", "Smith, John|Smith, John Patrick")]
    [InlineData("John Patrick M. Smith", "Smith, John|Smith, John Patrick")]
    [InlineData("John Patrick M Smith", "Smith, John|Smith, John Patrick")]
    [InlineData("John Patrick Alfred Paul H Smith", "Smith, John|Smith, John Patrick")]
    [InlineData("H Smith, John Patrick Alfred Paul", "Smith, John|Smith, John Patrick")]
    [InlineData("James Bond", "Bond, James")]
    [InlineData("Bond, James", "Bond, James")]
    [InlineData("James E Bond", "Bond, James")]
    [InlineData("James E. Bond", "Bond, James")]
    [InlineData("Bond, James E", "Bond, James")]
    [InlineData("Bond, James E.", "Bond, James")]
    [InlineData("Jean-Luc Cartier-Bresson", "Cartier-Bresson, Jean-Luc")]
    [InlineData("Mr. Peter F O’Toole Harold O’Donnel", "O’Donnel, Peter|ODonnel, Peter|O’Donnel, Peter O’Toole|ODonnel, Peter OToole")]
    [InlineData("Dr Peter F O'Toole Harold O'Donnel", "O'Donnel, Peter|ODonnel, Peter|O'Donnel, Peter O'Toole|ODonnel, Peter OToole")]
    [InlineData("Professor Peter F O`Toole Harold O`Donnel", "O`Donnel, Peter|ODonnel, Peter|O`Donnel, Peter O`Toole|ODonnel, Peter OToole")]
    [InlineData(" Harold O´Donnel , Peter F O´Toole ", "O´Donnel, Peter|ODonnel, Peter|O´Donnel, Peter O´Toole|ODonnel, Peter OToole")]
    [InlineData("Björn Borg", "Borg, Björn|Borg, Bjorn|Borg, Bjoern")]
    [InlineData("Jörgen W Friedrich Küttner",
                "Küttner, Jörgen|Küttner, Jörgen Friedrich|Kuttner, Jorgen|Kuttner, Jorgen Friedrich|Kuettner, Joergen|Kuettner, Joergen Friedrich")]
    public void GenerateKeyForFinScanSearch_devHelloWorldTest_ReturnsNameInIndirectForm(string input, string expected)
    {
        var result = _keyGen.GenerateKeyForFinScanSearch(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("  a`b~c!d@e#f$g%h^i&j*k(l)m‐n–o_p=q+r[s{t]u}v\\w|x;y:z'a’b´c\"d“«e”»f,g<h.i>j/k?l   ",
                "a b c d e f g h i j k l m n o p q r s t u v w x y z a b c d e f g h i j k l")]
    public void RemoveSpecialCharacters_devNameWithSpecialCharacters_ReturnsNameWithoutSpecialCharacters(string input, string expected)
    {
        var result = KeyGen.lib.KeyGen.RemoveSpecialCharacters(input);
        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("Dr. Frankenstein Jr.", "Frankenstein")]
    [InlineData("Dr. Frankenstein Jr", "Frankenstein")]
    [InlineData("Dr Frankenstein Jr.", "Frankenstein")]
    [InlineData("Dr Frankenstein Jr", "Frankenstein")]
    public void ReplaceCustomSubstrings_devNameWithLegalExtensions_ReturnsOrigStringWithoutLegalExtensions(string input, string expected)
    {
        var result = _keyGen.ReplaceCustomSubstrings(input);
        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("Atilio Luiz Magila Albiero Junior", "Albiero")]
    [InlineData("Marino Esq, James  J.", "Marino")]
    public void GenerateKey_qaBug1019000_IgnoresLegalExtensionsAndReturnsLastName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Young, Christie Lee Arlene.", "Young")]
    public void GenerateKey_btcInvertedName_ReturnsLastName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Jet Li", "Jet Li")]
    [InlineData("Li, Jet", "Li, Jet")]
    public void GenerateKey_qaBug1019010_CandidateResultWithLessThan3Chars_ReturnsFullOriginalName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Alfred Yao", "Yao")]
    [InlineData("Yao, Alfred", "Yao")]
    public void GenerateKey_qaBug1019010_CandidateResultWithMoreThanOrEqualTo3Chars_ReturnsMainName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Christie Lee Arlene Young", "Young")]
    public void GenerateKey_btcDirectName_ReturnsLastName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Dantas E G", "Dantas E G")]
    [InlineData("Madonna", "Madonna")]
    public void GenerateKey_devHelloWorldTest(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }

    
    [Theory]
    [InlineData("Peter O'Toole", "O'Toole|OToole")]
    [InlineData("O'Toole, Peter", "O'Toole|OToole")]
    public void GenerateKey_devDillipQuestion20240109_ReturnsMoreThanOneItem(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }

    [Fact]
    public void GenerateKey_devEmptyString_ReturnsEmptyString()
    {
        var result = _keyGen.GenerateKey(null);
        Assert.Equal(string.Empty, result.AsString("|"));

        result = _keyGen.GenerateKey(string.Empty);
        Assert.Equal(string.Empty, result.AsString("|"));
    }


    [Theory]
    [InlineData("Monday\x00A0 Tuesday\x00A0 Wednesday\x00A0Thursday\x00A0 \x00A0Friday\x00A0", "Friday")]
    public void GenerateKey_devNonBreakingSpaces_ReturnsSimpleSpaces(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("AB CD EF GH IJ KL MN OP QR ST UV WX YZ", "AB CD EF GH IJ KL MN OP QR ST UV WX YZ")]
    public void GenerateKey_devMultipleShortWords_ReturnsOriginalName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }

    [Theory]
    [InlineData(" Cæsar Augustus", "Augustus")]
    [InlineData("AUGUSTUS, CÆSAR ", "AUGUSTUS")]
    [InlineData("Björn Borg", "Borg")]
    [InlineData("Borg, Björn", "Borg")]
    [InlineData("aĪb, John", "aĪb|aIb")]
    [InlineData("CīD, John", "CīD|CiD")]
    public void GenerateKey_devContainsDiacritics_ReturnsAlternateForms(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }
    

    [Theory]
    [InlineData("  John Smith !@#$%^&*_+=|\\:;<>?`~/\"“”()[]{} ", "Smith")]
    public void GenerateKey_devContainsSpecialCharacters_ReturnsMainText(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("  John       (JK)  [JK] {jk} Kennedy   ", "Kennedy")]
    public void GenerateKey_devContainsTextInsideBrackets_ReturnsTextOutsideBrackets(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Aa Bb Cc Ddd Eeee", "Eeee")]
    [InlineData("John Patrick Smith", "Smith")]
    public void GenerateKey_devDirectSimpleName_ReturnsTheLongLastName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Jean-Pierre Henry Smith-Robertson", "Smith|Robertson")]
    [InlineData("Hu-Wuang Henry Tally-Ho", "Tally")]
    public void GenerateKey_devDirectCompoundName_ReturnsTheLongFirstNames(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Eeee Ff Gg Hh, Aa Bb Cc Ddd", "Eeee")]
    [InlineData("Smith, John Patrick", "Smith")]
    public void GenerateKey_devInvertedSimpleName_ReturnsTheLongLastName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Fffff-Gg Hh Ii, Aa Bb Cc Dd-Eeee", "Fffff")]
    [InlineData("Smith-Robertson, Jean-Pierre Henry", "Smith|Robertson")]
    public void GenerateKey_devInvertedCompoundName_ReturnsTheLongLastNames(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Aa Bb Cc Ddd-Eee Fff-Gggg-Beta Hh Ii", "Aa Bb Cc Ddd-Eee Fff-Gggg-Beta Hh Ii")]
    [InlineData("Aa Bb Cc Dd-Eeee Fffff-Gg-Beta Hh Ii", "Aa Bb Cc Dd-Eeee Fffff-Gg-Beta Hh Ii")]
    public void GenerateKey_devInvalidNames_ReturnsOriginalName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("O’Toole O’Donnel", "O’Donnel|ODonnel")]
    [InlineData("O'Toole O'Donnel", "O'Donnel|ODonnel")]
    [InlineData("O`Toole O`Donnel", "O`Donnel|ODonnel")]
    [InlineData("O´Toole O´Donnel", "O´Donnel|ODonnel")]
    [InlineData("O’Donnel, O’Toole", "O’Donnel|ODonnel")]
    [InlineData("O'Donnel, O'Toole", "O'Donnel|ODonnel")]
    [InlineData("O`Donnel, O`Toole", "O`Donnel|ODonnel")]
    [InlineData("O´Donnel, O´Toole", "O´Donnel|ODonnel")]
    public void GenerateKey_devNameWithApostrophy_ReturnsWithAndWithoutApostrophy(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Mr. John Smith", "Smith")]
    [InlineData("Mrs. Robinson, Jane", "Robinson")]
    [InlineData("Prof Charles Xavier, PhD", "Xavier")]
    public void GenerateKey_devNameWithTitle_IgnoreTitleReturnsLastName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }
}
