namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

/// <summary>
/// Contains locators for elements within a single inventory item component
/// as displayed on the SauceDemo inventory page.
/// </summary>
/// <remarks>
/// This static class centralizes the element locators for an individual product item,
/// making them easily accessible and maintainable for the <c>InventoryItemComponent</c>.
/// It utilizes <see cref="SmartLocators"/> for some locators, implying a custom locator strategy
/// that might be based on data-test attributes or other robust selection methods.
/// </remarks>
public static class InventoryItemComponentMap
{
    /// <summary>
    /// Gets the locator for the image of the inventory item.
    /// </summary>
    /// <value>A <see cref="By"/> object for the item's image element.</value>
    public static By ItemImage => By.CssSelector("img.inventory_item_img");

    /// <summary>
    /// Gets the locator for the name or title of the inventory item.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the item's name element.</value>
    public static By ItemName => SmartLocators.DataTest("inventory-item-name");

    /// <summary>
    /// Gets the locator for the description text of the inventory item.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the item's description element.</value>
    public static By ItemDescription => SmartLocators.DataTest("inventory-item-desc");

    /// <summary>
    /// Gets the locator for the price display of the inventory item.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the item's price element.</value>
    public static By ItemPrice => SmartLocators.DataTest("inventory-item-price");

    /// <summary>
    /// Gets the locator for the action button associated with the inventory item
    /// (e.g., "Add to cart" or "Remove" button).
    /// </summary>
    /// <value>A <see cref="By"/> object for the item's action button.</value>
    public static By ActionButton => By.CssSelector("button.btn_inventory");
}
