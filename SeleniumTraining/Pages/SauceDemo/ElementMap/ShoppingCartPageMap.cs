namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

/// <summary>
/// Contains locators for elements on the SauceDemo Shopping Cart Page,
/// as well as the expected page identifier (URL path).
/// </summary>
/// <remarks>
/// This static class centralizes element locators for the shopping cart page,
/// aiding in the maintainability and clarity of the <c>ShoppingCartPage</c> page object.
/// It also defines a collection of <see cref="ShoppingCartPageElements"/> considered critical for the page.
/// </remarks>
public static class ShoppingCartPageMap
{
    /// <summary>
    /// The expected relative URL path for the Shopping Cart page (e.g., "/cart.html").
    /// </summary>
    public static readonly string PageUrlPath = "/cart.html";

    /// <summary>
    /// Locator for an individual cart item container. Each item in the cart is wrapped by an element matching this locator.
    /// </summary>
    public static By CartItem => By.ClassName("cart_item");

    /// <summary>
    /// Locator for the "Continue Shopping" button on the cart page.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    public static By ContinueShoppingButton => SmartLocators.DataTest("continue-shopping");

    /// <summary>
    /// Locator for the "Checkout" button on the cart page.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    public static By CheckoutButton => SmartLocators.DataTest("checkout");

    /// <summary>
    /// Gets an array of <see cref="By"/> locators representing elements considered critical
    /// for the Shopping Cart Page to be deemed loaded and operational.
    /// Used by <see cref="BasePage.EnsureCriticalElementsAreDisplayed"/>.
    /// </summary>
    /// <value>An array of <see cref="By"/> objects for critical page elements, currently including the Checkout and Continue Shopping buttons.</value>
    /// <remarks>
    /// Individual cart items are not included as critical elements here because the cart can be legitimately empty.
    /// </remarks>
    public static By[] ShoppingCartPageElements { get; } = [
        CheckoutButton,
        ContinueShoppingButton
    ];
}
