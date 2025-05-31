namespace SeleniumTraining.Tests.SauceDemoTests;

// [TestFixture(BrowserType.Brave), Category("Brave")]
[TestFixture(BrowserType.Chrome), Category("Chrome")]
[TestFixture(BrowserType.Firefox), Category("Firefox")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[AllureSuite("SauceDemo Login and Inventory Tests")]
[AllureOwner("Kenan")]
[AllureTag("UI", "SauceDemo")]
public partial class SauceDemoTests : BaseTest
{
    private SauceDemoSettings _sauceDemoSettings = null!;

    public SauceDemoTests(BrowserType browserType) : base(browserType)
    {
    }

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        Logger.LogInformation("Starting SauceDemoTests-specific SetUp for {TestName} on {Browser}.",
            TestContext.CurrentContext.Test.FullName, BrowserType.ToString());

        Logger.LogDebug("Retrieving SauceDemo application settings.");
        try
        {
            _sauceDemoSettings = SettingsProvider.GetSettings<SauceDemoSettings>("SauceDemo");
            Logger.LogInformation("SauceDemo settings loaded successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to retrieve or bind SauceDemo settings from configuration section 'SauceDemo'.");
            throw;
        }

        try
        {
            _sauceDemoSettings.PageUrl.ShouldNotBeNullOrEmpty("SauceDemo URL (PageUrl) not found in the SauceDemo configuration settings.");
            Logger.LogDebug("SauceDemo URL validated: {SauceDemoAppUrl}", _sauceDemoSettings.PageUrl);
        }
        catch (ShouldAssertException ex)
        {
            Logger.LogError(ex, "SauceDemo URL validation failed: {ShouldlyMessage}", ex.Message);
            throw;
        }

        Logger.LogInformation("Attempting to navigate to SauceDemo URL: {SauceDemoAppUrl}", _sauceDemoSettings.PageUrl);
        try
        {
            IWebDriver driver = WebDriverManager.GetDriver();

            using (new PerformanceTimer(
                $"NavigateTo_{LoginPageMap.PageTitle}",
                Logger, Microsoft.Extensions.Logging.LogLevel.Information,
                new Dictionary<string, object> { { "PageUrl", _sauceDemoSettings.PageUrl! } }
            ))
            {
                driver.Navigate().GoToUrl(_sauceDemoSettings.PageUrl);
                Logger.LogDebug("Navigation to {SauceDemoAppUrl} initiated.", _sauceDemoSettings.PageUrl);
            }

            driver.Title.ShouldBe(LoginPageMap.PageTitle, "The page title was not as expected.");
            Logger.LogInformation("Successfully navigated to {SauceDemoAppUrl} and verified page title '{ExpectedPageTitle}'.", _sauceDemoSettings.PageUrl, LoginPageMap.PageTitle);
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogError(ex, "WebDriver was not available (null or not initialized) when attempting to navigate in SauceDemoTests.SetUp.");
            throw new InvalidOperationException("WebDriver was not initialized prior to navigating.", ex);
        }
        catch (WebDriverException ex)
        {
            Logger.LogError(ex, "A WebDriverException occurred during navigation to {SauceDemoAppUrl} or title check.", _sauceDemoSettings.PageUrl);
            throw new InvalidOperationException($"Error occurred while navigating to the SauceDemo page: {_sauceDemoSettings.PageUrl} or verifying its title.", ex);
        }
        catch (ShouldAssertException ex)
        {
            Logger.LogError(
                ex,
                "Page title verification failed for URL {SauceDemoAppUrl}. Expected '{ExpectedTitle}', Actual '{ActualTitle}'. Message: {ShouldlyMessage}",
                _sauceDemoSettings.PageUrl,
                LoginPageMap.PageTitle,
                WebDriverManager.GetDriver().Title,
                ex.Message
            );
            throw;
        }

        Logger.LogDebug("Validating SauceDemo login credentials from configuration.");
        try
        {
            _sauceDemoSettings.LoginUsernameStandardUser.ShouldNotBeNullOrEmpty("SauceDemo username not found in the configuration file");
            _sauceDemoSettings.LoginPassword.ShouldNotBeNullOrEmpty("SauceDemo password not found in the configuration file");
            Logger.LogDebug("SauceDemo login credentials (username & password presence) validated successfully from configuration.");
        }
        catch (ShouldAssertException ex)
        {
            Logger.LogError(ex, "SauceDemo credential validation failed: {ShouldlyMessage}", ex.Message);
            throw;
        }

        Logger.LogInformation(
            "SauceDemoTests-specific SetUp completed successfully for {TestName} on {Browser}.",
            TestContext.CurrentContext.Test.FullName,
            BrowserType.ToString()
        );
    }
}
