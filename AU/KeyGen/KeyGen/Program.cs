using KeyGen.lib;
using KeyGen.lib.Enums;
using System.Reflection;

namespace KeyGen;

internal class Program
{
    private static AppSettings? _appSettings;
    private static KeyGenFactory? _keyGenFactory;
    private static KeyGen.lib.KeyGen? _keyGen;
    private static string? _name;
    private static string? _command;
    private static string? _duns;
    private static string? _gisid;
    private static string? _paceapgloc;


    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (!ValidateParameters(args))
        {
            return;
        }        
        if(!GetParameters(args))
        {
            return;
        }
        if (!LoadKeyGenObjects())
        {
            return;
        }

        ExecuteCommand();
    }


    private static bool ValidateParameters(string[] args)
    {
        if ( (args.Length == 0) || string.IsNullOrEmpty(args[0]) )
        {
            DisplaySyntax("Too few arguments.");
            return false;
        }
        else if (args.Length > 5)
        {
            DisplaySyntax("Too many arguments.");
            return false;
        }
        else if (args[0].Equals("/?"))
        {
            DisplaySyntax();
            return false;
        }

        return true;
    }


    private static bool GetParameters(string[] args)
    {
        _command = args[0].ToLower();
        _name = args[1];
        if (args.Length < 3)
        {
            return true;
        }

        Dictionary<string, string> parameters = [];
        for (int i = 2; i < args.Length; i++)
        {
            string[] parameter = args[i].Split(':');
            if (parameter.Length != 2)
            {
                DisplaySyntax("Incorrect syntax.");
                return false;
            }
            parameters.Add(parameter[0], parameter[1]);
        }

        _ = parameters.TryGetValue("duns", out _duns!);
        _duns ??= string.Empty;

        _ = parameters.TryGetValue("gisid", out _gisid!);
        _gisid ??= string.Empty;
        
        _ = parameters.TryGetValue("paceapgloc", out _paceapgloc!);
        _paceapgloc ??= string.Empty;

        return true;
    }


    private static bool LoadKeyGenObjects()
    {
        switch (_command!.ToLower())
        {
            case "entity":
                Display($"Loading settings from SQL database. Please wait some seconds...");
                _appSettings = new();
                _keyGenFactory = new(_appSettings.ConnectionString);
                _keyGen = _keyGenFactory.KeyGenForEntities;
                break;

            case "individual":
                Display($"Loading settings from SQL database. Please wait some seconds...");
                _appSettings = new();
                _keyGenFactory = new(_appSettings.ConnectionString);
                _keyGen = _keyGenFactory!.KeyGenForIndividuals;
                break;

            case "entfinscan":
                Display($"Loading settings from SQL database. Please wait some seconds...");
                _appSettings = new();
                _keyGenFactory = new(_appSettings.ConnectionString);
                _keyGen = _keyGenFactory!.KeyGenForEntities;
                break;

            case "indivfinscan":
                Display($"Loading settings from SQL database. Please wait some seconds...");
                _appSettings = new();
                _keyGenFactory = new(_appSettings.ConnectionString);
                _keyGen = _keyGenFactory!.KeyGenForIndividuals;
                break;

            case "identify":
                Display($"Loading settings from SQL database. Please wait some seconds...");
                _appSettings = new();
                _keyGenFactory = new(_appSettings.ConnectionString);
                break;

            default:
                DisplaySyntax("Incorrect syntax.");
                return false;
        }

        return true;
    }


    private static void ExecuteCommand()
    {
        switch (_command!.ToLower())
        {
            case "entity":
            case "individual":
                ExecuteKeyGenCommand();
                break;

            case "entfinscan": 
                ExecuteKeyGenEntFinScanCommand();
                break;

            case "indivfinscan":
                ExecuteKeyGenIndivFinScanCommand();
                break;

            case "identify":
                ExecuteIdentifyCommand();
                break;

            default:
                break;
        }
    }


    private static void ExecuteKeyGenCommand()
    {
        Display($"\nComputing keywords for {_command!.ToLower()} \"{_name}\"...");
        List<string> keywords = _keyGen!.GenerateKey(_name);

        if (keywords is null)
        {
            Display($"No keywords corresponding to {_command.ToLower()} \"{_name}\"");
            return;
        }
        foreach (string keyword in keywords)
        {
            Display($"\t{keyword}");
        }
    }


    private static void ExecuteKeyGenEntFinScanCommand()
    {
        Display($"\nComputing keywords for FinScan Search of Entity {_command!.ToLower()} \"{_name}\"...");
        List<string> keywords = ((KeyGenForEntities)_keyGen!).GenerateKeyForFinScanSearch(_name);

        if (keywords is null)
        {
            Display($"No keywords corresponding to {_command.ToLower()} \"{_name}\"");
            return;
        }
        foreach (string keyword in keywords)
        {
            Display($"\t{keyword}");
        }
    }


    private static void ExecuteKeyGenIndivFinScanCommand()
    {
        Display($"\nComputing keywords for FinScan Search of Individual {_command!.ToLower()} \"{_name}\"...");
        List<string> keywords = ((KeyGenForIndividuals)_keyGen!).GenerateKeyForFinScanSearch(_name);

        if (keywords is null)
        {
            Display($"No keywords corresponding to {_command.ToLower()} \"{_name}\"");
            return;
        }
        foreach (string keyword in keywords)
        {
            Display($"\t{keyword}");
        }
    }


    private static void ExecuteIdentifyCommand()
    {
        Display($"\nTrying to identify type of \"{_name}\"" +
                $" duns:\"{_duns}\"" +
                $" gisid:\"{_gisid}\"" +
                $" paceapgloc:\"{_paceapgloc}\"" +
                "...");
    
        SubjectTypeEnum subjectType = _keyGenFactory!.GetSubjectType(_name!, _duns!, _gisid!, _paceapgloc!);

        Display($"\t{subjectType}");
    }


    private static void Display(string message) => Console.WriteLine(message);


    private static void DisplaySyntax(string message = "")
    {
        if (!string.IsNullOrEmpty(message))
        {
            Display(message);
        }
        Display(CommandSyntax());
    }


    private static string CommandSyntax() =>
        $"Command syntax: " + 
        $"\n\t{CurrentProgramName()} entity|individual|entfinscan|indivfinscan \"Name to search for\"" +
        $"\n\t{CurrentProgramName()} identify \"Name to identify\" duns:\"999999999\" gisid:\"9999999\" paceapgloc:\"xxxxx\"";


    private static string CurrentProgramName() =>
        Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
}