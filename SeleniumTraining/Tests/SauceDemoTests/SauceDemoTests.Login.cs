namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Verifies the behavior of the inventory page when logged in as the "problem_user".
    /// This user typically displays the same incorrect image for all products.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Instantiates the LoginPage.</description></item>
    ///   <item><description>Enters credentials for the "problem_user".</description></item>
    ///   <item><description>Performs login using 'Click' mode and expects navigation to InventoryPage.</description></item>
    ///   <item><description>Asserts that the current page is indeed the InventoryPage.</description></item>
    ///   <item><description>Retrieves all inventory items.</description></item>
    ///   <item><description>Asserts that there is more than one item displayed (to make the image check meaningful).</description></item>
    ///   <item><description>Gets the 'src' attribute of each product image.</description></item>
    ///   <item><description>Asserts that all product images have the same 'src' attribute, indicating they are all the same image.</description></item>
    /// </list>
    /// Performance and resource usage of login and the image verification are measured.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Login and Verify Product Images for problem_user")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureDescription("Verifies that when logged in as problem_user, all product images on the inventory page are identical.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldDisplaySameImageForAllProductsForProblemUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName} for problem_user", currentTestName);

        BasePage resultPage;
        const LoginMode loginMode = LoginMode.Click;

        var loginOperationProps = new Dictionary<string, object>
        {
            { "Username", _sauceDemoSettings.LoginUsernameProblemUser },
            { "LoginAction", loginMode.ToString() }
        };

        var loginTimer = new PerformanceTimer(
            "TestStep_UserLogin_ProblemUser",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginOperationProps,
            ResourceMonitor
        );
        bool loginStepSuccess = false;

        TestLogger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameProblemUser,
            loginMode
        );

        try
        {
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
            resultPage = loginPage
                .EnterUsername(_sauceDemoSettings.LoginUsernameProblemUser)
                .EnterPassword(_sauceDemoSettings.LoginPassword)
                .LoginAndExpectNavigation(loginMode);
                
            loginStepSuccess = resultPage is InventoryPage;
        }
        finally
        {
            loginTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: loginStepSuccess ? 7000 : null);
            loginTimer.Dispose();
        }

        InventoryPage inventoryPage = resultPage.ShouldBeOfType<InventoryPage>("Login as problem_user should be successful and navigate to the Inventory Page.");
        TestLogger.LogInformation("Login successful as problem_user, currently on InventoryPage.");

        var imageVerificationTimer = new PerformanceTimer(
            "TestStep_VerifyProductImages_ProblemUser",
            TestLogger,
            resourceMonitor: ResourceMonitor
        );

        var productImageSources = new List<string>();
        try
        {
            TestLogger.LogInformation("Retrieving inventory items to check product images.");
            IEnumerable<InventoryItemComponent> items = inventoryPage.GetInventoryItems();

            items.Count().ShouldBeGreaterThan(1, "There should be multiple items to compare images.");

            foreach (InventoryItemComponent item in items)
            {
                try
                {
                    IWebElement itemImageElement = item.ItemImage; // Access the property
                    if (itemImageElement != null && itemImageElement.Displayed)
                    {
                        string? imgSrc = itemImageElement.GetAttribute("src");
                        if (!string.IsNullOrEmpty(imgSrc))
                        {
                            productImageSources.Add(imgSrc);
                            TestLogger.LogDebug("Item '{ItemName}' image src: {ImageSrc}", item.ItemName, imgSrc);
                        }
                        else
                        {
                            TestLogger.LogWarning("Item '{ItemName}' image src attribute is null or empty.", item.ItemName);
                        }
                    }
                    else
                    {
                        TestLogger.LogWarning("Item '{ItemName}' image element is not displayed or null.", item.ItemName);
                    }
                }
                catch (Exception ex)
                {
                    TestLogger.LogError(ex, "Error getting image source for item: {ItemName}", item.ItemName);
                }
            }

            productImageSources.Count.ShouldBe(items.Count(), "Should have retrieved an image source for each item.");

            if (productImageSources.Count != 0)
            {
                int distinctImageSourcesCount = productImageSources.Distinct().Count();
                distinctImageSourcesCount.ShouldBe(1, $"Expected all product images to be the same for problem_user, but found {distinctImageSourcesCount} distinct image sources. Sources: [{string.Join(", ", productImageSources.Distinct())}]");
                TestLogger.LogInformation("Verified that all {ImageCount} product images have the same source: {ImageSrc_Example}", productImageSources.Count, productImageSources.First());
            }
            else
            {
                Assert.Fail("No product image sources were collected, cannot verify problem_user image issue.");
            }
        }
        finally
        {
            imageVerificationTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
            imageVerificationTimer.Dispose();
        }

        TestLogger.LogInformation("Finished test: {TestName} for problem_user", currentTestName);
    }
}
