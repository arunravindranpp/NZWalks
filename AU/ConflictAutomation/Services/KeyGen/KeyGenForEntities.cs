using ConflictAutomation.Extensions;
using System.Text.RegularExpressions;

namespace ConflictAutomation.Services.KeyGen;

public class KeyGenForEntities : KeyGen
{
    protected const string AMPERSAND = "&";
    protected const string REGULAR_AND = "and";

    public List<string> PrefixesToBeRemoved { get; set; }

    public List<string> GeoLocationNames { get; set; }


    public KeyGenForEntities(
            Dictionary<string, string> substringReplacements = null,
            Dictionary<string, string> specialCharacterReplacements = null,
            Dictionary<string, string[]> diacriticsReplacements = null,
            List<string> prefixesToBeRemoved = null,
            List<string> geoLocationNames = null
           ) : base(substringReplacements, specialCharacterReplacements, diacriticsReplacements)
    {
        SubstringReplacements = substringReplacements ?? DefaultValueSubstringReplacements;
        SpecialCharacterReplacements = specialCharacterReplacements ?? DefaultValueSpecialCharacterReplacements;
        PrefixesToBeRemoved = prefixesToBeRemoved ?? DefaultValueForPrefixesToBeRemoved;
        GeoLocationNames = geoLocationNames ?? DefaultValueForGeoLocationNames;
    }


    public override List<string> GenerateKey(string entityName)
    {
        entityName = entityName?.FullTrim();
        if (string.IsNullOrEmpty(entityName))
        {
            return [string.Empty];
        }

        string name = entityName;

        // Recommended sequence of operations: 
        //   (KeyGen)             1. Fix unusual things related to applications and OS; 
        //   (KeyGen)             2. Standardize dashes; 
        //   (KeyGenForEntities)  3. Perform removal of custom prefixes; 
        //   (KeyGenForEntities)  4. Perform removal of LegalExtensions from the very beginning or very end; 
        //   (KeyGen)             5. Standardize punctuation that may deviate due to user input; 
        //   (KeyGen)             6. Perform replacements of diacritics characters; 
        //   (KeyGenForEntities)  7. Perform replacements of special characters(not necessarily surrounded by spaces);
        //   (KeyGenForEntities)  8. Shorten the inputList (uses KeyGen.ShortenName()).
        //   (KeyGenForEntities)  9. Handle special cases.
        //   (KeyGenForEntities) 10. Provide special variations.
        //   (KeyGen)            11. Restore original text for too short keywords.

        name = FixNonBreakingSpaces(name);  // #1
        name = FixTabs(name);  // #1
        name = StandardizeDashes(name);  // #2
        name = StandardizeDots(name);  // #2
        name = RemoveCustomPrefixes(name);  // #3
        name = ReplaceLegalExtensionsFromVeryBeginningOrVeryEnd(name);  // #4
        name = StandardizePunctuation(name);  // #5

        List<string> result;
        result = ReplaceDiacritics(name);  // #6       
        result = ReplaceSpecialCharactersAllCombinations(result);  // #7
        result = ShortenKeywordsNotStartingWithGeoLocations(result);  // #8
        result = HandleSpecialCases(result);  // #9
        result = ProvideDashVariations(result);  // #10
        result = ProvideBracketVariations(result);  // #10
        result = RestoreOriginalNameForTooShortKeywords(result, entityName);  // #11

        result = result.Distinct().ToList();
        return result;
    }


    public List<string> GenerateKeyForFinScanSearch(string entityName)
    {
        List<string> result = [entityName];

        result.AddRange(GenerateKey(entityName));

        result = result.Distinct().ToList();
        return result;
    }


