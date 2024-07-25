using ConflictAutomation.Extensions;
using System.Text.RegularExpressions;

namespace ConflictAutomation.Services.KeyGen;

public class KeyGenForIndividuals : KeyGen
{
    protected static readonly List<char> APOSTROPHIES = ['’', '\'', '`', '´'];


    public KeyGenForIndividuals(
            Dictionary<string, string> substringReplacements = null,
            Dictionary<string, string> specialCharacterReplacements = null,
            Dictionary<string, string[]> diacriticsReplacements = null
           ) : base(substringReplacements, specialCharacterReplacements, diacriticsReplacements)
    {
        SubstringReplacements = substringReplacements ?? DefaultValueSubstringReplacements;
        SpecialCharacterReplacements = specialCharacterReplacements ?? DefaultValueSpecialCharacterReplacements;
    }


    public override List<string> GenerateKey(string individualName)
    {
        individualName = individualName?.FullTrim();
        if (string.IsNullOrEmpty(individualName))
        {
            return [string.Empty];
        }

        string name = individualName;

        // Recommended sequence of operations: 
        //   (KeyGen)                1. Fix unusual things related to applications and OS; 
        //   (KeyGen)                2. Standardize punctuation that may deviate due to user input; (DISMISSED; it is not necessary for individuals.)
        //   (KeyGen)                3. Perform replacements of diacritics characters; 
        //   (KeyGenForIndividuals)  4. Perform removal of custom substrings(segments of whole words - surrounded by spaces); 
        //   (KeyGen)                5. Perform replacements of custom substrings(segments of whole words - surrounded by spaces); 
        //   (KeyGenForIndividuals)  6. Perform replacements of special characters(not necessarily surrounded by spaces);
        //   (KeyGen)                7. Same as #5, once more(*): perform replacements of custom substrings(segments of whole words - surrounded by spaces); 
        //                              (*) Because the substrings to be replaced may have missed some special characters.
        //   (KeyGenForIndividuals)  8. Shorten the inputList (uses KeyGen.ShortenName()).
        //   (KeyGenForIndividuals)  9. Handle special cases.
        //   (KeyGen)               10. Restore original name for too short keywords.

        name = FixNonBreakingSpaces(name);  // #1
        name = FixTabs(name);  // #1
        name = StandardizeDashes(name);  // #1
        name = StandardizeDots(name);  // #1

        List<string> result;

        result = ReplaceDiacritics(name);  // #3

        result = RemoveTextInsideBrackets(result);  // #4

        result = ReplaceCustomSubstrings(result);  // #5

        result = ReplaceSpecialCharacters(result);  // #6

        result = ReplaceCustomSubstrings(result);  // #7

        result = GetMainWords(result, countryName: null);  // #8

        result = HandleSpecialCases(result);  // #9

        result = RestoreOriginalNameForTooShortKeywords(result, name);  // #10

        result = result.Distinct().ToList();
        return result;
    }


    public List<string> GenerateKeyForFinScanSearch(string individualName)
    {
        individualName = individualName?.FullTrim();
        if (string.IsNullOrEmpty(individualName))
        {
            return [string.Empty];
        }

        string name = individualName;

        name = FixNonBreakingSpaces(name);  // #1
        name = FixTabs(name);  // #1
        name = StandardizeDashes(name);  // #1
        name = StandardizeDots(name);  // #1

        List<string> result;

        result = ReplaceDiacritics(name);  // #3

        result = RemoveTextInsideBrackets(result);  // #4

        result = ReplaceCustomSubstrings(result);  // #5

        result = ReplaceSpecialCharactersForFinScanSearch(result);  // #6

        result = ReplaceCustomSubstrings(result);  // #7

        result = GetNameInIndirectFormat(result);  // #8

        result = HandleSpecialCases(result, doCommaHandling: false);  // #9        

        result = result.Distinct().ToList();
        return result;
    }


    private static List<string> ReplaceSpecialCharactersForFinScanSearch(List<string> result)
    {
        List<char> exceptionList = [HYPHEN, STD_COMMA[0]];
        exceptionList.AddRange(APOSTROPHIES);

        result = result.Select(keyword => RemoveSpecialCharacters(keyword, exceptionList).FullTrim()).ToList();
        return result;
    }


    protected static Dictionary<string, string> DefaultValueSubstringReplacements =>
        new() 
        {
            {"col", " "},
            {"col.", " "},
            {"doctor", " "},
            {"dr", " "},
            {"dr.", " "},
            {"esq", " "},
            {"esq.", " "},
            {"gen", " "},
            {"gen.", " "},
            {"hon", " "},
            {"hon.", " "},
            {"honorable", " "},
            {"honorary", " "},
            {"jr", " "},
            {"jr.", " "},
            {"junior", " "},
            {"lieutenant", " "},
            {"lt", " "},
            {"lt.", " "},
            {"m.d", " "},
            {"m.d.", " "},
            {"md", " "},
            {"md.", " "},
            {"mister", " "},
            {"mr", " "},
            {"mr.", " "},
            {"mrs", " "},
            {"mrs.", " "},
            {"ms", " "},
            {"ms.", " "},
            {"phd", " "},
            {"phd.", " "},
            {"prof", " "},
            {"prof.", " "},
            {"professor", " "},
            {"rev", " "},
            {"rev.", " "},
            {"reverend", " "},
            {"rt hon.", " "},
            {"rt. hon", " "},
            {"rt. hon.", " "},
            {"senior", " "},
            {"sir", " "},
            {"sr", " "},
            {"sr.", " "}
        };


