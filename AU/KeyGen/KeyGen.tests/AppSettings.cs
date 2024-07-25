// This class requires Packages:
// - Microsoft.Extensions.Configuration
// - Microsoft.Extensions.Configuration.Json

using Microsoft.Extensions.Configuration;

namespace KeyGen.tests;

public class AppSettings
{
    private readonly IConfigurationRoot _configurationbuilder;

    public AppSettings()
    {
        _configurationbuilder = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                    .Build();
    }

    public string ConnectionString => _configurationbuilder["ConnectionStrings:SQLDatabase"]!;
}
