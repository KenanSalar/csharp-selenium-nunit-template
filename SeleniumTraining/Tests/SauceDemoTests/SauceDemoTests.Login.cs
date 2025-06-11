namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Instantiates the LoginPage.</description></item>
    ///   <item><description>Enters credentials for a "locked out" user.</description></item>
    ///   <item><description>Calls the <see cref="LoginPage.LoginAndExpectFailure"/> method, which performs the login action without verifying navigation. This makes the test's intent explicit.</description></item>
    ///   <item><description>Retrieves the error message displayed on the returned LoginPage instance.</description></item>
    ///   <item><description>Asserts that the error message matches the expected message for a locked out user (<see cref="SauceDemoMessages.LockedOutUserError"/>).</description></item>
    /// </list>
    /// This test is critical for verifying error handling and security aspects of the login process.
    /// Performance and resource usage (memory) of the login attempt and error message retrieval are measured.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Login with Submit for locked_out_user")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies user login with the locked_out_user, using the Submit action.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldNotLoginSuccessfullyWithLockedOutUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName}", currentTestName);

        const LoginMode loginMode = LoginMode.Submit;

        var loginAttemptProps = new Dictionary<string, object>
    {
        { "Username", _sauceDemoSettings.LoginUsernameLockedOutUser },
        { "LoginAction", loginMode.ToString() }
    };

        var loginAttemptTimer = new PerformanceTimer(
            "TestStep_UserLogin_LockedOut_Failure",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginAttemptProps,
            ResourceMonitor
        );

        LoginPage loginPage;
        try
        {
            TestLogger.LogDebug("Instantiating LoginPage.");
            var initialPage = new LoginPage(LifecycleManager.WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);

            loginPage = initialPage
                .EnterUsername(_sauceDemoSettings.LoginUsernameLockedOutUser)
                .EnterPassword(_sauceDemoSettings.LoginPassword)
                .LoginAndExpectFailure(loginMode);
        }
        finally
        {
            loginAttemptTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
            loginAttemptTimer.Dispose();
        }

        TestLogger.LogInformation("Login attempt completed for {LoginUsername}. Verifying error message.", _sauceDemoSettings.LoginUsernameLockedOutUser);

        var errorMsgTimer = new PerformanceTimer(
            "TestStep_GetLoginErrorMessage_LockedOut",
            TestLogger,
            resourceMonitor: ResourceMonitor
        );

        string actualErrorMessage;
        try
        {
            actualErrorMessage = loginPage.GetErrorMessage();
        }
        finally
        {
            errorMsgTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 1000);
            errorMsgTimer.Dispose();
        }

        actualErrorMessage.ShouldBe(SauceDemoMessages.LockedOutUserError, $"The error message was not as expected.");

        TestLogger.LogInformation("Verified correct error message is displayed. Test passed.");
        TestLogger.LogInformation("Finished test: {TestName}", currentTestName);
    }
}
