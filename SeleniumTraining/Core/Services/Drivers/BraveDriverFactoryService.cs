using System.Runtime.InteropServices;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumTraining.Core.Services.Drivers;

public class BraveDriverFactoryService : ChromiumDriverFactoryServiceBase, IBrowserDriverFactoryService
{
    public BrowserType Type => BrowserType.Brave;
    protected override BrowserType ConcreteBrowserType => BrowserType.Brave;
    protected override Version MinimumSupportedVersion { get; } = new("115.0");

    public BraveDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(BraveDriverFactoryService), Type);
    }

    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not BraveSettings settings) // Cast to specific BraveSettings
        {
            var ex = new ArgumentException($"Invalid settings type provided. Expected {nameof(BraveSettings)}, got {settingsBase.GetType().Name}.", nameof(settingsBase));
            Logger.LogError(ex, "Settings type mismatch in {FactoryName}.", nameof(BraveDriverFactoryService));
            throw ex;
        }

        Logger.LogInformation(
            "Creating {BrowserType} WebDriver (uses ChromeDriver). Requested settings - Headless: {IsHeadless}, WindowSize: {WindowWidth}x{WindowHeight} (if specified).",
            Type, settings.Headless, settings.WindowWidth ?? -1, settings.WindowHeight ?? -1);

        Logger.LogInformation("Skipping explicit WebDriverManager setup for {BrowserType}. ChromeDriver is expected to be found via NuGet package or system PATH.", Type);

        ChromeOptions braveOptions = ConfigureCommonChromeOptions(
            settings,
            options,
            out List<string> appliedOptionsForLog
        );

        Logger.LogDebug("Attempting to locate Brave browser executable for {BrowserType}.", Type);
        string? braveExecutablePath = GetBraveExecutablePathInternal();
        if (string.IsNullOrEmpty(braveExecutablePath))
        {
            var ex = new FileNotFoundException(
                "Brave browser executable was not found. Ensure BRAVE_EXECUTABLE_PATH environment variable is set or Brave is installed in a standard location.");

            Logger.LogError(ex, "Critical error: Brave browser executable could not be located for {BrowserType}.", Type);
            throw ex;
        }
        braveOptions.BinaryLocation = braveExecutablePath;
        Logger.LogInformation("Brave browser binary location set for {BrowserType}: {BraveBinaryPath}", Type, braveExecutablePath);
        appliedOptionsForLog.Add($"--binary={braveExecutablePath}");

        Logger.LogInformation(
            "ChromeOptions (for {BrowserType}) configured. BinaryLocation: {BinaryLocation}. Effective arguments: [{EffectiveArgs}]",
            Type, braveOptions.BinaryLocation, string.Join(", ", appliedOptionsForLog.Distinct()));

        return CreateDriverInstanceWithChecks(braveOptions);
    }

    private string? GetBraveExecutablePathInternal()
    {
        Logger.LogDebug("Searching for Brave executable: Checking BRAVE_EXECUTABLE_PATH environment variable.");
        string? braveExecutablePathEnv = Environment.GetEnvironmentVariable("BRAVE_EXECUTABLE_PATH");

        if (!string.IsNullOrEmpty(braveExecutablePathEnv))
        {
            Logger.LogDebug("BRAVE_EXECUTABLE_PATH environment variable found: '{BraveEnvPath}'. Checking if file exists.", braveExecutablePathEnv);
            if (File.Exists(braveExecutablePathEnv))
            {
                Logger.LogInformation("Brave executable found using BRAVE_EXECUTABLE_PATH environment variable: '{BraveEnvPath}'", braveExecutablePathEnv);
                return braveExecutablePathEnv;
            }
            Logger.LogWarning("BRAVE_EXECUTABLE_PATH environment variable '{BraveEnvPath}' points to a non-existent file.", braveExecutablePathEnv);
        }
        else
        {
            Logger.LogDebug("BRAVE_EXECUTABLE_PATH environment variable not set or empty.");
        }

        Logger.LogDebug("Attempting to detect Brave executable in standard local installation paths for OS {CurrentOS}.", RuntimeInformation.OSDescription);
        string? detectedLocalPath = DetectLocalBravePath();

        if (!string.IsNullOrEmpty(detectedLocalPath))
        {
            Logger.LogInformation("Brave executable detected at local path: '{BraveLocalPath}'", detectedLocalPath);
            return detectedLocalPath;
        }

        Logger.LogWarning("Brave executable not found in standard local installation paths for OS {CurrentOS}.", RuntimeInformation.OSDescription);

        return null;
    }

    private static string? DetectLocalBravePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string[] windowsPaths = [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "BraveSoftware", "Brave-Browser", "Application", "brave.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "BraveSoftware", "Brave-Browser", "Application", "brave.exe")
            ];
            return windowsPaths.FirstOrDefault(p => !string.IsNullOrEmpty(p) && File.Exists(p));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            const string macOSPath = "/Applications/Brave Browser.app/Contents/MacOS/Brave Browser";
            return File.Exists(macOSPath) ? macOSPath : null;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] linuxPaths = ["/usr/bin/brave-browser", "/opt/brave.com/brave/brave-browser", "/snap/bin/brave"];
            return linuxPaths.FirstOrDefault(p => !string.IsNullOrEmpty(p) && File.Exists(p));
        }
        return null;
    }
}
