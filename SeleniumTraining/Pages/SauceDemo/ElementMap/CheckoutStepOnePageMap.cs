namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

/// <summary>
/// Contains locators for elements on the "Checkout: Your Information" page (Step One)
/// of the SauceDemo application.
/// </summary>
/// <remarks>
/// This static class centralizes element locators for the first step of the checkout process,
/// aiding in the maintainability of the <see cref="CheckoutStepOnePage"/> page object.
/// It includes locators for input fields, action buttons, and the page title.
/// </remarks>
public static class CheckoutStepOnePageMap
{
    /// <summary>
    /// The expected URL path for the "Checkout: Your Information" page.
    /// </summary>
    public static readonly string PageUrlPath = "/checkout-step-one.html";

    /// <summary>
    /// The expected title text displayed on the page (e.g., "Checkout: Your Information").
    /// </summary>
    public static readonly string PageTitleText = "Checkout: Your Information";

    /// <summary>
    /// Locator for the page title element.
    /// </summary>
    public static By PageTitle => By.ClassName("title");

    /// <summary>
    /// Locator for the First Name input field.
    /// </summary>
    public static By FirstNameInput => SmartLocators.DataTest("firstName");

    /// <summary>
    /// Locator for the Last Name input field.
    /// </summary>
    public static By LastNameInput => SmartLocators.DataTest("lastName");

    /// <summary>
    /// Locator for the Zip/Postal Code input field.
    /// </summary>
    public static By PostalCodeInput => SmartLocators.DataTest("postalCode");

    /// <summary>
    /// Locator for the "Continue" button.
    /// </summary>
    public static By ContinueButton => SmartLocators.DataTest("continue");

    /// <summary>
    /// Locator for the "Cancel" button.
    /// </summary>
    public static By CancelButton => SmartLocators.DataTest("cancel");

    /// <summary>
    /// Locator for the container that displays error messages on this page.
    /// </summary>
    public static By ErrorMessageContainer => SmartLocators.DataTest("error");

    /// <summary>
    /// Gets an array of <see cref="By"/> locators representing elements considered critical
    /// for the Checkout Step One Page to be deemed loaded and operational.
    /// </summary>
    public static By[] CheckoutStepOnePageElements { get; } = [
        PageTitle,
        FirstNameInput,
        LastNameInput,
        PostalCodeInput,
        ContinueButton
    ];
}
