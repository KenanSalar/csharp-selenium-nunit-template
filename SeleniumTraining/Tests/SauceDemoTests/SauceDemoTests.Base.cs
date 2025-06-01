namespace SeleniumTraining.Tests.SauceDemoTests;

// [TestFixture(BrowserType.Brave), Category("Brave")]
[TestFixture(BrowserType.Chrome), Category("Chrome")]
[TestFixture(BrowserType.Firefox), Category("Firefox")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[AllureSuite("SauceDemo Login and Inventory Tests")]
[AllureOwner("Kenan")]
[AllureTag("UI", "SauceDemo")]
[Category("UI")]
public partial class SauceDemoTests : BaseTest
{
    private SauceDemoSettings _sauceDemoSettings = null!;

    public SauceDemoTests(BrowserType browserType)
        : base(browserType)
    {
    }

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
