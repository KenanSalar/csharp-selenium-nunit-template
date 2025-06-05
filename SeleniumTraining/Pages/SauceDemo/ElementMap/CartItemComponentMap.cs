namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

/// <summary>
/// Contains locators for elements within a single item displayed in the shopping cart
/// on the SauceDemo site. These locators are used by the <see cref="CartItemComponent"/>.
/// </summary>
public static class CartItemComponentMap
{
    /// <summary>
    /// Locator for the name of the item in the cart.
    /// Consider verifying if 'data-test="inventory-item-name"' is actually used on the cart page item name,
    /// or if a class name like 'inventory_item_name' is more appropriate.
    /// </summary>
    public static By ItemName => SmartLocators.DataTest("inventory-item-name");

    /// <summary>
    /// Locator for the description of the item in the cart.
    /// Consider verifying if 'data-test="inventory-item-desc"' is used on the cart page,
    /// or if a class name like 'inventory_item_desc' is more appropriate.
    /// </summary>
    public static By ItemDescription => SmartLocators.DataTest("inventory-item-desc");

    /// <summary>
    /// Locator for the price of the item in the cart.
    /// Consider verifying if 'data-test="inventory-item-price"' is used on the cart page,
    /// or if a class name like 'inventory_item_price' is more appropriate.
    /// </summary>
    public static By ItemPrice => SmartLocators.DataTest("inventory-item-price");

    /// <summary>
    /// Locator for the quantity display of the item in the cart.
    /// </summary>
    public static By ItemQuantity => By.ClassName("cart_quantity");

    /// <summary>
    /// Locator for the "Remove" button for an item in the cart.
    /// Uses a smart locator to find a button element that contains the class 'cart_button'
    /// and has the exact text "Remove".
    /// </summary>
    /// <remarks>
    /// The actual SauceDemo HTML uses more specific data-test attributes for remove buttons
    /// (e.g., data-test="remove-sauce-labs-backpack").
    /// This locator is a more generic fallback using text and class.
    /// </remarks>
    public static By RemoveButton => SmartLocators.TextContains("Remove", "cart_button", "button");
}
