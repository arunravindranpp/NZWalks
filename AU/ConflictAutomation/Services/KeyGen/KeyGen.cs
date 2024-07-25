using ConflictAutomation.Extensions;

namespace ConflictAutomation.Services.KeyGen;

public abstract class KeyGen
{
    protected const char NON_BREAKING_SPACE = '\x00A0';  // char 160
    protected const char TAB = '\x0009';  // char 9
    protected const char STD_SPACE = '\x0020';  // char 32
    protected const char HYPHEN = '-';  // ASCII 45 = \u002D (standard dash = minus sign)
    protected static readonly string[] NON_STANDARD_COMMAS = [" , ", " ,", ","];
    protected const string STD_COMMA = ", ";
    protected const int LONG_LENGTH = 3;


    private Dictionary<string, string> _substringReplacements = null;
    public Dictionary<string, string> SubstringReplacements
    {
        get => _substringReplacements;
        set => _substringReplacements = value?.SortByLengthDescending();
    }

    public Dictionary<string, string> SpecialCharacterReplacements { get; set; }


    public Dictionary<string, string[]> DiacriticsReplacements { get; set; }


    public KeyGen(
            Dictionary<string, string> substringReplacements = null,
            Dictionary<string, string> specialCharacterReplacements = null,
            Dictionary<string, string[]> diacriticsReplacements = null)
    {
        SubstringReplacements = substringReplacements;
        SpecialCharacterReplacements = specialCharacterReplacements;
        DiacriticsReplacements = diacriticsReplacements ?? DefaultValueDiacriticsReplacements;
    }


    public abstract List<string> GenerateKey(string name);


    // Replace non-breaking spaces with standard spaces
    protected static string FixNonBreakingSpaces(string text) =>
        text.Replace((char)NON_BREAKING_SPACE, (char)STD_SPACE).FullTrim();


    // Replace tabs with standard spaces
    protected static string FixTabs(string text) =>
        text.Replace((char)TAB, (char)STD_SPACE).FullTrim();


    // From ASCII 8208 = \u2010 (short dash) or ASCII 8211 = \u2013 (long dash)
    // to standard dash (ASCII 45 = \u002D)
    protected static string StandardizeDashes(string text) =>
        text.Replace($"{(char)8208}", HYPHEN.ToString())
            .Replace($"{(char)8211}", HYPHEN.ToString()).FullTrim();


    protected static string StandardizeDots(string text)
    {
        for (int i = 0; i < 40; i++)
        {
            text = text.Replace("..", ".");
        }
        return text.FullTrim();
    }