    protected static Dictionary<string, string> DefaultValueSpecialCharacterReplacements =>
    new() {
            { "‐", "-" },    // From ASC 63 = UNICODE \u2010 to ASC 45 = UNICODE \u002D (regular dash)
            { "–", "-" },    // From ASC 150 = UNICODE \u2013 to ASC 45 = UNICODE \u002D (regular dash)
            { "!", " " },
            { "@", " " },
            { "#", " " },
            { "$", " " },
            { "%", " " },
            { "^", " " },
            { "*", " " },
            { "_", " " },
            { "+", " " },
            { "=", " " },
            { "|", " " },
            { "\\", " " },
            { ":", " " },
            { ";", " " },
            { "<", " " },
            { ">", " " },
            { "?", " " },
            { "~", " " },
            { "/", " " },
            { "\"", " " },
            { "“", " " },
            { "”", " " },
            { "(", " " },
            { ")", " " },
            { "[", " " },
            { "]", " " },
            { "{", " " },
            { "}", " " },
            { ".", " " },
            { "&", " " }
    };


    // This method uses a RegEx to identify: 
    //   [(\[{]    an opening  parenthesis or square bracket or curly bracket
    //   .*        followed by zero or more characters    
    //   ?         immediately followed by (i.e. performs a "lazy search" for the first occurrence of)
    //   [)\]}]    a closing  parenthesis or square bracket or curly bracket
    // 
    // Once located, the substrings that match this pattern are removed from the string.
    protected static string RemoveTextInsideBrackets(string individualName)
    {
        individualName = individualName?.FullTrim();
        if (string.IsNullOrEmpty(individualName))
        {
            return string.Empty;
        }

        string regex = @"[(\[{].*?[)\]}]";
        string result = Regex.Replace(individualName, regex, string.Empty, RegexOptions.IgnoreCase);

        return result.FullTrim();
    }


    protected static List<string> RemoveTextInsideBrackets(List<string> inputList) => 
        inputList.Select(name => RemoveTextInsideBrackets(name)).Distinct().ToList();


    protected virtual List<string> ReplaceSpecialCharacters(List<string> inputList)
    {
        if (SubstringReplacements.IsNullOrEmpty())
        {
            return inputList;
        }

        // The statement below calls base class method KeyGen.ReplaceSpecialCharacters()
        List<string> result = inputList.Select(keyword => ReplaceSpecialCharacters(keyword)).ToList();

        result = result.Distinct().ToList();
        return result;
    }


    protected static List<string> GetMainWords(string name, string countryName)
    {
        while (!name.IsNullOrEmpty())
        {
            // Ensure that name is a direct name, like "John Smith", instead of "Smith, John".
            name = DirectName(name);

            var allWords = name.Split(" ").ToArray();
            if (allWords.Length < 2)
            {
                return [name];  // name is a single word, like "Madonna".
            }

            // If there is no countryName, returns the possible long last word from name; 
            // otherwise, returns the possible long first word from name.
            string potentialName = string.IsNullOrWhiteSpace(countryName) ?
                                        PossibleLongLastName(allWords)
                                        : PossibleLongFirstName(allWords);
            if (potentialName.IsNullOrEmpty())
            {
                break;
            }

            int hyphenCount = potentialName.Count(HYPHEN.ToString());
            if (hyphenCount < 1)
            {
                return [potentialName];  // potentialName is a single word.
            }
            else if (hyphenCount > 1)
            {
                break;  // potentialName is an invalid name, because it contains more than one HYPHEN.
            }

            // At this point, potentialName is a "compound name", i.e. a name
            // with exactly one HYPHEN, like "Jean-Pierre" or "Smith-Robertson").

            // Extracts the long words from potentialName.
            var potentialResults = potentialName.Split(HYPHEN).Where(IsLongWord);
            if (!potentialResults.IsNullOrEmpty())
            {
                return potentialResults.ToList();
            }

            // At this point, each of the two words separated by HYPHEN in potentialName is too short.
            // So, remove the first word (i.e. potentialName) from name and try this loop again.
            name = name.Replace(potentialName, string.Empty).FullTrim();
        }

        return [string.Empty];  // In case of invalid names, returns an empty list.
    }


