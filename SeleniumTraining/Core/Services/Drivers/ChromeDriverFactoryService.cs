using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumTraining.Core.Services.Drivers;

public partial class ChromeDriverFactoryService : ChromiumDriverFactoryServiceBase, IBrowserDriverFactoryService
{
    public BrowserType Type => BrowserType.Chrome;
    protected override BrowserType ConcreteBrowserType => BrowserType.Chrome;
    protected override Version MinimumSupportedVersion { get; } = new("110.0");

    public ChromeDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(ChromeDriverFactoryService), Type);
    }

    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not ChromeSettings settings)
        {
            var ex = new ArgumentException($"Invalid settings type provided. Expected {nameof(ChromeSettings)}, got {settingsBase.GetType().Name}.", nameof(settingsBase));
            Logger.LogError(ex, "Settings type mismatch in {FactoryName}.", nameof(ChromeDriverFactoryService));
            throw ex;
        }

        Logger.LogInformation(
            "Creating {BrowserType} WebDriver. Requested settings - Headless: {IsHeadless}, WindowSize: {WindowWidth}x{WindowHeight} (if specified).",
            Type,
            settings.Headless,
            settings.WindowWidth ?? -1,
            settings.WindowHeight ?? -1
        );

        string chromeExecutablePath = GetChromeExecutablePathInternal();
        if (string.IsNullOrEmpty(chromeExecutablePath))
        {
            Logger.LogWarning("{BrowserType} executable path could not be determined. WebDriverManager will attempt to find Chrome or use its default.", Type);
        }
        else
        {
            Logger.LogInformation("Using {BrowserType} executable found at: {ChromePath}", Type, chromeExecutablePath);
        }

        try
        {
            string? actualChromeVersion = null;
            if (!string.IsNullOrEmpty(chromeExecutablePath))
            {
                actualChromeVersion = GetActualChromeVersion(chromeExecutablePath);
            }

            if (!string.IsNullOrEmpty(actualChromeVersion))
            {
                Logger.LogInformation("Attempting to set up ChromeDriver for detected Chrome version {ActualChromeVersion} using WebDriverManager.", actualChromeVersion);
                _ = new DriverManager().SetUpDriver(new ChromeConfig(), actualChromeVersion);
                Logger.LogInformation("WebDriverManager successfully completed ChromeDriver setup for Chrome using specific version: {ActualChromeVersion}.", actualChromeVersion);
            }
            else
            {
                Logger.LogWarning(
                    "Could not determine specific Chrome version (executable path: '{ChromeExePath}'). Falling back to WebDriverManager's default ChromeConfig (likely latest ChromeDriver).",
                    chromeExecutablePath ?? "Not Found"
                );
                _ = new DriverManager().SetUpDriver(new ChromeConfig());
                Logger.LogInformation("WebDriverManager completed ChromeDriver setup for Chrome using default ChromeConfig (latest driver).");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "WebDriverManager failed to set up ChromeDriver (path: {ChromePath}).", chromeExecutablePath ?? "Not determined");
            throw;
        }

        ChromeOptions chromeOptions = ConfigureCommonChromeOptions(settings, options, out List<string> appliedOptionsForLog);

        if (!string.IsNullOrEmpty(chromeExecutablePath))
        {
            chromeOptions.BinaryLocation = chromeExecutablePath;
        }

        Logger.LogInformation("ChromeOptions (for {BrowserType}) configured. BinaryLocation: {BinaryLocation}. Effective arguments: [{EffectiveArgs}]", Type, chromeOptions.BinaryLocation ?? "Default", string.Join(", ", appliedOptionsForLog.Distinct()));
        return CreateDriverInstanceWithChecks(chromeOptions);
    }

    [GeneratedRegex(@"Google Chrome\s*([\d\.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ChromeVersionRegex();

    private string? GetActualChromeVersion(string chromeExecutablePath)
    {
        if (string.IsNullOrEmpty(chromeExecutablePath) || !File.Exists(chromeExecutablePath))
        {
            Logger.LogWarning("Cannot get Chrome version; executable path is invalid or file does not exist: {ChromePath}", chromeExecutablePath);
            return null;
        }
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = chromeExecutablePath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null) return null;

            string output = string.Empty;
            if (process.WaitForExit(5000)) { output = process.StandardOutput.ReadToEnd(); }
            else { Logger.LogWarning("Timeout getting Chrome version from {ChromePath}", chromeExecutablePath); try { process.Kill(true); } catch {/*ignore*/} return null; }

            if (process.ExitCode != 0) { Logger.LogWarning("Chrome --version for {ChromePath} exited with code {ExitCode}. Output: {Output}", chromeExecutablePath, process.ExitCode, output); return null; }

            Logger.LogDebug("Google Chrome --version output for path {ChromePath}: {Output}", chromeExecutablePath, output);
            Match match = ChromeVersionRegex().Match(output); // Using generated regex
            if (match.Success && match.Groups.Count > 1)
            {
                string version = match.Groups[1].Value;
                if (version.Split('.').Length >= 3)
                {
                    Logger.LogInformation("Extracted Google Chrome version {ChromeVersion}.", version);
                    return version;
                }
            }
            Logger.LogWarning("Could not parse Google Chrome version from output: {Output}", output);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting Google Chrome version from {ChromePath}", chromeExecutablePath);
            return null;
        }
    }

    private string GetChromeExecutablePathInternal()
    {
        // Prefer environment variable if set (useful for CI or specific configs)
        string? chromePathEnv = Environment.GetEnvironmentVariable("CHROME_EXECUTABLE_PATH");
        if (!string.IsNullOrEmpty(chromePathEnv) && File.Exists(chromePathEnv))
        {
            Logger.LogInformation("Using Chrome executable from CHROME_EXECUTABLE_PATH: {ChromePathEnv}", chromePathEnv);
            return chromePathEnv;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Add common Windows paths for Chrome
            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string[] paths = [
                Path.Combine(progFiles, "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(progFilesX86, "Google", "Chrome", "Application", "chrome.exe")
            ];
            return paths.FirstOrDefault(File.Exists) ?? string.Empty;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] paths = ["/usr/bin/google-chrome", "/usr/bin/google-chrome-stable", "/opt/google/chrome/chrome"];
            return paths.FirstOrDefault(File.Exists) ?? string.Empty;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
        }
        Logger.LogWarning("Could not determine Chrome executable path for OS {OSPlatform}", RuntimeInformation.OSDescription);
        return string.Empty;
    }
}