    protected static Dictionary<string, string[]> DefaultValueDiacriticsReplacements =>
        new()
        {
            { "Á", [ "Á", "A", "A", "" ] },
            { "á", [ "á", "a", "a", "" ] },
            { "À", [ "À", "A", "A", "" ] },
            { "à", [ "à", "a", "a", "" ] },
            { "Â", [ "Â", "A", "A", "" ] },
            { "â", [ "â", "a", "a", "" ] },
            { "Ä", [ "Ä", "A", "ae", "" ] },
            { "ä", [ "ä", "a", "ae", "" ] },
            { "Ã", [ "Ã", "A", "A", "" ] },
            { "ã", [ "ã", "a", "a", "" ] },
            { "Å", [ "Å", "A", "aa", "" ] },
            { "å", [ "å", "a", "aa", "" ] },
            { "Ầ", [ "Ầ", "A", "A", "" ] },
            { "ầ", [ "ầ", "a", "a", "" ] },
            { "Ả", [ "Ả", "A", "A", "" ] },
            { "ả", [ "ả", "a", "a", "" ] },
            { "Ậ", [ "Ậ", "A", "A", "" ] },
            { "ậ", [ "ậ", "a", "a", "" ] },
            { "Æ", [ "Æ", "AE", "AE", "" ] },
            { "æ", [ "æ", "ae", "ae", "" ] },
            { "Č", [ "Č", "C", "C", "" ] },
            { "č", [ "č", "c", "c", "" ] },
            { "Ç", [ "Ç", "C", "C", "" ] },
            { "ç", [ "ç", "c", "c", "" ] },
            { "Đ", [ "Đ", "D", "D", "" ] },
            { "đ", [ "đ", "d", "d", "" ] },
            { "É", [ "É", "E", "E", "" ] },
            { "é", [ "é", "e", "e", "" ] },
            { "È", [ "È", "E", "E", "" ] },
            { "è", [ "è", "e", "e", "" ] },
            { "Ê", [ "Ê", "E", "E", "" ] },
            { "ê", [ "ê", "e", "e", "" ] },
            { "Ë", [ "Ë", "E", "ee", "" ] },
            { "ë", [ "ë", "e", "ee", "" ] },
            { "Ě", [ "Ě", "E", "E", "" ] },
            { "ě", [ "ě", "e", "e", "" ] },
            { "Ę", [ "Ę", "E", "E", "" ] },
            { "ę", [ "ę", "e", "e", "" ] },
            { "Ể", [ "Ể", "E", "E", "" ] },
            { "ể", [ "ể", "e", "e", "" ] },
            { "Ġ", [ "Ġ", "G", "G", "" ] },
            { "ġ", [ "ġ", "g", "g", "" ] },
            { "Ğ", [ "Ğ", "G", "G", "" ] },
            { "ğ", [ "ğ", "g", "g", "" ] },
            { "Ħ", [ "Ħ", "H", "H", "" ] },
            { "ħ", [ "ħ", "h", "h", "" ] },
            { "Í", [ "Í", "I", "I", "" ] },
            { "í", [ "í", "i", "i", "" ] },
            { "Ì", [ "Ì", "I", "I", "" ] },
            { "ì", [ "ì", "i", "i", "" ] },
            { "Î", [ "Î", "I", "I", "" ] },
            { "î", [ "î", "i", "i", "" ] },
            { "Ï", [ "Ï", "I", "ie", "" ] },
            { "ï", [ "ï", "i", "ie", "" ] },
            { "Ĩ", [ "Ĩ", "I", "I", "" ] },
            { "ĩ", [ "ĩ", "i", "i", "" ] },
            { "Ī", [ "Ī", "I", "I", "" ] },
            { "ī", [ "ī", "i", "i", "" ] },
            { "Ł", [ "Ł", "L", "L", "" ] },
            { "ł", [ "ł", "l", "l", "" ] },
            { "Ń", [ "Ń", "N", "N", "" ] },
            { "ń", [ "ń", "n", "n", "" ] },
            { "Ñ", [ "Ñ", "N", "ny", "" ] },
            { "ñ", [ "ñ", "n", "ny", "" ] },
            { "Ņ", [ "Ņ", "N", "N", "" ] },
            { "ņ", [ "ņ", "n", "n", "" ] },
            { "Ó", [ "Ó", "O", "O", "" ] },
            { "ó", [ "ó", "o", "o", "" ] },
            { "Ò", [ "Ò", "O", "O", "" ] },
            { "ò", [ "ò", "o", "o", "" ] },
            { "Ô", [ "Ô", "O", "O", "" ] },
            { "ô", [ "ô", "o", "o", "" ] },
            { "Ö", [ "Ö", "O", "OE", "" ] },
            { "ö", [ "ö", "o", "oe", "" ] },
            { "Ō", [ "Ō", "O", "O", "" ] },
            { "ō", [ "ō", "o", "o", "" ] },
            { "Õ", [ "Õ", "O", "O", "" ] },
            { "õ", [ "õ", "o", "o", "" ] },
            { "Ồ", [ "Ồ", "O", "O", "" ] },
            { "ồ", [ "ồ", "o", "o", "" ] },
            { "Ø", [ "Ø", "O", "OE", "" ] },
            { "ø", [ "ø", "o", "oe", "" ] },
            { "Ổ", [ "Ổ", "O", "O", "" ] },
            { "ổ", [ "ổ", "o", "o", "" ] },
            { "Ợ", [ "Ợ", "O", "O", "" ] },
            { "ợ", [ "ợ", "o", "o", "" ] },
            { "Œ", [ "Œ", "OE", "OE", "" ] },
            { "œ", [ "œ", "oe", "oe", "" ] },
            { "Ś", [ "Ś", "S", "S", "" ] },
            { "ś", [ "ś", "s", "s", "" ] },
            { "Š", [ "Š", "S", "S", "" ] },
            { "š", [ "š", "s", "s", "" ] },
            { "Ş", [ "Ş", "S", "S", "" ] },
            { "ş", [ "ş", "s", "s", "" ] },
            { "ß", [ "ß", "s", "ss", "" ] },
            { "Ú", [ "Ú", "U", "U", "" ] },
            { "ú", [ "ú", "u", "u", "" ] },
            { "Ù", [ "Ù", "U", "U", "" ] },
            { "ù", [ "ù", "u", "u", "" ] },
            { "Ü", [ "Ü", "U", "UE", "" ] },
            { "ü", [ "ü", "u", "ue", "" ] },
            { "Ů", [ "Ů", "U", "UU", "" ] },
            { "ů", [ "ů", "u", "uu", "" ] },
            { "Ư", [ "Ư", "U", "U", "" ] },
            { "ư", [ "ư", "u", "u", "" ] },
            { "Ứ", [ "Ứ", "U", "U", "" ] },
            { "ứ", [ "ứ", "u", "u", "" ] },
            { "Ý", [ "Ý", "Y", "Y", "" ] },
            { "ý", [ "ý", "y", "y", "" ] },
            { "Ỳ", [ "Ỳ", "Y", "Y", "" ] },
            { "ỳ", [ "ỳ", "y", "y", "" ] },
            { "Ÿ", [ "Ÿ", "Y", "Y", "" ] },
            { "ÿ", [ "ÿ", "y", "y", "" ] },
            { "Ż", [ "Ż", "Z", "Z", "" ] },
            { "ż", [ "ż", "z", "z", "" ] }
        };