    private static string PossibleLongLastName(string[] allWords)
    {
        for (int i = allWords.Length - 1; i > 0; i--)  // Discard the first word, 
        {                                              // which is surelly the first name.
            string word = allWords[i];
            if (IsLongWord(word))
            {
                return word;
            }
        }
        return string.Empty;  // Possible long last name not found
    }
    private static string PossibleLongFirstName(string[] allWords)
    {
        for (int i = 1; i < allWords.Length; i++)  // Discard the first word, 
        {                                          // which is surelly the first name.
            string word = allWords[i];
            if (IsLongWord(word))
            {
                return word;
            }
        }
        return string.Empty;  // Possible long first name not found
    }


    protected static List<string> GetMainWords(List<string> inputList, string countryName)
    {
        List<string> result = [];

        if(inputList.IsNullOrEmpty())
        {
            return result;
        }

        foreach(string name in inputList)
        {
            result.AddRange(GetMainWords(name, countryName));
        }

        result = result.Distinct().ToList();
        return result;
    }


    protected static string DirectName(string name)
    {
        if (name.IsNullOrEmpty())
        {
            return string.Empty;
        }

        if (name.Contains(STD_COMMA))
        {
            // Inverted name, like "Smith, John".
            return $"{name.StrRight(STD_COMMA)} {name.StrLeft(STD_COMMA)}".FullTrim();
        }

        // Direct name, like "John Smith".
        return name.FullTrim();
    }


    protected static List<string> HandleSpecialCases(List<string> inputKeywords, bool doCommaHandling = true)
    {
        List<string> result = [];
        List<string> appostrophiesAsListString = AppostrophiesAsListString();

        foreach (string keyword in inputKeywords)
        {
            result.Add(keyword);            
            if (keyword.Contains(appostrophiesAsListString))
            {
                result.Add(keyword.RemoveAll(appostrophiesAsListString));
            }
        }

        if (doCommaHandling)
        {
            result = result.Select(w => w.Replace(STD_COMMA.FullTrim(), " ").FullTrim()).ToList();
        }

        result = result.Distinct().ToList();

        return result;
    }


    public override string ReplaceCustomSubstrings(string text)
    {
        if (SubstringReplacements.IsNullOrEmpty())
        {
            return text;
        }

        string auxText = text;
        List<string> sepSymbols = ["(", ")", "{", "}", "[", "]", "<", ">", ",", ";", ":"];
        foreach (string sepSymbol in sepSymbols)
        {
            auxText = auxText.Replace(sepSymbol, $" {sepSymbol} ");
        }

        string result = " " + auxText.FullTrim() + " ";
        foreach (var replacement in SubstringReplacements!)
        {
            result = result.Replace($" {replacement.Key} ", $" {replacement.Value} ",
                                    StringComparison.OrdinalIgnoreCase);
        }

        result = result.FullTrim()
                       .Replace("( ", "(")
                       .Replace("[ ", "[")
                       .Replace("{ ", "{")
                       .Replace("< ", "<")
                       .Replace(" )", ")")
                       .Replace(" ]", ")")
                       .Replace(" }", "}")
                       .Replace(" >", ">")
                       .Replace(" ,", ",")
                       .Replace(" ;", ";")
                       .Replace(" :", ":")
                       .Replace("()", "")
                       .Replace("[]", "")
                       .Replace("{}", "")
                       .Replace("<>", "");

        return result.FullTrim();
    }


    protected static List<string> GetNameInIndirectFormat(List<string> inputList)
    {
        List<string> result = [];

        if (inputList.IsNullOrEmpty())
        {
            return result;
        }

        return inputList.SelectMany(name => GetNameInIndirectFormat(name)).Distinct().ToList();
    }


    protected static List<string> GetNameInIndirectFormat(string name)
    {
        const int COUNT_INITIAL_NAMES = 2;

        name = name?.FullTrim();

        if (string.IsNullOrEmpty(name))
        {
            return [string.Empty];
        }

        if (name.Contains(STD_COMMA))
        {
            name = DirectName(name);
        }

#pragma warning disable IDE0305 // Simplify collection initialization
        List<string> words = name.Split(STD_SPACE).ToList();
#pragma warning restore IDE0305 // Simplify collection initialization

        string lastName = words.Last().FullTrim();

        // Get the "initial names" (all words, except the last one).
        List<string> initialNames = words.Take(words.Count - 1).ToList();

        // initialNames with 1 char only are discarded; 
        // Get, at most, COUNT_INITIAL_NAMES "initial names"
        initialNames = initialNames.Where(word => word.FullTrim().Length > 1)
                                   .Take(COUNT_INITIAL_NAMES)
                                   .ToList();
        if(initialNames.IsNullOrEmpty())
        {
            return [lastName];
        }

        List<string> result = [];
        for (int i = 0; i < initialNames.Count; i++)
        {
            result.Add($"{lastName}, {string.Join(STD_SPACE, initialNames.Take(i+1))}".FullTrim());
        }
        return result.IsNullOrEmpty() ? [lastName] : result.Distinct().ToList();
    }


    private static List<string> AppostrophiesAsListString() => 
        APOSTROPHIES.Select(character => character.ToString()).ToList();
}
    