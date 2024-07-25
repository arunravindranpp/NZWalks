using KeyGen.tests.Extensions;
using KeyGen.tests.Fixtures;

namespace KeyGen.tests;

public class KeyGenForEntitiesTests : IClassFixture<KeyGenFixture>
{
    private readonly KeyGenFixture _keyGenFixture;
    private readonly KeyGenForEntities _keyGen;


    [Theory]
    [InlineData("ABC,Plastics", "ABC, Plastics|ABC Plastics")]
    [InlineData("ABC-Plastics", "ABC-Plastics|ABC - Plastics|ABC Plastics")]
    [InlineData("ABC XY.............. Inc.", "ABC XY.|ABC XY")]
    public void GenerateKey_btcManisha20240723_ReturnsProperKeyword(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    public KeyGenForEntitiesTests(KeyGenFixture keyGenFixture)
    {
        _keyGenFixture = keyGenFixture;
        _keyGen = _keyGenFixture.KeyGenForEntities;
    }


    [Theory]
    [InlineData("INTERNATIONAL BANK FOR RECONSTRUCTION AND DEVELOPMENT",
                "INTERNATIONAL BANK FOR RECONSTRUCTION AND DEVELOPMENT|BANK FOR")]
    [InlineData("St. James's Place - nonclient pension/insurance products", 
                "St. James's Place - nonclient pension/insurance products|James's Place|Jamess Place")]
    [InlineData("Thüga & Holding", "Thüga & Holding|Thüga|Thuga|Thuega")]
    public void GenerateKeyForFinScanSearch_btc20240715_ReturnsFullNameAndKeywords(string input, string expected)
    {
        var result = _keyGen.GenerateKeyForFinScanSearch(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Dong-A ST Co.,Ltd.", "Dong-A ST|Dong - A ST|Dong A ST")]
    public void GenerateKey_uatIssue1022487LegalExtSuffixWithPunctuationInBetween_ReturnsNameWithoutLegalExt(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("European Union / European Institutions", "Union / European|Union European")]
    [InlineData("European Union / European Supercalifragilisticexpialidocious", "Union / European|Union European")]
    public void GenerateKey_btcNameStartingWithLegalExtensionAndContainingSpecialChar_ReturnsProperKeyword(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("European Union / European Institution", "Union / European|Union European")]
    public void GenerateKey_btcNameEndingWithLegalExtensionAndContainingSpecialChar_ReturnsProperKeyword(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Theion GmbH", "Theion")]
    public void GenerateKey_btcPrefixContainingSubstringSimilarToLegalExtension_ReturnsProperKeyword(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Theion GmbH", "Theion")]
    public void ReplaceCustomSubstrings_btcPrefixContainingSubstringSimilarToLegalExtension_ReturnsProperKeyword(string input, string expected)
    {
        var result = _keyGen.ReplaceCustomSubstrings(input);
        Assert.Equal(expected, result);
    }


    [Theory]
    // [InlineData("Thüga Holding GmbH & Co. KGaA", "Thüga|Thuga|Thuega")]
    // Above line commented out and replaced by the following one due to
    // User Story #1021023 - Consider legal extensions at beginning & end of entity name only
    [InlineData("Thüga Holding GmbH & Co. KGaA", "Thüga Holding|Thuga Holding|Thuega Holding")]
    [InlineData("Thüga & Holding", "Thüga|Thuga|Thuega")]
    [InlineData("Thüga & Holding GmbH & Co. KGaA",
                "Thüga & Holding|Thuga & Holding|Thuega & Holding|Thüga and Holding|Thuga and Holding|Thuega and Holding")]    
    public void GenerateKey_qaBug1021376NamesEndingWithAmpersandOrAnd_ReturnsNameOnly(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
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
    [InlineData("CENTRO DE EXCELENCIA ONCOLOGICA SA", "CENTRO DE EXCELENCIA")]
    [InlineData("Aaaaa Ltd Bbbbb Ccccc", "Aaaaa Ltd")]
    [InlineData("Ltd Aaaaa Bbbbb Ccccc", "Aaaaa Bbbbb")]
    [InlineData("Aaaaa Bbbbb Ccccc Ltd", "Aaaaa Bbbbb")]
    public void GenerateKey_ustNameWithLegalExtensionsAtTheVeryBeginningOrEnd_ReturnsKeywordsWithoutLEPrefixesOrSufixes(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    // [Theory]
    // [InlineData("MINISTRY OF FINANCE", "MINISTRY OF FINANCE")]  // ok
    // // The Keyword Generation algorithm tries to get 2 words with at least 3 characters long.
    // // - "MINISTRY" has 8 characters; so, at this point, we have 1 word with at least 3 characters; 
    // // - "OF" has 2 characters; so, at this point, we still have only 1 word with at least 3 characters; therefore it continues;
    // // - "FINANCE" has 7 characters; so, at this point, we finally have 2 words with at least 3 characters, and then it stops.

    // // In this other case, user says it should be "MINISTRY FOR FINANCE", but Keyword Generator returns "MINISTRY FOR".
    // // This happens because the Keyword Generation algorithm tries to get 2 words with at least 3 characters long.
    // // - "MINISTRY" has 8 characters; so, at this point, we have 1 word with at least 3 characters; 
    // // - "FOR" has 3 characters; so, at this point, we have 2 words with at least 3 characters, and then it stops.
    // [InlineData("MINISTER FOR FINANCE", "MINISTER FOR")]

    // // Similarly to the 1st case, "MINISTRY OFa FINANCE" would return "MINISTRY OFa", 
    // // This happens because the Keyword Generation algorithm tries to get 2 words with at least 3 characters long.
    // // - "MINISTRY" has 8 characters; so, at this point, we have 1 word with at least 3 characters; 
    // // - "OFa" has 3 characters; so, at this point, we have 2 words with at least 3 characters, and then it stops.
    // [InlineData("MINISTRY OFa FINANCE", "MINISTRY OFa")]

    // public void ReplaceCustomSubstrings_devMinist(string input, string expected)
    // {
    //     var result = _keyGen.GenerateKey(input);
    //     Assert.Equal(expected, result.AsString("|"));
    // }


    [Theory]
    [InlineData("MBS CONSTRUCTION CHEMICALS EGYPT S.A.E", "MBS CONSTRUCTION CHEMICALS EGYPT")]
    [InlineData("MBS Construction Chemicals Egypt SAE", "MBS Construction Chemicals Egypt")]
    [InlineData("MBS CONSTRUCTION CHEMICALS EGYPT (S.A.E)", "MBS CONSTRUCTION CHEMICALS EGYPT")]
    [InlineData("MBS Construction Chemicals Egypt,SAE", "MBS Construction Chemicals Egypt,")]
    [InlineData("MBS Construction Chemicals Egypt ;S.A.E", "MBS Construction Chemicals Egypt;")]
    [InlineData("S.A.E MBS CONSTRUCTION CHEMICALS EGYPT", "MBS CONSTRUCTION CHEMICALS EGYPT")]
    [InlineData("(S.A.E) MBS CONSTRUCTION CHEMICALS EGYPT", "MBS CONSTRUCTION CHEMICALS EGYPT")]
    public void ReplaceCustomSubstrings_devMBS(string input, string expected)
    {
        var result = _keyGen.ReplaceCustomSubstrings(input);
        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("Apple, Inc.", "Apple")]
    [InlineData("Apple, Inc", "Apple")]
    [InlineData("Apple Inc.", "Apple")]
    [InlineData("Apple Inc", "Apple")]
    public void GenerateKey_devHelloWorldTest(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Bank of America Financial S/A", "Bank of America")]
    [InlineData("Standard Oil S/A", "Standard Oil")]
    public void GenerateKey_ustNameWithSpecialIdentifWords_ReturnsKeepingSpecialIdentifWords(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData(
        "K, Hovnanian Craftbuilt Homes of South Carolina, L.L.C.",
        "K, Hovnanian Craftbuilt|K Hovnanian Craftbuilt")]
    public void GenerateKey_qaBug1020345_ReturnsUntilSecondLongWord(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Accel ‐KKR Growth Capital Partners II, LP", "Accel -KKR|Accel - KKR|Accel KKR")]
    public void GenerateKey_qaBug1019216_ReturnsDashVariations(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Electronics Boutique Canada Inc", "Boutique Canada")]
    [InlineData("Electronics For Imaging Inc", "For Imaging")]
    [InlineData("SCIENTIFIC GAMES FRANCE SARL", "GAMES FRANCE")]
    [InlineData("SERVICE DE TRANSPORT COMBINE TUNISIE SARL", "DE TRANSPORT COMBINE")]

    public void GenerateKey_qaBug1019000_ReturnsAtMostTwoFirstWordsOfNameWithoutLegalExtensions(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("The QA Inc.", "The QA Inc.")]
    [InlineData("SK Inc", "SK Inc")]
    public void GenerateKey_qaBug1019010_CandidateResultWithLessThan3Chars_ReturnsFullOriginalName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("The QAC Inc.", "QAC")]
    public void GenerateKey_qaBug1019010_CandidateResultWithMoreThanOrEqualTo3Chars_ReturnsMainName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("St. James's Place - nonclient pension/insurance products", "James's Place|Jamess Place")]
    [InlineData("Mt. James's Place - nonclient pension/insurance products", "Mt. James's|Mt. Jamess|Mt James's Place|Mt Jamess Place")]
    public void GenerateKey_qaSpecialChars_ReturnsAllCombinations(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Facebook (Thailand) Limited", "Facebook (Thailand)|Facebook Thailand")]
    public void GenerateKey_btcSpecialChars_ReturnsOrigAndClearedKWs(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("DBB Vermögensverwaltung GmbH & Co.KG", "DBB Vermögensverwaltung|DBB Vermogensverwaltung|DBB Vermoegensverwaltung")]
    [InlineData("aĪb", "aĪb|aIb")]
    [InlineData("CīD", "CīD|CiD")]
    public void GenerateKey_btcDiacritics_ReturnsOrigAndEquivalentKWs(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Accel-KKR Growth Capital Partners II, LP", "Accel-KKR Growth|Accel - KKR Growth|Accel KKR Growth")]
    public void GenerateKey_btcDash_ReturnsThreeFormsOfDash(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("LEONARDO E GIUSEPPE VENA HOLDING SRL IN FORMA ABBREVIATO LGV HOLDING SRL.",
                "LEONARDO E GIUSEPPE")]
    public void GenerateKey_btcLongName_ReturnsShortenedName(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Guess? Inc.", "Guess?|Guess")]
    public void GenerateKey_btcNameWithSpecialChars_ReturnsWithAndWithoutSpecialChars(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("Smith & Sons Inc.", "Smith & Sons|Smith and Sons")]
    [InlineData("Ho & Fu & Co.", "Ho & Fu|Ho and Fu")]
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
    [InlineData("a1\x00A0 b2\x00A0c3 d4\x00A0e5", "a1 b2 c3 d4 e5")]
    public void GenerateKey_devNonBreakingSpaces_ReturnsSimpleSpaces(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("  Abcde  ,   Fghij", "Abcde, Fghij|Abcde Fghij")]
    public void GenerateKey_devNonStdCommasAndSpaces_ReturnsStringWithStdCommasAndSpaces(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("(   dO  nOT    uSE  )    XX YY ZZ", "XX YY ZZ")]
    [InlineData("  liQuidATeD  -    XX YY ZZ", "XX YY ZZ")]
    public void GenerateKey_devUnwantedPrefix_ReturnsStringWithoutPrefix(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("ACME", "ACME")]
    [InlineData("ACME GADGETS", "ACME GADGETS")]
    public void GenerateKey_devAtMost2Words_ReturnsAtMostThe1st2Words(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("ACME GADGETS XY", "ACME GADGETS")]
    public void GenerateKey_devAtLeast3WordsAndFirst2WithAtLeast3Chars_ReturnsAtMostThe1st2Words(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("HO LI YU TAO MING", "HO LI YU TAO MING")]
    [InlineData("HO LI YU MING", "HO LI YU MING")]
    [InlineData("HO LI FU YU MING", "HO LI FU YU MING")]
    public void GenerateKey_devAtLeast3WordsAndFirst2WithLessThan3Chars_ReturnsUntilTheFirstLongWord(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("QW ER TY UI OP ZX BN ML", "QW ER TY UI OP ZX BN ML")]
    public void GenerateKey_devAtLeast3WordsAndNoLongWords_ReturnsAllWords(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("( DO NOT USE ) Brazil Coffee Starship   Qwertyuiop   ", "Brazil Coffee Starship Qwertyuiop")]
    [InlineData("Brazil Coffee Starship   Qwertyuiop   ", "Brazil Coffee Starship Qwertyuiop")]
    [InlineData("  Massachusetts Institute  of   Technology   ", "Massachusetts Institute of")]
    [InlineData("  United  States   Electronic   Components    ", "United States Electronic Components")]
    public void GenerateKey_devStartsWithGeoLocationName_ReturnsNameWithGeoLocationPrefix(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("   Bananarama Fruits    ", "Bananarama Fruits")]
    [InlineData("   Bananarama Company   ", "Bananarama")]
    [InlineData(" US Steel Group   Corp  Ltd.   ", "US Steel Group")]
    public void GenerateKey_devLegalSpecs_ReturnsStringWithoutLegalSpecs(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }


    [Theory]
    [InlineData("ENCYCLOPÆDIA BRITANNICA", "ENCYCLOPÆDIA BRITANNICA|ENCYCLOPAEDIA BRITANNICA")]
    [InlineData("Encyclopædia Britannica", "Encyclopædia Britannica|Encyclopaedia Britannica")]
    [InlineData("Friederichstraße Berlin", "Friederichstraße Berlin|Friederichstrase Berlin|Friederichstrasse Berlin")]
    [InlineData("Jürgenstraße", "Jürgenstraße|Jurgenstrase|Juergenstrasse")]
    public void GenerateKey_devContainsDiacritics_ReturnsAlternateForms(string input, string expected)
    {
        var result = _keyGen.GenerateKey(input);
        Assert.Equal(expected, result.AsString("|"));
    }
}
