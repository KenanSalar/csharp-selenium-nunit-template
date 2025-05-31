using System.Runtime.InteropServices;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SeleniumTraining.Core.Services.Drivers;

public partial class BraveDriverFactoryService : ChromiumDriverFactoryServiceBase, IBrowserDriverFactoryService
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
        if (settingsBase is not BraveSettings settings)
        {
            var ex = new ArgumentException($"Invalid settings type provided. Expected {nameof(BraveSettings)}, got {settingsBase.GetType().Name}.", nameof(settingsBase));
            Logger.LogError(ex, "Settings type mismatch in {FactoryName}.", nameof(BraveDriverFactoryService));
            throw ex;
        }

        Logger.LogInformation(
            "Creating {BrowserType} WebDriver (uses ChromeDriver). Requested settings - Headless: {IsHeadless}, WindowSize: {WindowWidth}x{WindowHeight} (if specified).",
            Type, settings.Headless, settings.WindowWidth ?? -1, settings.WindowHeight ?? -1);

        Logger.LogInformation("Skipping explicit WebDriverManager setup for {BrowserType}. ChromeDriver is expected to be found via NuGet package or system PATH.", Type);

        Logger.LogDebug("Attempting to locate Brave browser executable for {BrowserType}.", Type);
        string? braveExecutablePath = GetBraveExecutablePathInternal();
        if (string.IsNullOrEmpty(braveExecutablePath))
        {
            var ex = new FileNotFoundException(
                "Brave browser executable was not found. Ensure BRAVE_EXECUTABLE_PATH environment variable is set or Brave is installed in a standard location.");

            Logger.LogError(ex, "Critical error: Brave browser executable could not be located for {BrowserType}.", Type);
            throw ex;
        }

        Logger.LogInformation("Brave browser binary location found for {BrowserType}: {BraveBinaryPath}", Type, braveExecutablePath);

        try
        {
            string? chromiumVersionForDriver = GetChromiumVersionFromBraveExecutable(braveExecutablePath);

            if (!string.IsNullOrEmpty(chromiumVersionForDriver))
            {
                Logger.LogInformation("Attempting to set up ChromeDriver matching detected Chromium version {ChromiumVersion} using WebDriverManager for Brave.", chromiumVersionForDriver);
                _ = new DriverManager().SetUpDriver(new ChromeConfig(), chromiumVersionForDriver);
                Logger.LogInformation("WebDriverManager successfully completed ChromeDriver setup for Brave using specific version: {ChromiumVersion}.", chromiumVersionForDriver);
            }
            else
            {
                Logger.LogWarning("Could not determine specific Chromium version for Brave from path {BravePath}. Falling back to WebDriverManager's default behavior for ChromeConfig (likely latest ChromeDriver). This might lead to version mismatches.", braveExecutablePath);
                _ = new DriverManager().SetUpDriver(new ChromeConfig());
                Logger.LogInformation("WebDriverManager completed ChromeDriver setup for Brave using default ChromeConfig (latest driver).");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "WebDriverManager failed to set up ChromeDriver for Brave (path: {BravePath}).", braveExecutablePath);
            throw;
        }

        ChromeOptions braveOptions = ConfigureCommonChromeOptions(
            settings,
            options,
            out List<string> appliedOptionsForLog
        );

        braveOptions.BinaryLocation = braveExecutablePath;
        Logger.LogInformation("Brave browser binary location set for {BrowserType}: {BraveBinaryPath}", Type, braveExecutablePath);
        
        string binaryLog = $"--binary={braveExecutablePath}";
        // if (!appliedOptionsForLog.Contains(binaryLog))
        // {
        //     appliedOptionsForLog.Add(binaryLog);
        // }

        Logger.LogInformation(
            "ChromeOptions (for {BrowserType}) configured. BinaryLocation: {BinaryLocation}. Effective arguments: [{EffectiveArgs}]",
            Type,
            braveOptions.BinaryLocation,
            string.Join(", ", appliedOptionsForLog.Distinct())
        );

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
            string[] linuxPaths = ["/usr/bin/brave-browser", "/opt/brave.com/brave/brave-browser", "/snap/bin/brave", "/opt/brave.com"];
            return linuxPaths.FirstOrDefault(p => !string.IsNullOrEmpty(p) && File.Exists(p));
        }
        return null;
    }

    [GeneratedRegex(@"Chromium:\s*([\d\.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ChromiumVersionRegex();

    [GeneratedRegex(@"Brave Browser\s*([\d\.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex BraveBrowserVersionRegex();


    private string? GetChromiumVersionFromBraveExecutable(string braveExecutablePath)
    {
        if (string.IsNullOrEmpty(braveExecutablePath) || !File.Exists(braveExecutablePath))
        {
            Logger.LogWarning("Cannot get Brave version; executable path is invalid or file does not exist: {BravePath}", braveExecutablePath);
            return null;
        }

        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = braveExecutablePath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                Logger.LogWarning("Failed to start process to get Brave version for path: {BravePath}", braveExecutablePath);
                return null;
            }

            string output = string.Empty;
            if (process.WaitForExit(5000))
            {
                output = process.StandardOutput.ReadToEnd();
            }
            else
            {
                Logger.LogWarning("Timeout waiting for Brave --version command to exit for path: {BravePath}. Process might be hanging.", braveExecutablePath);
                try { process.Kill(true); } catch { /* ignored */ }
                return null;
            }

            if (process.ExitCode != 0)
            {
                Logger.LogWarning("Brave --version command exited with code {ExitCode} for path: {BravePath}. Output: {StdOutput}", process.ExitCode, braveExecutablePath, output);
                return null;
            }

            Logger.LogDebug("Brave --version output for path {BravePath}: {Output}", braveExecutablePath, output);

            Match chromiumMatch = ChromiumVersionRegex().Match(output);
            if (chromiumMatch.Success && chromiumMatch.Groups.Count > 1)
            {
                string chromiumVersion = chromiumMatch.Groups[1].Value;
                if (chromiumVersion.Split('.').Length >= 3)
                {
                    Logger.LogInformation("Extracted Chromium version {ChromiumVersion} from Brave version output.", chromiumVersion);
                    return chromiumVersion;
                }
                Logger.LogWarning("Parsed Chromium version '{ChromiumVersion}' from Brave output appears incomplete. Output: {Output}", chromiumVersion, output);
            }

            Match braveVersionMatch = BraveBrowserVersionRegex().Match(output);
            if (braveVersionMatch.Success && braveVersionMatch.Groups.Count > 1)
            {
                string braveVersion = braveVersionMatch.Groups[1].Value;
                if (braveVersion.Split('.').Length >= 3)
                {
                    Logger.LogInformation(
                        "Using Brave Browser version {BraveVersion} as a proxy for Chromium version (Chromium prefix not found in output: {Output}).",
                        braveVersion, output);
                    return braveVersion;
                }
                Logger.LogWarning("Parsed Brave Browser version '{BraveVersion}' from output appears incomplete. Output: {Output}", braveVersion, output);
            }

            Logger.LogWarning("Could not parse a suitable Chromium version from Brave output: {Output}", output);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting Brave version from executable: {BravePath}", braveExecutablePath);
            return null;
        }
    }
}
