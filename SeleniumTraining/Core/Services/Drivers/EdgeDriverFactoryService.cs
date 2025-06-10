using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using OpenQA.Selenium.Edge;
using System.Runtime.InteropServices;

namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Factory service specifically for creating and configuring <see cref="EdgeDriver"/> instances.
/// </summary>
/// <remarks>
/// This service handles the Edge-specific setup, including applying user preferences
/// and locating the Edge executable before creating the driver instance.
/// </remarks>
public class EdgeDriverFactoryService : ChromiumDriverFactoryServiceBase
{
    /// <inheritdoc/>
    public override BrowserType Type => BrowserType.Edge;

    /// <inheritdoc/>
    protected override Version MinimumSupportedVersion { get; } = new("110.0");

    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeDriverFactoryService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create loggers.</param>
    public EdgeDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(EdgeDriverFactoryService), Type);
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Attempts to locate the Microsoft Edge executable on the current system.
    /// </summary>
    /// <returns>The full path to the Edge executable if found; otherwise, an empty string.</returns>
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
