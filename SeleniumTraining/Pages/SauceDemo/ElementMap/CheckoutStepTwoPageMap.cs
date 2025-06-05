namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

/// <summary>
/// Contains locators for elements on the "Checkout: Overview" page (Step Two)
/// of the SauceDemo application.
/// </summary>
/// <remarks>
/// This static class centralizes element locators for the second step of the checkout process (order overview),
/// aiding in the maintainability of the <see cref="CheckoutStepTwoPage"/> page object.
/// It includes locators for order summary details, cart items (though item details might be handled by components),
/// and action buttons.
/// </remarks>
public static class CheckoutStepTwoPageMap
{
    /// <summary>
    /// The expected URL path for the "Checkout: Overview" page.
    /// </summary>
    public static readonly string PageUrlPath = "/checkout-step-two.html";

    /// <summary>
    /// The expected title text displayed on the page (e.g., "Checkout: Overview").
    /// </summary>
    public static readonly string PageTitleText = "Checkout: Overview";

    /// <summary>
    /// Locator for the page title element.
    /// </summary>
    public static By PageTitle => By.ClassName("title");

    /// <summary>
    /// Locator for an individual cart item listed on the overview page.
    /// This is the same class as on the cart page.
    /// </summary>
    public static By CartItem => By.ClassName("cart_item");

    /// <summary>
    /// Locator for the payment information display.
    /// </summary>
    public static By PaymentInformationValue => By.CssSelector("[data-test='payment-info-value']");

    /// <summary>
    /// Locator for the shipping information display.
    /// </summary>
    public static By ShippingInformationValue => By.CssSelector("[data-test='shipping-info-value']");

    /// <summary>
    /// Locator for the subtotal (Item total) label.
    /// </summary>
    public static By SubtotalLabel => SmartLocators.DataTest("subtotal-label"); // "Item total: $X.YZ"

    /// <summary>
    /// Locator for the tax label.
    /// </summary>
    public static By TaxLabel => SmartLocators.DataTest("tax-label"); // "Tax: $X.YZ"

    /// <summary>
    /// Locator for the total price label.
    /// </summary>
    public static By TotalLabel => SmartLocators.DataTest("total-label"); // "Total: $X.YZ"

    /// <summary>
    /// Locator for the "Finish" button.
    /// </summary>
    public static By FinishButton => SmartLocators.DataTest("finish");

    /// <summary>
    /// Locator for the "Cancel" button.
    /// </summary>
    public static By CancelButton => SmartLocators.DataTest("cancel");

    /// <summary>
    /// Gets an array of <see cref="By"/> locators representing elements considered critical
    /// for the Checkout Step Two Page to be deemed loaded and operational.
    /// </summary>
    public static By[] CheckoutStepTwoPageElements { get; } = [
        PageTitle,
        FinishButton,
        SubtotalLabel // A good indicator that summary details are present
    ];
}
