using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ConflictAutomation.Extensions;

public static class StringExtensions
{
    private static readonly string[] ptBRprepositions = ["da", "de", "do", "das", "dos", "e"];


    public static string ToProperCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        string[] words = input.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 1)
            {
                words[i] = words[i][0].ToString().ToUpper() + words[i][1..].ToLower();
            }
            else
            {
                words[i] = words[i].ToUpper();
            }
        }

        return string.Join(' ', words);
    }

    public static string ToLower(this string input, IEnumerable<string> prepositions)
    {
        string[] lowerCasePrepositions = prepositions.Select(preposition => preposition.ToLower()).ToArray();

        IEnumerable<string> words = input.Split(' ');
        words = words.Select(word =>
                    Array.IndexOf(lowerCasePrepositions, word.ToLower()) < 0 ? word : word.ToLower()
                );

        return string.Join(' ', words);
    }

    public static string ToPtBrProperName(this string name) =>
        name.ToProperCase().ToLower(ptBRprepositions);

    public static string StrLeft(this string input, string delimiter, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        int pos = input.IndexOf(delimiter, stringComparison);
        if (pos < 0)
        {
            return string.Empty;
        }

        return input[..pos];
    }

    public static string StrRight(this string input, string delimiter, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        int pos = input.IndexOf(delimiter, stringComparison);
        if (pos < 0)
        {
            return string.Empty;
        }

        return input[(pos + delimiter.Length)..];
    }

    public static string StrLeftBack(this string input, string delimiter, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        int pos = input.LastIndexOf(delimiter, stringComparison);
        if (pos < 0)
        {
            return string.Empty;
        }

        return input[..pos];
    }

    public static string StrRightBack(this string input, string delimiter, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        int pos = input.LastIndexOf(delimiter, stringComparison);
        if (pos < 0)
        {
            return string.Empty;
        }

        return input[(pos + delimiter.Length)..];
    }

    public static string StrMid(this string input, string delimiterLeft, string delimiterRight, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        int posLeft = input.IndexOf(delimiterLeft, stringComparison);
        if (posLeft < 0)
        {
            return string.Empty;
        }

        int posRight = input.IndexOf(delimiterRight, posLeft + delimiterLeft.Length, stringComparison);
        if (posRight < 0)
        {
            return string.Empty;
        }

        if (posLeft >= posRight)
        {
            return string.Empty;
        }

        return input[(posLeft + delimiterLeft.Length)..posRight];
    }

    public static string StrMidBack(this string input, string delimiterLeft, string delimiterRight, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        int posRight = input.LastIndexOf(delimiterRight, stringComparison);
        if (posRight < 0)
        {
            return string.Empty;
        }

        int posLeft = input.LastIndexOf(delimiterLeft, posRight - delimiterRight.Length, stringComparison);
        if (posLeft < 0)
        {
            return string.Empty;
        }

        if (posLeft >= posRight)
        {
            return string.Empty;
        }

        return input[(posLeft + delimiterLeft.Length)..posRight];
    }

    public static string Reverse(this string input)
    {
        char[] chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    public static string FullTrim(this string input)
    {
        if(input.IsNullOrEmpty())
        {
            return string.Empty;
        }

        string result = input.Trim();
        while (result.Contains("  "))
        {
            result = result.Replace("  ", " ");
        }

        return result;
    }


    public static int Count(this string input, string substring, bool ignoreCase = true)
    {
        if (input.IsNullOrEmpty())
        {
            return 0;
        }

        return Regex.Matches(input, substring, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None).Count;
    }


    public static bool Contains(this string input, IEnumerable<string> substrings, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        if (input.IsNullOrEmpty())
        {
            return false;
        }

        return substrings.Where(substring => input.Contains(substring, stringComparison)).Any();
    }


    public static string ReplaceAll(this string input, Dictionary<string, string> replacements, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        string result = input;

        foreach (var replacement in replacements)
        {
            result = result.Replace(replacement.Key, replacement.Value, stringComparison);
        }

        return result;
    }


    public static string RemoveAll(this string input, IEnumerable<string> removals, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        string result = input;

        foreach (string removal in removals)
        {
            result = result.Replace(removal, string.Empty, stringComparison);
        }

        return result;
    }


    public static string UnencodeUnicodeChars(this string input)
    {
        string result = input;

        result = result.Replace("\\u00C1", "Á", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E1", "á", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00C0", "À", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E0", "à", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00C2", "Â", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E2", "â", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00C4", "Ä", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E4", "ä", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00C3", "Ã", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E3", "ã", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00C5", "Å", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E5", "å", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EA6", "Ầ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EA7", "ầ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EA2", "Ả", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EA3", "ả", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EAC", "Ậ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EAD", "ậ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00C6", "Æ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E6", "æ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u010C", "Č", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u010D", "č", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00C7", "Ç", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E7", "ç", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0110", "Đ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0111", "đ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00C9", "É", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E9", "é", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00C8", "È", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00E8", "è", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00CA", "Ê", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00EA", "ê", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00CB", "Ë", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00EB", "ë", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u011A", "Ě", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u011B", "ě", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0118", "Ę", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0119", "ę", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EC2", "Ể", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EC3", "ể", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0120", "Ġ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0121", "ġ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u011E", "Ğ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u011F", "ğ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0126", "Ħ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0127", "ħ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00CD", "Í", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00ED", "í", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00CC", "Ì", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00EC", "ì", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00CE", "Î", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00EE", "î", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00CF", "Ï", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00EF", "ï", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0128", "Ĩ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0129", "ĩ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u012A", "Ī", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u012B", "ī", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0141", "Ł", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0142", "ł", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0143", "Ń", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0144", "ń", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00D1", "Ñ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00F1", "ñ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0145", "Ņ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0146", "ņ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00D3", "Ó", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00F3", "ó", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00D2", "Ò", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00F2", "ò", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00D4", "Ô", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00F4", "ô", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00D6", "Ö", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00F6", "ö", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u014C", "Ō", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u014D", "ō", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00D5", "Õ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00F5", "õ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1ED2", "Ồ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1ED3", "ồ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00D8", "Ø", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00F8", "ø", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1ED4", "Ổ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1ED5", "ổ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EE2", "Ợ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EE3", "ợ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0152", "Œ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0153", "œ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u015A", "Ś", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u015B", "ś", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0160", "Š", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0161", "š", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u015E", "Ş", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u015F", "ş", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00DF", "ß", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00DA", "Ú", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00FA", "ú", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00D9", "Ù", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00F9", "ù", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00DC", "Ü", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00FC", "ü", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u016E", "Ů", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u016F", "ů", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u01AF", "Ư", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u01B0", "ư", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EE8", "Ứ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EE9", "ứ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00DD", "Ý", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00FD", "ý", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EF2", "Ỳ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u1EF3", "ỳ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u0178", "Ÿ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u00FF", "ÿ", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u017B", "Ż", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("\\u017C", "ż", StringComparison.OrdinalIgnoreCase);

        return result;
    }


    public static string ConvertDiacriticsToStandardAnsi(this string input)
    {
        // Adjust the exceptional cases not covered by 
        // .NET's string.Normalize(NormalizationForm.FormD)
        input = input.Replace("Å", "U")
                     .Replace("å", "u")
                     .Replace("Æ", "AE")
                     .Replace("æ", "ae")
                     .Replace("ß", "ss")
                     .Replace("Đ", "D")
                     .Replace("đ", "d")
                     .Replace("Ħ", "H")
                     .Replace("ħ", "h")
                     .Replace("Ł", "L")
                     .Replace("ł", "l")
                     .Replace("Œ", "OE")
                     .Replace("œ", "oe")
                     .Replace("Ø", "O")
                     .Replace("ø", "o");

        // Normalization to FormD converts diacritics to 
        // letters + accents (full canonical decomposition)
        var normalizedText = input.Normalize(NormalizationForm.FormD);

        // Removes all diacritics (accents) from normalizedText
        var normalizedTextWithoutAccents =
            normalizedText.Where(c =>
                CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

        // Creates a new string from the remaining chars
        var result = new string(normalizedTextWithoutAccents.ToArray());

        return result;
    }


    public static bool NotEquals(this string textA, string textB, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) =>
        !string.Equals(textA, textB, comparisonType);


    public static string RemovePrefixes(
        this string text, string prefix,
        StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        while (text.StartsWith(prefix, stringComparison))
        {
            text = text[prefix.Length..];
        }
        return text;
    }


    public static int TryParseIntWithDefault(this string value, int defaultValue = 0)
    {
        if (int.TryParse(value, out int result))
        {
            return result;
        }
        return defaultValue;
    }


    public static string ReplaceFirstOccurrence(this string input, string fromSubstring, string toSubstring,
    StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        int pos = input.IndexOf(fromSubstring, stringComparison);
        if (pos < 0)
        {
            return input;
        }

        return input.Remove(pos, fromSubstring.Length).Insert(pos, toSubstring);
    }


    public static string ReplaceLastOccurrence(this string input, string fromSubstring, string toSubstring,
        StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        int pos = input.LastIndexOf(fromSubstring, stringComparison);
        if (pos < 0)
        {
            return input;
        }

        return input.Remove(pos, fromSubstring.Length).Insert(pos, toSubstring);
    }


    public static string GetDigitsOnly(this string input) => new(input.Where(c => c.IsDigit()).ToArray());


    public static string GetLettersOnly(this string input) => new(input.Where(c => c.IsLetter()).ToArray());


    public static string Left(this string input, int length) =>
        string.IsNullOrEmpty(input) || (length < 1) ? string.Empty
            : input.Substring(0, Math.Min(length, input.Length));

    public static string Right(this string input, int length) =>
        string.IsNullOrEmpty(input) || (length < 1) ? string.Empty
            : input.Substring(Math.Max(0, input.Length - length));

    public static string Mid(this string input, int startIndex, int length) =>
        string.IsNullOrEmpty(input) || (length < 1) ? string.Empty
            : input.Substring(startIndex, Math.Min(length, input.Length - startIndex));
}
