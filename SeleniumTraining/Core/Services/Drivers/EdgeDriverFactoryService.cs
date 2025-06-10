using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using OpenQA.Selenium.Edge;
using System.Runtime.InteropServices;

namespace SeleniumTraining.Core.Services.Drivers;

public class EdgeDriverFactoryService : ChromiumDriverFactoryServiceBase
{
    public override BrowserType Type => BrowserType.Edge;
    protected override Version MinimumSupportedVersion { get; } = new("110.0");

    public EdgeDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(EdgeDriverFactoryService), Type);
    }

    public override IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not EdgeSettings settings)
        {
            throw new ArgumentException($"Invalid settings type. Expected {nameof(EdgeSettings)}, got {settingsBase.GetType().Name}.", nameof(settingsBase));
        }

        EdgeOptions edgeOptions = ConfigureCommonChromiumOptions<EdgeOptions>(settings, options, out _);

        if (settings.UserProfilePreferences != null && settings.UserProfilePreferences.Count != 0)
        {
            ServiceLogger.LogDebug(
                "Applying {PrefCount} user profile preferences via 'prefs' experimental option.",
                settings.UserProfilePreferences.Count
            );

            foreach (KeyValuePair<string, object> pref in settings.UserProfilePreferences)
            {
                try
                {
                    string key = pref.Key;
                    string? stringValue = pref.Value?.ToString();

                    if (stringValue is null)
                    {
                        ServiceLogger.LogWarning("Skipping user profile preference '{PrefKey}' because its value is null.", key);
                        continue;
                    }

                    object finalValue;

                    if (bool.TryParse(stringValue, out bool boolResult))
                    {
                        finalValue = boolResult;
                    }
                    else if (int.TryParse(stringValue, out int intResult))
                    {
                        finalValue = intResult;
                    }
                    else
                    {
                        finalValue = stringValue;
                    }

                    edgeOptions.AddUserProfilePreference(key, finalValue);
                }
                catch (Exception ex)
                {
                    ServiceLogger.LogError(ex, "Failed to apply user profile preference '{PrefKey}' with value '{PrefValue}'.", pref.Key, pref.Value);
                }
            }
        }

        string edgeExecutablePath = GetEdgeExecutablePathInternal();
        if (!string.IsNullOrEmpty(edgeExecutablePath))
        {
            edgeOptions.BinaryLocation = edgeExecutablePath;
        }

        if (string.IsNullOrEmpty(settings.SeleniumGridUrl))
        {
            _ = new DriverManager().SetUpDriver(new EdgeConfig());
            return CreateDriverInstanceWithChecks(edgeOptions, options => new EdgeDriver(options));
        }
        else
        {
            return new RemoteWebDriver(new Uri(settings.SeleniumGridUrl), edgeOptions);
        }
    }

    private string GetEdgeExecutablePathInternal()
    {
        ServiceLogger.LogDebug("Searching for Edge executable...");
        string? edgePathEnv = Environment.GetEnvironmentVariable("EDGE_EXECUTABLE_PATH");
        if (!string.IsNullOrEmpty(edgePathEnv) && File.Exists(edgePathEnv))
        {
            return edgePathEnv;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string[] pathsToTry = [Path.Combine(progFilesX86, "Microsoft", "Edge", "Application", "msedge.exe")];
            return pathsToTry.FirstOrDefault(File.Exists) ?? string.Empty;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] pathsToTry = ["/usr/bin/microsoft-edge-stable", "/usr/bin/microsoft-edge-dev"];
            return pathsToTry.FirstOrDefault(File.Exists) ?? string.Empty;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            const string osxPath = "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge";
            if (File.Exists(osxPath)) return osxPath;
        }

        return string.Empty;
    }
}
