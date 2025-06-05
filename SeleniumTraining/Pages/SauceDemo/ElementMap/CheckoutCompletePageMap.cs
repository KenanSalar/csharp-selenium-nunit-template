namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

/// <summary>
/// Contains locators for elements on the "Checkout: Complete!" page
/// of the SauceDemo application.
/// </summary>
/// <remarks>
/// This static class centralizes element locators for the order confirmation page,
/// aiding in the maintainability of the <see cref="CheckoutCompletePage"/> page object.
/// </remarks>
public static class CheckoutCompletePageMap
{
    /// <summary>
    /// The expected URL path for the "Checkout: Complete!" page.
    /// </summary>
    public static readonly string PageUrlPath = "/checkout-complete.html";

    /// <summary>
    /// The expected title text displayed on the page (e.g., "Checkout: Complete!").
    /// </summary>
    public static readonly string PageTitleText = "Checkout: Complete!";

    /// <summary>
    /// Locator for the page title element.
    /// </summary>
    public static By PageTitle => By.ClassName("title");

    /// <summary>
    /// Locator for the main confirmation header text (e.g., "Thank you for your order!").
    /// On SauceDemo, this is "THANK YOU FOR YOUR ORDER".
    /// </summary>
    public static By CompleteHeader => By.ClassName("complete-header");

    /// <summary>
    /// Locator for the descriptive text on the completion page.
    /// </summary>
    public static By CompleteText => By.ClassName("complete-text");

    /// <summary>
    /// Locator for the "Pony Express" image on the completion page.
    /// </summary>
    public static By PonyExpressImage => By.ClassName("pony_express");

    /// <summary>
    /// Locator for the "Back Home" button.
    /// </summary>
    public static By BackHomeButton => SmartLocators.DataTest("back-to-products");

    /// <summary>
    /// Gets an array of <see cref="By"/> locators representing elements considered critical
    /// for the Checkout Complete Page to be deemed loaded and operational.
    /// </summary>
    public static By[] CheckoutCompletePageElements { get; } = [
        PageTitle,
        CompleteHeader,
        BackHomeButton
    ];
}
