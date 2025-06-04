namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Verifies that a "locked out" user cannot log in successfully and receives the appropriate error message.
    /// The login attempt is made using the 'Submit' action (e.g., pressing Enter in the password field).
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Instantiates the LoginPage.</description></item>
    ///   <item><description>Enters credentials for a "locked out" user.</description></item>
    ///   <item><description>Performs login using the 'Submit' mode and expects to remain on the LoginPage.</description></item>
    ///   <item><description>Asserts that the current page is still the LoginPage.</description></item>
    ///   <item><description>Retrieves the error message displayed on the LoginPage.</description></item>
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
            { "Username", _sauceDemoSettings.LoginUsernameStandardUser },
            { "LoginAction", LoginMode.Click.ToString() }
        };

        var loginAttemptTimer = new PerformanceTimer(
            "TestStep_UserLogin_Standard",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginAttemptProps,
            ResourceMonitor
        );

        bool loginAttemptSuccessAsExpected = false;

        BasePage resultPage;
        try
        {
            TestLogger.LogDebug("Instantiating LoginPage.");
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);

            resultPage = loginPage
                .EnterUsername(_sauceDemoSettings.LoginUsernameLockedOutUser)
                .EnterPassword(_sauceDemoSettings.LoginPassword)
                .LoginAndExpectNavigation(loginMode);

            loginAttemptSuccessAsExpected = resultPage is LoginPage;
        }
        finally
        {
            loginAttemptTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: loginAttemptSuccessAsExpected ? 5000 : null);
            loginAttemptTimer.Dispose();
        }

        TestLogger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameLockedOutUser,
            loginMode
        );

        LoginPage loginPageInstance = resultPage.ShouldBeOfType<LoginPage>("User should have remained on the Login Page.");

        var errorMsgTimer = new PerformanceTimer(
            "TestStep_GetLoginErrorMessage_LockedOut",
            TestLogger,
            resourceMonitor: ResourceMonitor
        );

        string actualErrorMessage;
        try
        {
            actualErrorMessage = loginPageInstance.GetErrorMessage();
        }
        finally
        {
            errorMsgTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 1000);
            errorMsgTimer.Dispose();
        }

        actualErrorMessage.ShouldBe(SauceDemoMessages.LockedOutUserError, $"Error message should be: {SauceDemoMessages.LockedOutUserError} but was: {actualErrorMessage}");

        TestLogger.LogInformation("Login not successful, currently on LoginPage.");
        TestLogger.LogInformation("Finished test: {TestName}", currentTestName);
    }
}
