namespace SeleniumTraining.Tests.SauceDemoTests;

/// <summary>
/// Defines a comprehensive test fixture for UI testing of the SauceDemo application.
/// This fixture groups a variety of test scenarios, including different user login behaviors,
/// inventory page interactions (like product sorting and image verification), and visual validation checks.
/// It is designed to run tests across multiple browsers, as specified by NUnit's [TestFixture] attributes.
/// </summary>
/// <remarks>
/// The <c>SauceDemoTests</c> class is implemented as a <see langword="partial"/> class,
/// with its constituent parts organized into separate files for better maintainability and clarity:
/// <list type="bullet">
///   <item>
///     <term>SauceDemoTests.Base.cs</term>
///     <description>Contains the core fixture setup, including NUnit's <c>[TestFixture]</c> attributes for browser parameterization,
///     the class constructor, common SauceDemo-specific fields (like <see cref="_sauceDemoSettings"/>),
///     and an overridden <see cref="SetUp"/> method for SauceDemo-specific initializations (e.g., loading settings, initial navigation).
///     It inherits common test functionalities from <see cref="BaseTest"/>.</description>
///   </item>
///   <item>
///     <term>SauceDemoTests.Data.cs</term>
///     <description>Holds test data specifically related to the SauceDemo application, such as the
///     <see cref="_inventoryProductsDropdownOptions"/> list used for verifying product sorting.</description>
///   </item>
///   <item>
///     <term>SauceDemoTests.Login.cs</term>
///     <description>Contains test methods that primarily focus on validating different login scenarios and outcomes
///     directly on the login page. This includes tests for users like the "locked_out_user" and potentially other login error conditions.
///     It now also includes the test for the "problem_user" focusing on inventory display issues post-login.</description>
///   </item>
///   <item>
///     <term>SauceDemoTests.Inventory.cs</term>
///     <description>Groups test methods that, following a successful login, concentrate on interactions with and verifications of
///     the inventory page. This includes testing standard user flows like product sorting, and behaviors specific to user types
///     like the "visual_user" for visual regression checks.</description>
///   </item>
/// </list>
/// All test methods leverage NUnit attributes for execution control (e.g., <c>[Test]</c>, <c>[Retry]</c>) and
/// Allure attributes (e.g., <c>[AllureStep]</c>, <c>[AllureSeverity]</c>) for comprehensive reporting.
/// The underlying <see cref="BaseTest"/> class provides shared infrastructure such as WebDriver management,
/// DI service resolution, logging, and performance/resource monitoring capabilities.
/// </remarks>
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

    /// <summary>
    /// Logs in as the standard_user and navigates to the InventoryPage.
    /// Includes performance timing and logging for the login action.
    /// </summary>
    /// <returns>An instance of <see cref="InventoryPage"/> upon successful login and navigation.</returns>
    /// <remarks>This method centralizes the standard user login logic.</remarks>
    protected InventoryPage LoginAsStandardUserAndNavigateToInventoryPage()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Attempting to log in as standard_user for test: {TestName}", currentTestName);

        InventoryPage inventoryPage;
        var loginOperationProps = new Dictionary<string, object>
        {
            { "Username", _sauceDemoSettings.LoginUsernameStandardUser },
            { "LoginAction", LoginMode.Click.ToString() },
            { "TestName", currentTestName }
        };

        using (var loginTimer = new PerformanceTimer(
                   "Helper_LoginAsStandardUser",
                   TestLogger,
                   Microsoft.Extensions.Logging.LogLevel.Information,
                   loginOperationProps,
                   ResourceMonitor))
        {
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
            inventoryPage = loginPage
                .EnterUsername(_sauceDemoSettings.LoginUsernameStandardUser)
                .EnterPassword(_sauceDemoSettings.LoginPassword)
                .LoginAndExpectNavigation(LoginMode.Click)
                .ShouldBeOfType<InventoryPage>("Login as standard_user should lead to Inventory Page.");

            loginTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 7000);
        }

        TestLogger.LogInformation("Successfully logged in as standard_user and navigated to InventoryPage for test: {TestName}", currentTestName);
        return inventoryPage;
    }
}
