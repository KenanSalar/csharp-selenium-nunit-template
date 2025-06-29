namespace SeleniumTraining.Tests.SauceDemoTests;

/// <summary>
/// Defines a comprehensive test fixture for UI testing of the SauceDemo application.
/// This fixture groups a variety of test scenarios and is parameterized to run across
/// multiple browsers as specified by the [TestFixture] attributes.
/// </summary>
/// <remarks>
/// This <see langword="partial"/> class serves as the base for all SauceDemo tests, containing
/// the core fixture setup for browser parameterization, the class constructor, and an
/// overridden <see cref="SetUp"/> method for test-specific initializations. It inherits
/// common test functionalities from <see cref="BaseTest"/>.
/// </remarks>
[TestFixture(BrowserType.Chrome), Category("Chrome")]
[TestFixture(BrowserType.Edge), Category("Edge")]
[TestFixture(BrowserType.Firefox), Category("Firefox")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[AllureSuite("SauceDemo Login and Inventory Tests")]
[AllureOwner("Kenan")]
[AllureTag("UI", "SauceDemo")]
public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Holds the specific settings for the SauceDemo application, loaded from configuration
    /// during the <see cref="SetUp"/> method. This includes Page URL and login credentials.
    /// </summary>
    /// <value>The SauceDemo application settings. Null until successfully loaded in <see cref="SetUp"/>.</value>
    private SauceDemoSettings _sauceDemoSettings = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="SauceDemoTests"/> class for a specific browser type.
    /// </summary>
    /// <param name="browserType">The <see cref="BrowserType"/> on which the tests are intended to run,
    /// passed by the NUnit [TestFixture] attribute.</param>
    public SauceDemoTests(BrowserType browserType)
        : base(browserType)
    {
    }

    /// <summary>
    /// Overrides the base <see cref="BaseTest.SetUp()"/> to perform SauceDemo-specific initialization
    /// after the common base setup is complete.
    /// </summary>
    /// <remarks>
    /// This method loads the <see cref="SauceDemoSettings"/> from configuration, navigates to the
    /// application URL, and verifies the page title before any test logic runs.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if settings cannot be loaded or navigation fails.</exception>
    /// <exception cref="ShouldAssertException">Thrown if URL or page title validations fail.</exception>
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        TestLogger.LogInformation("Starting SauceDemoTests-specific SetUp for {TestName} on {Browser}.",
            TestContext.CurrentContext.Test.FullName, BrowserType.ToString());

        TestLogger.LogDebug("Retrieving SauceDemo application settings.");
        try
        {
            _sauceDemoSettings = SettingsProvider.GetSettings<SauceDemoSettings>(ConfigurationKeys.SauceDemo);
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
            IWebDriver driver = LifecycleManager.WebDriverManager.GetDriver();

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
                LifecycleManager.WebDriverManager.GetDriver().Title,
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
