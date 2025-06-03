namespace SeleniumTraining.Tests.SauceDemoTests;

/// <summary>
/// Provides a collection of constant string messages related to the SauceDemo application,
/// primarily used for assertions, error message verification, or logging in tests.
/// </summary>
/// <remarks>
/// This static class centralizes common string literals encountered in the SauceDemo application,
/// promoting consistency and maintainability in test assertions.
/// For example, it includes error messages for specific user login scenarios.
/// These constants are particularly useful in automated UI tests ([user_input_previous_message_with_filename_BasePage.cs]) for verifying expected outcomes.
/// </remarks>
public static class SauceDemoMessages
{
    /// <summary>
    /// The error message displayed by SauceDemo when a user attempts to log in
    /// with credentials for a "locked out" user.
    /// Expected value: "Epic sadface: Sorry, this user has been locked out."
    /// </summary>
    /// <remarks>
    /// This constant should be used in assertions to verify the correct error message
    /// appears on the login page for this specific scenario.
    /// </remarks>
    public const string LockedOutUserError = "Epic sadface: Sorry, this user has been locked out.";
}
