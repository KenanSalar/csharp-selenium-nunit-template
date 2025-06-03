namespace SeleniumTraining.Tests.SauceDemoTests;

/// <summary>
/// Provides a base class specifically for SauceDemo application tests, extending common test functionalities
/// from <see cref="BaseTest"/>. It handles SauceDemo-specific setup, including loading application settings,
/// navigating to the SauceDemo URL, and validating initial page state and configuration.
/// </summary>
/// <remarks>
/// This partial class is intended to be the direct base for actual test fixture classes targeting SauceDemo.
/// It is parameterized by <see cref="BrowserType"/> using NUnit's <c>[TestFixture]</c> attribute,
/// allowing tests to be run against different browsers.
/// The <see cref="SetUp"/> method overrides the base setup to:
/// <list type="bullet">
///   <item><description>Load <see cref="SauceDemoSettings"/>.</description></item>
///   <item><description>Validate essential settings like the Page URL and credentials.</description></item>
///   <item><description>Navigate to the SauceDemo login page (<see cref="SauceDemoSettings.PageUrl"/>).</description></item>
///   <item><description>Verify the initial page title.</description></item>
/// </list>
/// This class leverages Allure attributes for reporting suite, owner, and tags.
/// It's designed to be used in CI/CD environments ([user_input_previous_message_with_filename_programming.ci_cd]) where browser selection might be driven by environment variables.
/// </remarks>
[TestFixture(BrowserType.Chrome), Category("Chrome")]
// [TestFixture(BrowserType.Brave), Category("Brave")]
[TestFixture(BrowserType.Firefox), Category("Firefox")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[AllureSuite("SauceDemo Login and Inventory Tests")]
[AllureOwner("Kenan")]
[AllureTag("UI", "SauceDemo")]
[Category("UI")]
public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Holds the specific settings for the SauceDemo application, loaded from configuration
    /// during the <see cref="SetUp"/> method. This includes Page URL and login credentials.
    /// </summary>
    /// <value>The SauceDemo application settings. Null until successfully loaded in <see cref="SetUp"/>.</value>
    private SauceDemoSettings _sauceDemoSettings = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="SauceDemoTests"/> base class for a specific browser type.
    /// </summary>
    /// <param name="browserType">The <see cref="BrowserType"/> on which the tests derived from this base
    /// are intended to run. This is passed to the base <see cref="BaseTest"/> constructor.</param>
    /// <remarks>
    /// This constructor primarily calls the base class constructor to set up the common test infrastructure.
    /// SauceDemo-specific setup, including loading settings and navigation, occurs in the <see cref="SetUp"/> method.
    /// </remarks>
    public SauceDemoTests(BrowserType browserType)
        : base(browserType)
    {
    }

    /// <summary>
    /// Overrides the base <see cref="BaseTest.SetUp()"/> to perform SauceDemo-specific initialization
    /// after the common base setup is complete. This includes:
    /// <list type="bullet">
    ///   <item><description>Loading and validating <see cref="SauceDemoSettings"/>.</description></item>
    ///   <item><description>Navigating to the SauceDemo application URL specified in settings.</description></item>
    ///   <item><description>Verifying the initial page title against <see cref="LoginPageMap.PageTitle"/>.</description></item>
    ///   <item><description>Validating the presence of login credentials in the loaded settings.</description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// This method is decorated with <see cref="SetUpAttribute"/> and is called by NUnit before each test.
    /// If any validation (settings, URL, title, credentials) fails, a <see cref="ShouldAssertException"/>
    /// or other relevant exception is thrown, typically failing the test early.
    /// Performance of the navigation step is measured.
    /// Errors during setup are logged extensively.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if SauceDemo settings cannot be loaded, if WebDriver is not available for navigation, or if a WebDriverException occurs during navigation.</exception>
    /// <exception cref="ShouldAssertException">Thrown if URL, page title, or credential validations fail.</exception>
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        TestLogger.LogInformation("Starting SauceDemoTests-specific SetUp for {TestName} on {Browser}.",
            TestContext.CurrentContext.Test.FullName, BrowserType.ToString());

        TestLogger.LogDebug("Retrieving SauceDemo application settings.");
        try
        {
            _sauceDemoSettings = SettingsProvider.GetSettings<SauceDemoSettings>("SauceDemo");
            TestLogger.LogInformation("SauceDemo settings loaded successfully.");
        }
        catch (Exception ex)
        {
            TestLogger.LogError(ex, "Failed to retrieve or bind SauceDemo settings from configuration section 'SauceDemo'.");
            throw;
        }

        try
        {
            _sauceDemoSettings.PageUrl.ShouldNotBeNullOrEmpty("SauceDemo URL (PageUrl) not found in the SauceDemo configuration settings.");
            TestLogger.LogDebug("SauceDemo URL validated: {SauceDemoAppUrl}", _sauceDemoSettings.PageUrl);
        }
        catch (ShouldAssertException ex)
        {
            TestLogger.LogError(ex, "SauceDemo URL validation failed: {ShouldlyMessage}", ex.Message);
            throw;
        }

        TestLogger.LogInformation("Attempting to navigate to SauceDemo URL: {SauceDemoAppUrl}", _sauceDemoSettings.PageUrl);
        try
        {
            IWebDriver driver = WebDriverManager.GetDriver();

            using (new PerformanceTimer(
                $"NavigateTo_{LoginPageMap.PageTitle}",
                TestLogger, Microsoft.Extensions.Logging.LogLevel.Information,
                new Dictionary<string, object> { { "PageUrl", _sauceDemoSettings.PageUrl! } }
            ))
            {
                driver.Navigate().GoToUrl(_sauceDemoSettings.PageUrl);
                TestLogger.LogDebug("Navigation to {SauceDemoAppUrl} initiated.", _sauceDemoSettings.PageUrl);
            }

            driver.Title.ShouldBe(LoginPageMap.PageTitle, "The page title was not as expected.");
            TestLogger.LogInformation("Successfully navigated to {SauceDemoAppUrl} and verified page title '{ExpectedPageTitle}'.", _sauceDemoSettings.PageUrl, LoginPageMap.PageTitle);
        }
        catch (InvalidOperationException ex)
        {
            TestLogger.LogError(ex, "WebDriver was not available (null or not initialized) when attempting to navigate in SauceDemoTests.SetUp.");
            throw new InvalidOperationException("WebDriver was not initialized prior to navigating.", ex);
        }
        catch (WebDriverException ex)
        {
            TestLogger.LogError(ex, "A WebDriverException occurred during navigation to {SauceDemoAppUrl} or title check.", _sauceDemoSettings.PageUrl);
            throw new InvalidOperationException($"Error occurred while navigating to the SauceDemo page: {_sauceDemoSettings.PageUrl} or verifying its title.", ex);
        }
        catch (ShouldAssertException ex)
        {
            TestLogger.LogError(
                ex,
                "Page title verification failed for URL {SauceDemoAppUrl}. Expected '{ExpectedTitle}', Actual '{ActualTitle}'. Message: {ShouldlyMessage}",
                _sauceDemoSettings.PageUrl,
                LoginPageMap.PageTitle,
                WebDriverManager.GetDriver().Title,
                ex.Message
            );
            throw;
        }

        TestLogger.LogDebug("Validating SauceDemo login credentials from configuration.");
        try
        {
            _sauceDemoSettings.LoginUsernameStandardUser.ShouldNotBeNullOrEmpty("SauceDemo username not found in the configuration file");
            _sauceDemoSettings.LoginPassword.ShouldNotBeNullOrEmpty("SauceDemo password not found in the configuration file");
            TestLogger.LogDebug("SauceDemo login credentials (username & password presence) validated successfully from configuration.");
        }
        catch (ShouldAssertException ex)
        {
            TestLogger.LogError(ex, "SauceDemo credential validation failed: {ShouldlyMessage}", ex.Message);
            throw;
        }

        TestLogger.LogInformation(
            "SauceDemoTests-specific SetUp completed successfully for {TestName} on {Browser}.",
            TestContext.CurrentContext.Test.FullName,
            BrowserType.ToString()
        );
    }
}
