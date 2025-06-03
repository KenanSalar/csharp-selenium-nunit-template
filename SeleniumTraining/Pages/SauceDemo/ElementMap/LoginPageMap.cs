namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

/// <summary>
/// Contains locators for elements on the SauceDemo Login Page,
/// as well as the expected page title.
/// </summary>
/// <remarks>
/// This static class centralizes element locators and key page identifiers for the login page,
/// aiding in the maintainability and clarity of the <c>LoginPage</c> page object.
/// <see cref="SmartLocators"/> are used for robust element identification, likely targeting 'data-test' attributes.
/// It also defines a collection of <see cref="LoginPageElements"/> considered critical for the page.
/// </remarks>
public static class LoginPageMap
{
    /// <summary>
    /// The expected title of the SauceDemo Login Page ("Swag Labs").
    /// </summary>
    /// <value>A string representing the expected page title.</value>
    public static readonly string PageTitle = "Swag Labs";

    /// <summary>
    /// Gets the locator for the username input field on the login form.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the username input field.</value>
    public static By UsernameInput => SmartLocators.DataTest("username");

    /// <summary>
    /// Gets the locator for the password input field on the login form.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the password input field.</value>
    public static By PasswordInput => SmartLocators.DataTest("password");

    /// <summary>
    /// Gets the locator for the login button on the login form.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the login button.</value>
    public static By LoginButton => SmartLocators.DataTest("login-button");

    /// <summary>
    /// Gets the locator for the container element that displays error messages on login failure.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the error message container.</value>
    public static By ErrorMessageContainer => SmartLocators.DataTest("error");

    /// <summary>
    /// Gets an array of <see cref="By"/> locators representing elements considered critical
    /// for the Login Page to be deemed loaded and operational.
    /// Used by <see cref="BasePage.EnsureCriticalElementsAreDisplayed"/>.
    /// </summary>
    /// <value>An array of <see cref="By"/> objects for critical page elements.</value>
    /// <remarks>
    /// This collection currently includes the <see cref="UsernameInput"/>, <see cref="PasswordInput"/>,
    /// and <see cref="LoginButton"/>.
    /// </remarks>
    public static By[] LoginPageElements { get; } = [
        UsernameInput,
        PasswordInput,
        LoginButton
    ];
}
