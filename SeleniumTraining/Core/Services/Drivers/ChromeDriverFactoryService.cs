using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumTraining.Core.Services.Drivers;

public partial class ChromeDriverFactoryService : ChromiumDriverFactoryServiceBase, IBrowserDriverFactoryService // Ensure 'partial'
{
    public BrowserType Type => BrowserType.Chrome;
    protected override BrowserType ConcreteBrowserType => BrowserType.Chrome;
    protected override Version MinimumSupportedVersion { get; } = new("110.0"); // Set a reasonable minimum

    public ChromeDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(ChromeDriverFactoryService), Type);
    }

    // Re-add the GeneratedRegex method
    [GeneratedRegex(@"Google Chrome\s*([\d\.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ChromeVersionRegex();

    private string? GetActualChromeVersionForDriver(string chromeExecutablePath)
    {
        if (string.IsNullOrEmpty(chromeExecutablePath) || !File.Exists(chromeExecutablePath))
        {
            Logger.LogWarning("Cannot get Chrome version; executable path is invalid or file does not exist: {ChromePath}", chromeExecutablePath);
            return null;
        }
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = chromeExecutablePath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden // Ensure no window pops up
            };
            using var process = Process.Start(psi);
            if (process == null)
            {
                Logger.LogWarning("Failed to start Chrome process to get version from {ChromePath}.", chromeExecutablePath);
                return null;
            }

            string output = string.Empty;
            // Read output within a timeout to prevent indefinite hanging
            if (process.WaitForExit(5000)) // Wait for 5 seconds
            {
                output = process.StandardOutput.ReadToEnd();
            }
            else
            {
                Logger.LogWarning("Timeout waiting for Chrome --version command to exit for path: {ChromePath}. Process might be hanging.", chromeExecutablePath);
                try { process.Kill(true); } catch { /* ignored */ } // Try to kill hung process
                return null;
            }

            if (process.ExitCode != 0)
            {
                Logger.LogWarning("Chrome --version for {ChromePath} exited with code {ExitCode}. Output: {Output}", chromeExecutablePath, process.ExitCode, output);
                return null;
            }

            Logger.LogDebug("Google Chrome --version output for path {ChromePath}: {Output}", chromeExecutablePath, output.Trim()); // Trim whitespace from output
            Match match = ChromeVersionRegex().Match(output);
            if (match.Success && match.Groups.Count > 1)
            {
                string fullVersion = match.Groups[1].Value.Trim(); // Trim whitespace from parsed version
                if (fullVersion.Split('.').Length >= 3) // Check for at least Major.Minor.Build
                {
                    Logger.LogInformation("Extracted Google Chrome full version {FullChromeVersion}.", fullVersion);
                    // For WebDriverManager.Net (v2.17+) and CfT, providing the full browser version string is usually the best first attempt.
                    Logger.LogInformation("Using full version '{FullVersionString}' for WebDriverManager ChromeDriver lookup.", fullVersion);
                    return fullVersion;
                }
                Logger.LogWarning("Parsed Google Chrome version '{ParsedVersion}' from output appears incomplete (less than 3 parts). Output: {Output}", fullVersion, output.Trim());
            }

            Logger.LogWarning("Could not parse Google Chrome version from output: {Output}", output.Trim());
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
        Logger.LogDebug("Searching for Chrome executable...");
        string? chromePathEnv = Environment.GetEnvironmentVariable("CHROME_EXECUTABLE_PATH");
        if (!string.IsNullOrEmpty(chromePathEnv) && File.Exists(chromePathEnv))
        {
            Logger.LogInformation("Using Chrome executable from CHROME_EXECUTABLE_PATH: {ChromePathEnv}", chromePathEnv);
            return chromePathEnv;
        }

        string[] pathsToTry;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            pathsToTry =
            [
                Path.Combine(progFiles, "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(progFilesX86, "Google", "Chrome", "Application", "chrome.exe")
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pathsToTry = ["/usr/bin/google-chrome-stable", "/usr/bin/google-chrome", "/opt/google/chrome/chrome"];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            pathsToTry = ["/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"];
        }
        else
        {
            Logger.LogWarning("Unsupported OS platform for Chrome path detection: {OSPlatform}", RuntimeInformation.OSDescription);
            return string.Empty;
        }

        foreach (string path in pathsToTry)
        {
            if (File.Exists(path))
            {
                Logger.LogInformation("Found Chrome executable at: {ChromePath}", path);
                return path;
            }
            Logger.LogDebug("Chrome executable not found at: {PathToTry}", path);
        }

        Logger.LogWarning("Chrome executable not found in standard installation paths for OS {OSPlatform}.", RuntimeInformation.OSDescription);
        return string.Empty;
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
            Type, settings.Headless, settings.WindowWidth ?? -1, settings.WindowHeight ?? -1);

        string chromeExecutablePath = GetChromeExecutablePathInternal();

        if (string.IsNullOrEmpty(chromeExecutablePath))
        {
            Logger.LogWarning("Chrome executable path not found. WebDriverManager will use default detection if possible, and Selenium will rely on PATH.");
        }
        else
        {
            Logger.LogInformation("Chrome executable path determined to be: {ChromePath}", chromeExecutablePath);
        }

        try
        {
            string? detectedChromeVersion = null;
            if (!string.IsNullOrEmpty(chromeExecutablePath))
            {
                detectedChromeVersion = GetActualChromeVersionForDriver(chromeExecutablePath);
            }

            if (!string.IsNullOrEmpty(detectedChromeVersion))
            {
                Logger.LogInformation("Attempting to set up ChromeDriver for detected Chrome version '{DetectedChromeVersion}' using WebDriverManager.", detectedChromeVersion);
                _ = new DriverManager().SetUpDriver(new ChromeConfig(), detectedChromeVersion);
                Logger.LogInformation("WebDriverManager successfully completed ChromeDriver setup for Chrome using specific version: '{DetectedChromeVersion}'.", detectedChromeVersion);
            }
            else
            {
                Logger.LogWarning("Could not determine specific Chrome version (executable path for detection was: '{ChromeExePath}'). Falling back to WebDriverManager's default ChromeConfig (likely latest available stable ChromeDriver). This might lead to version mismatches.", chromeExecutablePath ?? "Not Determined");
                _ = new DriverManager().SetUpDriver(new ChromeConfig());
                Logger.LogInformation("WebDriverManager completed ChromeDriver setup for Chrome using default ChromeConfig.");
            }
        }
        catch (Exception ex)
        {
            string versionAttemptedForLog = "Default/Undetected";
            if (!string.IsNullOrEmpty(chromeExecutablePath))
            {
                try
                {
                    versionAttemptedForLog = GetActualChromeVersionForDriver(chromeExecutablePath) ?? "ParsingFailed";
                }
                catch
                {
                    /*ignore*/
                }
            }
            Logger.LogError(ex, "WebDriverManager failed to set up ChromeDriver. Executable path used for version detection: '{ChromePath}'. Version attempted: '{VersionAttempted}'",
                chromeExecutablePath ?? "Not determined",
                versionAttemptedForLog);
            throw;
        }

        ChromeOptions chromeOptions = ConfigureCommonChromeOptions(settings, options, out List<string> appliedOptionsForLog);

        if (!string.IsNullOrEmpty(chromeExecutablePath) && File.Exists(chromeExecutablePath))
        {
            chromeOptions.BinaryLocation = chromeExecutablePath;
        }
        else
        {
            Logger.LogDebug("Chrome binary location not explicitly set in options; Selenium will use default system path or detected driver's expectation.");
        }

        Logger.LogInformation("ChromeOptions configured for {BrowserType}. BinaryLocation: {BinaryLocation}. Effective arguments: [{EffectiveArgs}]", Type, chromeOptions.BinaryLocation ?? "Default/System PATH", string.Join(", ", appliedOptionsForLog.Distinct()));
        return CreateDriverInstanceWithChecks(chromeOptions);
    }
}
