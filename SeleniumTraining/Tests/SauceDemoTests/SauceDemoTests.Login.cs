namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Verifies that a user with "locked out" credentials cannot log in to the SauceDemo application.
    /// This test specifically uses the 'Submit' action on the login form to trigger the login attempt.
    /// </summary>
    /// <remarks>
    /// This test is critical for verifying the application's security and error handling for a known invalid user state.
    /// <para><b>Test Steps:</b></para>
    /// <list type="number">
    ///   <item>
    ///     <description>Instantiates the <see cref="LoginPage"/> and asserts it has loaded correctly.</description>
    ///   </item>
    ///   <item>
    ///     <description>Enters the username for the 'locked_out_user' and the corresponding password, retrieved from configuration settings.</description>
    ///   </item>
    ///   <item>
    ///     <description>Calls the <see cref="LoginPage.LoginAndExpectFailure(LoginMode)"/> method, which performs the login action via form submission without verifying successful navigation. This makes the test's intent explicit.</description>
    ///   </item>
    ///   <item>
    ///     <description>Retrieves the error message displayed on the page after the failed login attempt.</description>
    ///   </item>
    ///   <item>
    ///     <description>Asserts that the retrieved error message matches the expected message for a locked-out user, as defined in <see cref="SauceDemoMessages.LockedOutUserError"/>.</description>
    ///   </item>
    /// </list>
    /// The performance of both the login attempt and the subsequent error message retrieval are measured and logged using the <see cref="PerformanceTimer"/> utility, with results attached to the Allure report.
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

            loginPage = new LoginPage(LifecycleManager.WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService)
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

        Result<string, string> errorMessageResult = loginPage.GetErrorMessage();

        switch (errorMessageResult)
        {
            case Result<string, string>.SuccessResult success:
                success.Value.ShouldBe(SauceDemoMessages.LockedOutUserError, "The error message was not as expected.");
                TestLogger.LogInformation("Verified correct error message is displayed: '{ErrorMessage}'", success.Value);
                break;

            case Result<string, string>.FailureResult failure:
                Assert.Fail($"Could not retrieve the login error message. Reason: {failure.Error}");
                break;

            default:
                break;
        }

        TestLogger.LogInformation("Finished test: {TestName}", TestContext.CurrentContext.Test.Name);
    }
}