    protected List<string> ReplaceDiacritics(string text)
    {
        const int MAX_DIACRITIC_REPLACEMENTS = 4;

        if (DiacriticsReplacements.IsNullOrEmpty())
        {
            return [text];
        }

        List<string> result = [];

        for (int dcf = 0; dcf < MAX_DIACRITIC_REPLACEMENTS; dcf++)
        {
            var replacements = DiacriticsReplacements!.Where(replacement => !string.IsNullOrEmpty(replacement.Value[dcf]));
            if(replacements.IsNullOrEmpty())
            {
                continue;
            }

            string resultItem = text;
            foreach (var replacement in replacements)
            {
                string fromValue = replacement.Key;
                string toValue = replacement.Value[dcf];
                resultItem = resultItem.Replace(fromValue, toValue);
            }
            result.Add(resultItem);
        }

        result = result.Distinct().ToList();
        return result;
    }


    /* 
    // Lazy algorithm (not used, because it is wrong when there is more than one diacritic character inside text)
    protected List<string> ReplaceDiacritics(string text)
    {
        if (DiacriticsReplacements.IsNullOrEmpty())
        {
            return [text];
        }

        List<string> result = [];
        foreach (DiacriticReplacements diacriticReplacements in DiacriticsReplacements!)
        {
            string diacritic = diacriticReplacements.FromValue;
            foreach (string replacement in diacriticReplacements.ToValues)
            {
                result.Add(text.Replace(diacritic, replacement));
            }
        }

        result = result.Distinct().ToList();

        return result;
    }
    */


#pragma warning disable CA2211 // Non-constant fields should not be visible
    protected static Func<string, bool> IsLongWord = (word) => word.Length >= LONG_LENGTH;
#pragma warning restore CA2211 // Non-constant fields should not be visible


    protected virtual string StandardizePunctuation(string text)
    {
        // Makes sure there is a space after each comma, and none before; 
        // and Remove leading and trailing spaces, as well as multiple spaces between allWords
        foreach (string nonStdComma in NON_STANDARD_COMMAS)
        {
            text = text.Replace(nonStdComma, STD_COMMA);
        }
        return text.FullTrim();
    }


    public abstract string ReplaceCustomSubstrings(string text);


    protected virtual List<string> ReplaceCustomSubstrings(List<string> inputList)
    {
        if (SubstringReplacements.IsNullOrEmpty())
        {
            return inputList;
        }

        List<string> result = inputList
                                .Select(keyword => keyword.Replace(STD_COMMA, $" {STD_COMMA} ").FullTrim())
                                .Select(ReplaceCustomSubstrings)
                                .Select(keyword => keyword.FullTrim().Replace($" {STD_COMMA}", $"{STD_COMMA}"))
                                .Distinct().ToList();

        return result;
    }


    protected virtual string ReplaceSpecialCharacters(string text)
    {
        if (SubstringReplacements.IsNullOrEmpty())
        {
            return text;
        }

        return text.ReplaceAll(SpecialCharacterReplacements!).FullTrim();
    }


    protected virtual string ShortenName(string text)
    {
        const int TARGET_COUNT = 2;
        string[] allWords = text.Split(' ');
        
        // Tries to accumulate TARGET_COUNT words with at least LONG_LENGTH characters long.
        int wordsCount = 0;
        int longWordsCount = 0;
        foreach(string word in allWords)
        {
            wordsCount++;
            longWordsCount += IsLongWord(word) ? 1 : 0;
            if (longWordsCount >= TARGET_COUNT)
            {
                return string.Join(' ', allWords.Take(wordsCount));
            }
        }

        // If the code reaches this point, by default returns a string comprised of allWords.
        return string.Join(' ', allWords);
    }


    protected virtual List<string> RestoreOriginalNameForTooShortKeywords(List<string> inputList, string originalName)
    {
        List<string> result = [];

        if(inputList.IsNullOrEmpty())
        {
            return string.IsNullOrWhiteSpace(originalName) ? result : [originalName];
        }

        foreach(string keyword in inputList)
        {
            result.Add(keyword.Length < LONG_LENGTH ? originalName : keyword);
        }

        result = result.Distinct().ToList();
        return result;
    }


    public static string RemoveSpecialCharacters(string text, List<char> exceptionList = null)
    {
        if (SpecialCharactersFullList.IsNullOrEmpty())
        {
            return text;
        }

        exceptionList ??= [];

        return text.ReplaceAll(SpecialCharactersFullList
                                 .Where(character => !exceptionList.Contains(character))
                                 .Select(character => new KeyValuePair<string, string>(character.ToString(), " "))
                                 .ToDictionary()).FullTrim();
    }


    protected static List<char> SpecialCharactersFullList =>
        [ '`', '~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '‐', '–', '_', '=', '+', '[', '{', ']',
          '}', '\\', '|', ';', ':', '\'', '’', '´', '"', '“', '”', '«',  '»', ',', '<', '.', '>', '/', '?' ];


    public static string SurroundSpecialCharactersWithSpaces(string text)
    {
        if (SpecialCharactersFullList.IsNullOrEmpty())
        {
            return text;
        }

        return text.ReplaceAll(SpecialCharactersFullList
                                    .Select(c => new KeyValuePair<string, string>(c.ToString(), $" {c} "))
                                    .ToDictionary()).FullTrim();
    }
}