    private string ReplaceLegalExtensionsFromVeryBeginningOrVeryEnd(string text)
    {
        if (!SubstringReplacements.IsNullOrEmpty())
        {
            text = $" {text.FullTrim()} ";

            var effectiveReplacements = SubstringReplacements!.Where(r => r.Key.NotEquals(r.Value)).ToList();

            foreach (var replacement in effectiveReplacements)
            {
                if (text.StartsWith($" {replacement.Key} ", StringComparison.OrdinalIgnoreCase))
                {
                    text = replacement.Value + text.StrRight(replacement.Key);
                    break;
                }
            }

            foreach (var replacement in effectiveReplacements)
            {
                if (text.EndsWith($" {replacement.Key} ", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.StrLeftBack(replacement.Key) + replacement.Value;
                    break;
                }
            }
        }
        
        text = text.FullTrim();

        string result = SurroundEachSubstringWithWhiteSpaces(text,
                            ["(", ")", "{", "}", "[", "]", "<", ">", ",", ";", ":"]);
        result = " " + result.FullTrim() + " ";        
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


    protected static Dictionary<string, string> DefaultValueSubstringReplacements =>
        new()
        {
            { "(publ)", " " },
            { ".in.etwas.der.deutschen.gmbh", " " },
            { ".in.etwas.der.deutschen.gmbh.", " " },
            { "/adr/", " " },
            { "/de", " " },
            { "/fi", " " },
            { "/fi.", " " },
            { "/new", " " },
            { "/new/", " " },
            { "\"?", " " },
            { ">,.", " " },
            { "000", " " },
            { "a", " " },
            { "a. en p.", " " },
            { "a.b", " " },
            { "a.b.", " " },
            { "a.c", " " },
            { "a.c.", " " },
            { "a.c.e", " " },
            { "a.c.e.", " " },
            { "a.d", " " },
            { "a.d.", " " },
            { "a.e", " " },
            { "a.e.", " " },
            { "a.g", " " },
            { "a.g.", " " },
            { "a.l", " " },
            { "a.l.", " " },
            { "a.n.s", " " },
            { "a.n.s.", " " },
            { "a.o", " " },
            { "a.o.", " " },
            { "a.p.b", " " },
            { "a.p.b.", " " },
            { "a.p.s", " " },
            { "a.p.s.", " " },
            { "a.s", " " },
            { "a.s.", " " },
            { "a.s.a", " " },
            { "a.s.a.", " " },
            { "a.v.v", " " },
            { "a.v.v.", " " },
            { "a.x.a", " " },
            { "a.x.a.", " " },
            { "a/b", " " },
            { "a/c", " " },
            { "a/d", " " },
            { "a/e", " " },
            { "a/g", " " },
            { "a/l", " " },
            { "a/o", " " },
            { "a/p/s", " " },
            { "a/s", " " },
            { "ab", " " },
            { "ab.", " " },
            { "ac", " " },
            { "ac.", " " },
            { "acciones", " " },
            { "accnt", " " },
            { "account", " " },
            { "acct", " " },
            { "ace", " " },
            { "ace.", " " },
            { "acq", " " },
            { "acq.", " " },
            { "acquisition", " " },
            { "ad", " " },
            { "adr", " " },
            { "adr.", " " },
            { "adviser", " " },
            { "advisor", " " },
            { "advisory", " " },
            { "ae", " " },
            { "ag", " " },
            { "ag.", " " },
            { "aktien", " " },
            { "aktien.", " " },
            { "aktiengesellschaft", " " },
            { "aktiengesellschaft.", " " },
            { "aktiengesellschaft.adr", " " },
            { "aktiengesellschaft/", " " },
            { "aktieselskab", " " },
            { "aktieselskab.", " " },
            { "aktinegesellschaft", " " },
            { "aktsionernoe", " " },
            { "aktsionernoe.", " " },
            { "akzionernoje", " " },
            { "al", " " },
            { "alliance", " " },
            { "amba", " " },
            { "andelsselskab", " " },
            { "andelsselskab.", " " },
            { "anonim", " " },
            { "anonim.", " " },
            { "anónima", " " },
            { "anpartsselskab", " " },
            { "anpartsselskab.", " " },
            { "ans", " " },
            { "ans.", " " },
            { "ao", " " },
            { "ao.", " " },
            { "apb", " " },
            { "aps", " " },
            { "aps & co. k/s", " " },
            { "aps.&.co.k/s", " " },
            { "aps.co.k/s", " " },
            { "as", " " },
            { "as.", " " },
            { "asa", " " },
            { "asset", " " },
            { "auf", " " },
            { "avv", " " },
            { "axa", " " },
            { "b", " " },
            { "b.p.k", " " },
            { "b.p.k.", " " },
            { "b.t", " " },
            { "b.t.", " " },
            { "b.v", " " },
            { "b.v.", " " },
            { "b.v.b.a", " " },
            { "b.v.b.a.", " " },
            { "b/t", " " },
            { "b/v", " " },
            { "bank", " " },
            { "beschränkter", " " },
            { "beschränkter.", " " },
            { "beteiligungs", " " },
            { "beteiligungs.", " " },
            { "bgb.gesellschaft", " " },
            { "bgb.gesellschaft.", " " },
            { "bgbgesellschaft", " " },
            { "bgb-gesellschaft", " " },
            { "bgbgesellschaft.", " " },
            { "bgb-gesellschaft.", " " },
            { "bhd", " " },
            { "bhd.", " " },
            { "body", " " },
            { "bpk", " " },
            { "branch", " " },
            { "bt", " " },
            { "business", " " },
            { "bv", " " },
            { "bv.", " " },
            { "bvba", " " },
            { "c", " " },
            { "c.a", " " },
            { "c.a.", " " },
            { "c.i.n", " " },
            { "c.i.n.", " " },
            { "c.j.s.c", " " },
            { "c.j.s.c.", " " },
            { "c.o", " " },
            { "c.o.", " " },
            { "c.v", " " },
            { "c.v.", " " },
            { "c.v.a", " " },
            { "c.v.a.", " " },
            { "c.v.o.a", " " },
            { "c.v.o.a.", " " },
            { "c.v.oa", " " },
            { "c.v.oa.", " " },
            { "c/a", " " },
            { "c/v", " " },
            { "ca", " " },
            { "ca.", " " },
            { "caixa", " " },
            { "can", " " },
            { "canada", " " },
            { "capital", " " },
            { "cedel", " " },
            { "cin", " " },
            { "cin.", " " },
            { "city", " " },
            { "cjsc", " " },
            { "cjsc.", " " },
            { "closed", " " },
            { "club", " " },
            { "co", " " },
            { "co.", " " },
            { "co.,", " " },
            { "co.,ltd.", " " },
            { "co.de", " " },
            { "co.limited", " " },
            { "co/", " " },
            { "co/de", " " },
            { "code", " " },
            { "colectiva", " " },
            { "colimited", " " },
            { "com", " " },
            { "comanditaria", " " },
            { "common", " " },
            { "compania", " " },
            { "company", " " },
            { "cooperatief", " " },
            { "cooperatief.", " " },
            { "cooperatief.ua", " " },
            { "cooperatiefua", " " },
            { "co-operative", " " },
            { "corp", " " },
            { "corp.", " " },
            { "corp.p.ltd", " " },
            { "corp.p.ltd.", " " },
            { "corp.sdh.corp.bv", " " },
            { "corp.sdh.corp.bv.", " " },
            { "corp.sdhcorp.bv", " " },
            { "corp.sdhcorp.bv.", " " },
            { "corp.sucursal.en", " " },
            { "corp/new/", " " },
            { "corporation", " " },
            { "corporation.", " " },
            { "country", " " },
            { "county", " " },
            { "cusip", " " },
            { "cv", " " },
            { "cv.", " " },
            { "cv.oa", " " },
            { "cv.oa.", " " },
            { "cv/", " " },
            { "cva", " " },
            { "cvoa", " " },
            { "d", " " },
            { "d.a", " " },
            { "d.a.", " " },
            { "d.b.a", " " },
            { "d.b.a.", " " },
            { "d.d", " " },
            { "d.d.", " " },
            { "d.e", " " },
            { "d.e.", " " },
            { "d.n.o", " " },
            { "d.n.o.", " " },
            { "d.o.o", " " },
            { "d.o.o.", " " },
            { "d/a", " " },
            { "d/b/a", " " },
            { "d/d", " " },
            { "d/n/o", " " },
            { "d/o/o", " " },
            { "da", " " },
            { "da.", " " },
            { "dba", " " },
            { "dd", " " },
            { "dd.", " " },
            { "de", " " },
            { "de.", " " },
            { "debt", " " },
            { "department", " " },
            { "dept", " " },
            { "der", " " },
            { "deutschen", " " },
            { "deutschen.", " " },
            { "distribution", " " },
            { "district", " " },
            { "dno", " " },
            { "dno.", " " },
            { "doctor", " " },
            { "doo", " " },
            { "doo.", " " },
            { "dopolnitelnoj", " " },
            { "e", " " },
            { "e.e", " " },
            { "e.e.", " " },
            { "e.e.g", " " },
            { "e.e.g.", " " },
            { "e.i.r.l", " " },
            { "e.i.r.l.", " " },
            { "e.l.p", " " },
            { "e.l.p.", " " },
            { "e.o.o.d", " " },
            { "e.o.o.d.", " " },
            { "e.p.e", " " },
            { "e.p.e.", " " },
            { "e.u.r.l", " " },
            { "e.u.r.l.", " " },
            { "e.v", " " },
            { "e.v.", " " },
            { "e.w", " " },
            { "e.w.", " " },
            { "e/e/g", " " },
            { "e/l/p", " " },
            { "e/p/e", " " },
            { "e/v", " " },
            { "ee", " " },
            { "ee.", " " },
            { "eeg", " " },
            { "eirl", " " },
            { "eirl.", " " },
            { "electronics", " " },
            { "elp", " " },
            { "empresa", " " },
            { "en", " " },
            { "en.", " " },
            { "enkeltmandsfirma", " " },
            { "enkeltmandsfirma.", " " },
            { "enr", " " },
            { "enterprise", " " },
            { "entrpich", " " },
            { "entrspricht", " " },
            { "entrspricht.", " " },
            { "entspricht", " " },
            { "entspricht.", " " },
            { "entspricht.der.aktiengesellschaft", " " },
            { "entspricht.der.aktiengesellschaft.", " " },
            { "entspricht.in.etwa.der.deutschen.ohg.oder.bgb-gesellschaft", " " },
            { "entspricht.in.etwa.der.deutschen.ohg.oder.bgb-gesellschaft.", " " },
            { "entspricht.in.etwa.der.kommanditgesellschaft.auf.aktien", " " },
            { "entspricht.in.etwa.der.kommanditgesellschaft.auf.aktien.", " " },
            { "entspricht.in.etwa.kommanditgesellschaft", " " },
            { "entspricht.in.etwa.kommanditgesellschaft.", " " },
            { "eood", " " },
            { "eood.", " " },
            { "epe", " " },
            { "equity", " " },
            { "etwa", " " },
            { "etwa.", " " },
            { "etwas", " " },
            { "etwas.", " " },
            { "eurl", " " },
            { "euroclear", " " },
            { "europäischer", " " },
            { "europäischer.", " " },
            { "europea", " " },
            { "european", " " },
            { "ev", " " },
            { "ev.", " " },
            { "ew", " " },
            { "f", " " },
            { "f.p", " " },
            { "f.z.e", " " },
            { "f.z.e.", " " },
            { "f/z/e", " " },
            { "fi", " " },
            { "filial", " " },
            { "firm", " " },
            { "fond", " " },
            { "forening", " " },
            { "foundation", " " },
            { "fund", " " },
            { "fundo", " " },
            { "fze", " " },
            { "fze.", " " },
            { "g", " " },
            { "g.b.r", " " },
            { "g.b.r.", " " },
            { "g.c.v", " " },
            { "g.c.v.", " " },
            { "g.i.e", " " },
            { "g.i.e.", " " },
            { "g.k", " " },
            { "g.k.", " " },
            { "g.m.b.h", " " },
            { "g.m.b.h.", " " },
            { "g.o.r.e", " " },
            { "g.o.r.e.", " " },
            { "g/b/r", " " },
            { "g/c/v", " " },
            { "g/i/e", " " },
            { "gbr", " " },
            { "gcv", " " },
            { "general", " " },
            { "general.", " " },
            { "general.partnership", " " },
            { "general.partnership.", " " },
            { "ges", " " },
            { "ges.", " " },
            { "ges.mbh", " " },
            { "ges.mbh.", " " },
            { "gesellschaft", " " },
            { "gesellschaft.", " " },
            { "gesellschaft.mit.beschränkter.haftung", " " },
            { "gesellschaft.mit.beschränkter.haftung.", " " },
            { "gesmbh", " " },
            { "gesmbh.", " " },
            { "gie", " " },
            { "gk", " " },
            { "global", " " },
            { "gmbh", " " },
            { "gmbh & co. kg", " " },
            { "gmbh&co", " " },
            { "gmbh&co.", " " },
            { "gmbh&co.kg", " " },
            { "gmbh&co.kg.", " " },
            { "gmbh&co.kommanditgesellschaft", " " },
            { "gmbh.co.kg", " " },
            { "gmbh.co.kg.", " " },
            { "gore", " " },
            { "group", " " },
            { "group.", " " },
            { "group.gmbh", " " },
            { "group.gmbh.", " " },
            { "grundstucksverwaltungsgesellschaft", " " },
            { "grundstucksverwaltungsgesellschaft.", " " },
            { "grundstucksverwaltungsgesellschaft.mbh.co.beteiligungs.kg", " " },
            { "grundstucksverwaltungsgesellschaft.mbh.co.beteiligungs.kg.", " " },
            { "h", " " },
            { "h.b", " " },
            { "h.b.", " " },
            { "h.f", " " },
            { "h.f.", " " },
            { "h/b", " " },
            { "h/f", " " },
            { "haftung", " " },
            { "haftung.", " " },
            { "handelsgesellschaft", " " },
            { "handelsgesellschaft.", " " },
            { "hb", " " },
            { "hf", " " },
            { "hf.", " " },
            { "holding", " " },
            { "holdings", " " },
            { "holdings.", " " },
            { "holdings/equities", " " },
            { "hospital", " " },
            { "i", " " },
            { "i.b.c.", " " },
            { "i.d.", " " },
            { "i.n.c.", " " },
            { "i.s.", " " },
            { "i.s.i.n.", " " },
            { "i/d", " " },
            { "i/s", " " },
            { "ibc", " " },
            { "id", " " },
            { "ii", " " },
            { "iii", " " },
            { "in", " " },
            { "inc", " " },
            { "inc.", " " },
            { "inc/new/de/", " " },
            { "incorp", " " },
            { "incorporated", " " },
            { "incorporation", " " },
            { "ing", " " },
            { "ing.", " " },
            { "institution", " " },
            { "interessentselskab", " " },
            { "international", " " },
            { "intl", " " },
            { "investment", " " },
            { "investments", " " },
            { "invt", " " },
            { "ir", " " },
            { "ir.", " " },
            { "is", " " },
            { "isin", " " },
            { "iv", " " },
            { "j", " " },
            { "j.s.c", " " },
            { "j.s.c.", " " },
            { "j.t.d.", " " },
            { "joint", " " },
            { "joint-stock", " " },
            { "jsc", " " },
            { "jtd", " " },
            { "jtd.", " " },
            { "k", " " },
            { "k.a.s", " " },
            { "k.b.", " " },
            { "k.d", " " },
            { "k.d.", " " },
            { "k.d.a", " " },
            { "k.d.d.", " " },
            { "k.f.t", " " },
            { "k.g", " " },
            { "k.g.", " " },
            { "k.g.a.a.", " " },
            { "k.k.", " " },
            { "k.k.t.", " " },
            { "k.s", " " },
            { "k.s.", " " },
            { "k.v", " " },
            { "k.v.", " " },
            { "k.y", " " },
            { "k.y.", " " },
            { "k/b", " " },
            { "k/d", " " },
            { "k/d/a", " " },
            { "k/d/d", " " },
            { "k/g", " " },
            { "k/k", " " },
            { "k/k/t.", " " },
            { "k/s", " " },
            { "k/y", " " },
            { "ka.s", " " },
            { "ka/s", " " },
            { "kb", " " },
            { "kd", " " },
            { "kda", " " },
            { "kdd", " " },
            { "kenn-nummer", " " },
            { "kft", " " },
            { "kg", " " },
            { "kg.", " " },
            { "kgaa", " " },
            { "kk", " " },
            { "kk.", " " },
            { "kkt", " " },
            { "kol.", " " },
            { "kol. srk", " " },
            { "kom.", " " },
            { "kom. srk", " " },
            { "kommanditges.", " " },
            { "kommanditgesellschaft", " " },
            { "kommanditgesellschaft.", " " },
            { "kommanditselskab", " " },
            { "kommune", " " },
            { "ks", " " },
            { "kv", " " },
            { "ky", " " },
            { "ky.", " " },
            { "l", " " },
            { "l.d.a", " " },
            { "l.d.c", " " },
            { "l.l.c", " " },
            { "l.l.c.", " " },
            { "l.l.p", " " },
            { "l.p", " " },
            { "l.p.", " " },
            { "l.t.d", " " },
            { "l/d/a", " " },
            { "l/d/c", " " },
            { "laboral", " " },
            { "labour", " " },
            { "lda", " " },
            { "ldc", " " },
            { "liability", " " },
            { "life", " " },
            { "limitada", " " },
            { "limited", " " },
            { "limited.", " " },
            { "linked", " " },
            { "llc", " " },
            { "llc.", " " },
            { "llp", " " },
            { "lp", " " },
            { "ltd", " " },
            { "ltd.", " " },
            { "ltd/", " " },
            { "ltd/de", " " },
            { "ltda", " " },
            { "ltda.", " " },
            { "ltée", " " },
            { "ltée.", " " },
            { "m", " " },
            { "m.b.h", " " },
            { "m.b.h.", " " },
            { "management", " " },
            { "mbh", " " },
            { "mbh.", " " },
            { "mit", " " },
            { "mountain", " " },
            { "municipality", " " },
            { "n", " " },
            { "n.a.", " " },
            { "n.c", " " },
            { "n.c.", " " },
            { "n.p.c", " " },
            { "n.p.p", " " },
            { "n.p.p.", " " },
            { "n.t", " " },
            { "n.t.", " " },
            { "n.v", " " },
            { "n.v.", " " },
            { "n/a", " " },
            { "n/p/p", " " },
            { "n/t", " " },
            { "n/v", " " },
            { "na", " " },
            { "na.", " " },
            { "nauchno-proizvodstvennoe", " " },
            { "nc", " " },
            { "nc.", " " },
            { "nederland", " " },
            { "new", " " },
            { "nordisk", " " },
            { "npc", " " },
            { "npp", " " },
            { "nt", " " },
            { "nueva", " " },
            { "nv", " " },
            { "nv.", " " },
            { "o", " " },
            { "o.a.o", " " },
            { "o.a.o.", " " },
            { "o.d.o", " " },
            { "o.d.o.", " " },
            { "o.e", " " },
            { "o.e.", " " },
            { "o.h.g", " " },
            { "o.h.g.", " " },
            { "o.j.s.c.", " " },
            { "o.o.d", " " },
            { "o.o.d.", " " },
            { "o.o.o", " " },
            { "o.o.o.", " " },
            { "o.r.g.", " " },
            { "o.ü", " " },
            { "o.ü.", " " },
            { "o.y", " " },
            { "o.y.", " " },
            { "o.y.j", " " },
            { "o.y.j.", " " },
            { "o/d/o", " " },
            { "o/h/g", " " },
            { "o/ü", " " },
            { "o/y/j", " " },
            { "oa", " " },
            { "oao", " " },
            { "oao.", " " },
            { "obschestvo", " " },
            { "obschestvo.", " " },
            { "obschestvosdopolnitelnojotvetstvennostju", " " },
            { "obschtschestwo", " " },
            { "oder", " " },
            { "odo", " " },
            { "oe", " " },
            { "offene", " " },
            { "offene.", " " },
            { "ogranichennoy", " " },
            { "ohg", " " },
            { "ohg.", " " },
            { "oil", " " },
            { "ojsc", " " },
            { "one-man", " " },
            { "ood", " " },
            { "ooo", " " },
            { "open", " " },
            { "org", " " },
            { "org.", " " },
            { "organisation", " " },
            { "organization", " " },
            { "otkrytoe", " " },
            { "otvetstvennostju", " " },
            { "oü", " " },
            { "oü.", " " },
            { "oy", " " },
            { "oyj", " " },
            { "p", " " },
            { "p.a.o", " " },
            { "p.a.o.", " " },
            { "p.j.s.c.", " " },
            { "p.l.c", " " },
            { "p.l.c.", " " },
            { "p.m.a", " " },
            { "p.m.d.n", " " },
            { "p.r.c", " " },
            { "p.r.c.", " " },
            { "p.t.e", " " },
            { "p.t.y", " " },
            { "p.t.y.", " " },
            { "p.v.t", " " },
            { "p/l", " " },
            { "p/m/a", " " },
            { "p/m/d/n", " " },
            { "p/s", " " },
            { "p/t", " " },
            { "palace", " " },
            { "pao", " " },
            { "parent", " " },
            { "parthership", " " },
            { "partnership", " " },
            { "partnership.", " " },
            { "pc", " " },
            { "pc ltd", " " },
            { "pcltd", " " },
            { "pjsc", " " },
            { "pjsc.", " " },
            { "pl", " " },
            { "pl.", " " },
            { "plc", " " },
            { "plc.", " " },
            { "pma", " " },
            { "pma.", " " },
            { "pmdn", " " },
            { "pmdn.", " " },
            { "polnoe", " " },
            { "polnoetovarischestvo", " " },
            { "por", " " },
            { "portfolio", " " },
            { "prc", " " },
            { "predproyatie", " " },
            { "private", " " },
            { "production", " " },
            { "project", " " },
            { "proprietary", " " },
            { "proprietor", " " },
            { "prp", " " },
            { "prp.", " " },
            { "prp. ltd.", " " },
            { "ps", " " },
            { "ps.", " " },
            { "pt", " " },
            { "pte", " " },
            { "pte.", " " },
            { "pty", " " },
            { "pty.", " " },
            { "publ", " " },
            { "public", " " },
            { "publiclimitedcompany", " " },
            { "pvt", " " },
            { "pvt.", " " },
            { "q", " " },
            { "quebec", " " },
            { "r", " " },
            { "r.a.o", " " },
            { "r.a.s", " " },
            { "r.i.c", " " },
            { "r.j.s.c.", " " },
            { "r.l.", " " },
            { "r.o.", " " },
            { "r.t.", " " },
            { "r/o", " " },
            { "r/t", " " },
            { "rao", " " },
            { "rao.", " " },
            { "ras", " " },
            { "ras.", " " },
            { "region", " " },
            { "responsabilidad", " " },
            { "restaurant", " " },
            { "ric", " " },
            { "ric.", " " },
            { "rjsc", " " },
            { "rjsc.", " " },
            { "rl", " " },
            { "rl.", " " },
            { "ro", " " },
            { "ro.", " " },
            { "rossiskoje", " " },
            { "rossiskojeakzionernojeobschtschestwo", " " },
            { "rt", " " },
            { "rt.", " " },
            { "russian", " " },
            { "russianjointstockcompany", " " },
            { "s", " " },
            { "s. de r.l.", " " },
            { "s. en c.", " " },
            { "s. en n.c.", " " },
            { "s.a", " " },
            { "s.à", " " },
            { "s.a.", " " },
            { "s.a.de.cv.sofom.enr.", " " },
            { "s.a.e", " " },
            { "s.a.f.i", " " },
            { "s.a.i.c.a.", " " },
            { "s.a.o", " " },
            { "s.a.p.a.", " " },
            { "s.a.r.l.", " " },
            { "s.à.r.l.", " " },
            { "s.a.s.", " " },
            { "s.a.u", " " },
            { "s.c", " " },
            { "s.c.", " " },
            { "s.c.a.", " " },
            { "s.c.p.", " " },
            { "s.c.s", " " },
            { "s.c.s.", " " },
            { "s.com.", " " },
            { "s.de.r.l.de.c.v.", " " },
            { "s.e.", " " },
            { "s.e.c.", " " },
            { "s.e.d.o.l", " " },
            { "s.e.n.c.", " " },
            { "s.en.c.", " " },
            { "s.g.p.s.", " " },
            { "s.i.c.", " " },
            { "s.i.c.c.", " " },
            { "s.k.", " " },
            { "s.l.", " " },
            { "s.l.l.", " " },
            { "s.l.n.e.", " " },
            { "s.l.u.", " " },
            { "s.n.c", " " },
            { "s.n.c.", " " },
            { "s.p.", " " },
            { "s.p.a", " " },
            { "s.p.a.", " " },
            { "s.p.o.l.", " " },
            { "s.p.r.l.", " " },
            { "s.p.z.o.o.", " " },
            { "s.r.l.", " " },
            { "s.r.o.", " " },
            { "s.v.m.", " " },
            { "s/a", " " },
            { "s/a.", " " },
            { "s/a/o", " " },
            { "s/a/s", " " },
            { "s/e", " " },
            { "s/k", " " },
            { "s/l", " " },
            { "s/l/l", " " },
            { "s/l/u", " " },
            { "s/n/c", " " },
            { "sa", " " },
            { "sa de cv", " " },
            { "sa.", " " },
            { "sab", " " },
            { "sadecv", " " },
            { "sadecv/fi", " " },
            { "sadecv/fi.", " " },
            { "sadecvsofomenr", " " },
            { "sadecvsofomenr.", " " },
            { "sae", " " },
            { "sae.", " " },
            { "safi", " " },
            { "safi.", " " },
            { "saica", " " },
            { "saica.", " " },
            { "sakrytoje", " " },
            { "sakrytojeakzionernojeobschtschestwo", " " },
            { "sao", " " },
            { "sao.", " " },
            { "sapa", " " },
            { "sapa.", " " },
            { "sarl", " " },
            { "sàrl", " " },
            { "sarl.", " " },
            { "sarlycompanialimitada", " " },
            { "sas", " " },
            { "sas.", " " },
            { "sau", " " },
            { "sau.", " " },
            { "sc", " " },
            { "sc.", " " },
            { "sca", " " },
            { "sca.", " " },
            { "scientific", " " },
            { "scientificproductionenterprise", " " },
            { "scom", " " },
            { "scompora", " " },
            { "scp", " " },
            { "scp.", " " },
            { "scs", " " },
            { "scs.", " " },
            { "sderl", " " },
            { "sderl.", " " },
            { "sderldecv", " " },
            { "sderldecv.", " " },
            { "sdh", " " },
            { "sdh.", " " },
            { "sdhcorp", " " },
            { "sdhcorp.", " " },
            { "sdn", " " },
            { "sdn bhd", " " },
            { "sdn.", " " },
            { "sdnbhd", " " },
            { "sdnbhd.", " " },
            { "se", " " },
            { "se.", " " },
            { "sec", " " },
            { "sec.", " " },
            { "sedol", " " },
            { "sedol.", " " },
            { "senc", " " },
            { "senc.", " " },
            { "sep acc", " " },
            { "sep account", " " },
            { "separate", " " },
            { "service", " " },
            { "servicecorporation", " " },
            { "serviceincorp", " " },
            { "services", " " },
            { "servicescorp", " " },
            { "servicescorporation", " " },
            { "servicesincorp", " " },
            { "servicesincorporated", " " },
            { "sgps", " " },
            { "sgps.", " " },
            { "sic", " " },
            { "sicc", " " },
            { "sicovam", " " },
            { "sicovam.", " " },
            { "simple", " " },
            { "sirketi", " " },
            { "sirketi.", " " },
            { "sk", " " },
            { "sk.", " " },
            { "sl", " " },
            { "sl.", " " },
            { "sll", " " },
            { "sll.", " " },
            { "slne", " " },
            { "slne.", " " },
            { "slu", " " },
            { "slu.", " " },
            { "snc", " " },
            { "snc.", " " },
            { "sociedad", " " },
            { "sociedadanónima", " " },
            { "sociedadanónimaeuropea", " " },
            { "sociedadcolectiva", " " },
            { "sociedadcomanditariaporacciones", " " },
            { "sociedadcomanditariasimple", " " },
            { "sociedadderesponsabilidadlimitada", " " },
            { "sociedadlimitadalaboral", " " },
            { "sociedadlimitadanuevaempresa", " " },
            { "society", " " },
            { "sofom", " " },
            { "sole", " " },
            { "soleproprietor", " " },
            { "soparfi", " " },
            { "soparfi.", " " },
            { "sp", " " },
            { "sp.", " " },
            { "sp. z.o.o.", " " },
            { "sp;a", " " },
            { "spa", " " },
            { "spa.", " " },
            { "spol", " " },
            { "spol s.r.o.", " " },
            { "spol.", " " },
            { "spolssro", " " },
            { "sprl", " " },
            { "sprl.", " " },
            { "spzoo", " " },
            { "spzoo.", " " },
            { "srk", " " },
            { "srl", " " },
            { "srl.", " " },
            { "sro", " " },
            { "sro.", " " },
            { "st", " " },
            { "st.", " " },
            { "state", " " },
            { "stock", " " },
            { "sucursal", " " },
            { "sucursal.", " " },
            { "svm", " " },
            { "svm.", " " },
            { "system", " " },
            { "t", " " },
            { "t.d.", " " },
            { "t.l.s.", " " },
            { "t.v.", " " },
            { "td", " " },
            { "td.", " " },
            { "techno", " " },
            { "textile", " " },
            { "the", " " },
            { "tls", " " },
            { "tls.", " " },
            { "tovarischestvo", " " },
            { "tovarischestvonavere", " " },
            { "trust", " " },
            { "tv", " " },
            { "tv.", " " },
            { "u", " " },
            { "u.a", " " },
            { "u.a.", " " },
            { "u.n.d.", " " },
            { "u/a", " " },
            { "ua", " " },
            { "ua.", " " },
            { "und", " " },
            { "und.", " " },
            { "union", " " },
            { "v", " " },
            { "v.e.b.", " " },
            { "v.o.f.", " " },
            { "v.o.s.", " " },
            { "valoren", " " },
            { "variable", " " },
            { "veb", " " },
            { "veb.", " " },
            { "venture", " " },
            { "vere", " " },
            { "vergleichbar", " " },
            { "verwaltungs", " " },
            { "verwaltungsges", " " },
            { "verwaltungsges.", " " },
            { "verwaltungsgesellschaft", " " },
            { "verwaltungsgesellschaftmitbeschränkterhaftung", " " },
            { "verwaltungs-gmbh", " " },
            { "vof", " " },
            { "vof.", " " },
            { "vos", " " },
            { "vos.", " " },
            { "w", " " },
            { "w.k.n.", " " },
            { "w.p.k.", " " },
            { "wertpapier", " " },
            { "wertpapierkenn-nummer", " " },
            { "with", " " },
            { "wkn", " " },
            { "wkn.", " " },
            { "wpk", " " },
            { "wpk.", " " },
            { "x", " " },
            { "y", " " },
            { "y.k.", " " },
            { "y/k", " " },
            { "yk", " " },
            { "yk.", " " },
            { "z", " " },
            { "z.a.o", " " },
            { "z.a.o.", " " },
            { "z.o.o.", " " },
            { "z.r.t", " " },
            { "z.r.t.", " " },
            { "zakrytoe", " " },
            { "zakrytoeaktsionernoeobschestvo", " " },
            { "zao", " " },
            { "zao.", " " },
            { "zoo", " " },
            { "zoo.", " " },
            { "zrt", " " },
            { "zrt.", " " }
        };


    protected static Dictionary<string, string> DefaultValueSpecialCharacterReplacements =>
        new()
        {
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
            { ".", "" },
            { "'", "" },
            { "`", "" }
        };


    protected static Dictionary<string, string> BracketReplacements =>
        new()
        {
            { "(", " " },
            { ")", " " },
            { "[", " " },
            { "]", " " },
            { "{", " " },
            { "}", " " }
        };


    protected static List<string> DefaultValueForPrefixesToBeRemoved =>
        [
            "do not use",
            "inactive",
            "invalid",
            "liquidated",
            "llc",
            "merged",
            "p.t",
            "p.t.",
            "private",
            "pt",
            "pt.",
            "the"
        ];


    protected static List<string> DefaultValueForGeoLocationNames =>
        [
            "afghanistan",
            "albania",
            "algeria",
            "andorra",
            "angola",
            "anguilla",
            "antigua & barbuda",
            "antigua",
            "barbuda",
            "argentina",
            "armenia",
            "australia",
            "austria",
            "azerbaijan",
            "bahamas",
            "bahrain",
            "bangladesh",
            "barbados",
            "belarus",
            "belgium",
            "belize",
            "benin",
            "bermuda",
            "bhutan",
            "bolivia",
            "bosnia & herzegovina",
            "bosnia",
            "herzegovina",
            "botswana",
            "brazil",
            "brunei darussalam",
            "bulgaria",
            "burkina faso",
            "myanmar/burma",
            "burundi",
            "cambodia",
            "cameroon",
            "canada",
            "cape verde",
            "cayman islands",
            "central african republic",
            "chad",
            "chile",
            "china",
            "colombia",
            "comoros",
            "congo",
            "costa rica",
            "croatia",
            "cuba",
            "cyprus",
            "czech republic",
            "democratic republic of the congo",
            "denmark",
            "djibouti",
            "dominican republic",
            "dominica",
            "ecuador",
            "egypt",
            "el salvador",
            "equatorial guinea",
            "eritrea",
            "estonia",
            "ethiopia",
            "fiji",
            "finland",
            "france",
            "french guiana",
            "gabon",
            "gambia",
            // georgia is a country, but there is also a us state with the same inputList.
            "georgia",
            "germany",
            "ghana",
            "great britain",
            "greece",
            "grenada",
            "guadeloupe",
            "guatemala",
            "guinea",
            "guinea-bissau",
            "guyana",
            "haiti",
            "honduras",
            "hungary",
            "iceland",
            "india",
            "indonesia",
            "iran",
            "iraq",
            "israel",
            "italy",
            "ivory coast (cote d'ivoire)",
            "ivory coast",
            "cote d'ivoire",
            "jamaica",
            "japan",
            "jordan",
            "kazakhstan",
            "kenya",
            "kosovo",
            "kuwait",
            "kyrgyz republic (kyrgyzstan)",
            "kyrgyzstan",
            "laos",
            "latvia",
            "lebanon",
            "lesotho",
            "liberia",
            "libya",
            "liechtenstein",
            "lithuania",
            "luxembourg",
            "republic of macedonia",
            "macedonia",
            "madagascar",
            "malawi",
            "malaysia",
            "maldives",
            "mali",
            "malta",
            "martinique",
            "mauritania",
            "mauritius",
            "mayotte",
            "mexico",
            "moldova, republic of",
            "republic of moldova",
            "moldova",
            "monaco",
            "mongolia",
            "montenegro",
            "montserrat",
            "morocco",
            "mozambique",
            "namibia",
            "nepal",
            "netherlands",
            "new zealand",
            "nicaragua",
            "niger",
            "nigeria",
            "korea, democratic republic of (north korea)",
            "north korea",
            "korea",
            "norway",
            "oman",
            "pacific islands",
            "pakistan",
            "panama",
            "papua new guinea",
            "paraguay",
            "peru",
            "philippines",
            "poland",
            "portugal",
            "puerto rico",
            "qatar",
            "reunion",
            "romania",
            "russian federation",
            "rwanda",
            "saint kitts and nevis",
            "saint kitts",
            "st kitts",
            "nevis",
            "saint kitts & nevis",
            "saint lucia",
            "saint vincent's & grenadines",
            "saint vincent",
            "st vincent",
            "grenadines",
            "samoa",
            "sao tome and principe",
            "sao tome & principe",
            "principe",
            "sao tome",
            "saudi arabia",
            "senegal",
            "serbia",
            "seychelles",
            "sierra leone",
            "singapore",
            "slovak republic (slovakia)",
            "slovak republic",
            "slovakia",
            "slovenia",
            "solomon islands",
            "somalia",
            "south africa",
            "korea, republic of (south korea)",
            "south korea",
            "south sudan",
            "spain",
            "sri lanka",
            "sudan",
            "suriname",
            "swaziland",
            "sweden",
            "switzerland",
            "syria",
            "tajikistan",
            "tanzania",
            "thailand",
            "timor leste",
            "togo",
            "trinidad & tobago",
            "trinidad",
            "tobago",
            "tunisia",
            "turkey",
            "turkmenistan",
            "turks & caicos islands",
            "turks",
            "caicos",
            "uganda",
            "ukraine",
            "united arab emirates",
            "united states of america (usa)",
            "usa",
            "u.s.a.",
            "united states",
            "uruguay",
            "uzbekistan",
            "venezuela",
            "vietnam",
            "virgin islands",
            "virgin islands (uk)",
            "virgin islands (us)",
            "yemen",
            "zambia",
            "zimbabwe",
            "united kingdom",
            "ireland",
            "alabama",
            "alaska",
            "arizona",
            "arkansas",
            "california",
            "colorado",
            "connecticut",
            "delaware",
            "florida",
            "georgia",
            "hawaii",
            "idaho",
            "illinois",
            "indiana",
            "iowa",
            "kansas",
            "kentucky",
            "louisiana",
            "maine",
            "maryland",
            "massachusetts",
            "michigan",
            "minnesota",
            "mississippi",
            "missouri",
            "montana",
            "nebraska",
            "nevada",
            "new hampshire",
            "new jersey",
            "new mexico",
            "new york",
            "newyork",
            "north carolina",
            "north dakota",
            "ohio",
            "oklahoma",
            "oregon",
            "pennsylvania",
            "rhode island",
            "south carolina",
            "south dakota",
            "tennessee",
            "texas",
            "utah",
            "vermont",
            "virginia",
            "washington",
            "west virginia",
            "wisconsin",
            "wyoming",
            "alberta",
            "british columbia",
            "manitoba",
            "new brunswick",
            "newfoundland and labrador",
            "newfoundland & labrador",
            "newfoundland",
            "labrador",
            "northwest territories",
            "nova scotia",
            "nunavut",
            "ontario",
            "prince edward island",
            "quebec",
            "saskatchewan",
            "yukon territory",
            "yukon",
            "bihar",
            "hyderabad",
            "maharastra",
            "delhi",
            "ahmedabad",
            "pune",
            "bangalore"
        ];


    // This method uses a RegEx to identify: 
    //   ^         start of string
    //   [(-]?     followed by zero or one opening parentheses or dash
    //   [ ]*      followed by zero or more space(s)
    //   ?         immediately followed by (i.e. performs a "lazy search" for the first occurrence of)
    //   {prefix}  the prefix itself
    //   [ ]+      followed by one or more space(s)
    //   [)-]?     followed by zero or one closing parentheses or dash 
    //   [ ]*      followed by zero or more space(s) 
    // 
    // Once located, the prefix is removed from the string. 
    // The process is repeated for each prefix in the list.
    protected string RemoveCustomPrefixes(string text)
    {
        if (PrefixesToBeRemoved.IsNullOrEmpty())
        {
            return text;
        }

        string result = text;
        foreach (string prefix in PrefixesToBeRemoved)
        {
            string regex = $@"^[(-]?[ ]*?{prefix}[ ]+[)-]?[ ]*";
            result = Regex.Replace(result, regex, string.Empty, RegexOptions.IgnoreCase);
        }
        return result.FullTrim();
    }


    protected List<string> RemoveCustomPrefixes(List<string> inputList)
    {
        if (PrefixesToBeRemoved.IsNullOrEmpty())
        {
            return inputList;
        }

        List<string> result = inputList
                                .Select(keyword => RemoveCustomPrefixes(keyword))
                                .Distinct().ToList();

        return result;
    }


    private List<string> ReplaceSpecialCharactersAllCombinations(string prefix, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [prefix];
        }

        if (SpecialCharacterReplacements.IsNullOrEmpty())
        {
            return [$"{prefix}{text}"];
        }

        string firstChar = $"{text[0]}";
        List<string> firstCharOptions = [firstChar];
        if (SpecialCharacterReplacements!.TryGetValue(firstChar, out string alternateFirstChar))
        {
            firstCharOptions.Add(alternateFirstChar);
        }

        string remainingText = (text.Length > 1) ? text[1..] : string.Empty;
        List<string> result = [];
        foreach (string charOption in firstCharOptions)
        {
            result.AddRange(ReplaceSpecialCharactersAllCombinations($"{prefix}{charOption}", remainingText));
        }

        result = result.Distinct().ToList();
        return result; ;
    }


    private List<string> ReplaceSpecialCharactersAllCombinations(List<string> inputList)
    {
        if (inputList.IsNullOrEmpty() || SpecialCharacterReplacements.IsNullOrEmpty())
        {
            return inputList;
        }

        List<string> result = [];
        foreach (string text in inputList)
        {
            result.AddRange(ReplaceSpecialCharactersAllCombinations(string.Empty, text));
        }

        result = result.Distinct().ToList();
        return result; ;
    }



    private List<string> ShortenKeywordsNotStartingWithGeoLocations(List<string> inputList)
    {
        List<string> result = [];
        foreach (string keyword in inputList)
        {
            if (StartsWithGeoLocationName(keyword))
            {
                result.Add(keyword);
            }
            else
            {
                result.Add(ShortenName(keyword));
            }
        }

        result = result.Distinct().ToList();
        return result;
    }


    protected bool StartsWithGeoLocationName(string result)
    {
        if (GeoLocationNames.IsNullOrEmpty())
        {
            return false;
        }

        foreach (string countryOrState in GeoLocationNames)
        {
            if (result.StartsWith($"{countryOrState} ", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }


    private static List<string> HandleSpecialCases(List<string> inputList)
    {
        List<string> result = [];

        foreach (string keyword in inputList)
        {
            result.Add(keyword.FullTrim());
            if (keyword.Contains(STD_COMMA))
            {
                result.Add(keyword.Replace(STD_COMMA, Char.ToString(STD_SPACE)).FullTrim());
            }
        }

        List<string> tempResult = [];
        if (!result.IsNullOrEmpty())
        {
            tempResult.AddRange(result);
        }
        foreach (string keyword in tempResult)
        {
            if (keyword.Contains(AMPERSAND))  // When there is AMPERSAND
            {
                // Also add with regular AND, instead of AMPERSAND
                result.Add(keyword.Replace(AMPERSAND, $" {REGULAR_AND} ").FullTrim());
            }
        }

        result = RemoveForbiddenSufixes(result);
        return result.Distinct().ToList();
    }


    private static List<string> RemoveForbiddenSufixes(List<string> inputList) =>
        inputList.Select(keyword => keyword.FullTrim().EndsWith(STD_COMMA.FullTrim()) ?
                                        keyword.FullTrim().StrLeftBack(STD_COMMA.FullTrim()).FullTrim() :
                                    keyword.FullTrim().EndsWith($" {AMPERSAND}") ? 
                                        keyword.FullTrim().StrLeftBack($" {AMPERSAND}".FullTrim()).FullTrim() :
                                    keyword.FullTrim().EndsWith($" {REGULAR_AND}") ?
                                        keyword.FullTrim().StrLeftBack($" {REGULAR_AND}".FullTrim()).FullTrim() :
                                        keyword).Distinct().ToList();
    

    private static List<string> ProvideDashVariations(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }        
        
        List<string> result = [text];  // Add the keyword in its original form
        if (text.Contains(HYPHEN))  // When there is HYPHEN(= dash or minus sign)
        {
            result.Add(text.Replace(HYPHEN.ToString(), $" {HYPHEN} ").FullTrim());  // Also add with HYPHEN surrounded by spaces
            result.Add(text.Replace(HYPHEN.ToString(), STD_SPACE.ToString()).FullTrim());  // Also add with regular SPACE instead of HYPHEN
        }

        result = result.Distinct().ToList();
        return result;
    }


    private static List<string> ProvideDashVariations(List<string> inputList) =>
        inputList.SelectMany(keyword => ProvideDashVariations(keyword)).Distinct().ToList();


    private static List<string> ProvideBracketVariations(List<string> inputList)
    {
        if (BracketReplacements.IsNullOrEmpty())
        {
            return inputList;
        }

        List<string> result = [];
#pragma warning disable IDE0305 // Simplify collection initialization
        List<string> brackets = BracketReplacements.Keys.ToList();
#pragma warning restore IDE0305 // Simplify collection initialization
        foreach (string keyword in inputList)
        {
            result.Add(keyword);            
            if (keyword.Contains(brackets))  // When there is some opening or closing bracket
            {
                result.Add(keyword.ReplaceAll(BracketReplacements).FullTrim());  // Also add with regular SPACE instead of bracket
            }
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

        string origText = SurroundEachSubstringWithWhiteSpaces(text, 
                            ["(", ")", "{", "}", "[", "]", "<", ">", ",", ";", ":"]);
        origText = " " + origText.FullTrim() + " ";

        string result = origText;
        var effectiveReplacements = SubstringReplacements!.Where(r => r.Key.NotEquals(r.Value)).ToList();
        foreach (var replacement in effectiveReplacements)
        {
            if (IsPrefix(replacement.Key, origText) && IsPrefix(replacement.Key, result))
            {
                result = result.ReplaceFirstOccurrence(replacement.Key, replacement.Value);
                result = " " + result.FullTrim() + " ";
            }
            if (IsSuffix(replacement.Key, origText) && IsSuffix(replacement.Key, result))
            {                
                result = result.ReplaceLastOccurrence(replacement.Key, replacement.Value);
                result = " " + result.FullTrim() + " ";
            }
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


    private static string SurroundEachSubstringWithWhiteSpaces(string text, List<string> subStrings)
    {
        string result = text;

        foreach (string subString in subStrings)
        {
            result = result.Replace(subString, $" {subString} ");
        }

        return result;
    }


    private static bool IsPrefix(string possiblePrefix, string text)
    {
        string regex = @"^(\s[(\[{<])?\s" + Regex.Escape(possiblePrefix) + @"\s";
        return Regex.IsMatch(text, regex, RegexOptions.IgnoreCase);
    }


    private static bool IsSuffix(string possibleSufix, string text)
    {
        string regex = @"\s" + Regex.Escape(possibleSufix) + @"\s([)\]}>,;:]\s)?$";
        return Regex.IsMatch(text, regex, RegexOptions.IgnoreCase);
    }
}
