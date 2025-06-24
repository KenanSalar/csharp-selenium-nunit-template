namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

/// <summary>
/// Contains locators for various elements found on the SauceDemo Inventory Page.
/// </summary>
/// <remarks>
/// This static class centralizes element locators for the main inventory page,
/// facilitating easier maintenance and access for the <c>InventoryPage</c> page object.
/// It employs <see cref="SmartLocators"/> for many elements, likely targeting 'data-test' attributes.
/// It also defines a collection of <see cref="InventoryPageElements"/> considered critical for the page.
/// </remarks>
public static class InventoryPageMap
{
    /// <summary>
    /// Gets the locator for the product sort dropdown container.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the sort dropdown.</value>
    public static By SortDropdown => SmartLocators.DataTest("product-sort-container");

    /// <summary>
    /// Gets the locator for the main container holding all inventory items.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the inventory container.</value>
    public static By InventoryContainer => SmartLocators.DataTest("inventory-container");

    /// <summary>
    /// Gets the locator for the list element that contains the inventory items.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the inventory item list.</value>
    public static By InventoryItemList => SmartLocators.DataTest("inventory-list");

    /// <summary>
    /// Gets the locator used to find individual inventory item elements within the list.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for an individual inventory item.</value>
    public static By InventoryItem => SmartLocators.DataTest("inventory-item");

    /// <summary>
    /// Gets the locator for the shopping cart link, typically in the page header.
    /// Identified using a 'data-test' attribute via <see cref="SmartLocators"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the shopping cart link.</value>
    public static By ShoppingCartLink => SmartLocators.DataTest("shopping-cart-link");

    /// <summary>
    /// Gets the locator for the shopping cart badge, which displays the number of items in the cart.
    /// This badge is typically a child element of the <see cref="ShoppingCartLink"/>.
    /// </summary>
    /// <value>A <see cref="By"/> object for the shopping cart badge.</value>
    public static By ShoppingCartBadge => By.CssSelector(".shopping_cart_link .shopping_cart_badge");

    /// <summary>
    /// Gets the locator for the name of the first inventory item displayed in the list.
    /// This is a more complex CSS selector targeting specific 'data-test' attributes within the list structure.
    /// </summary>
    /// <value>A <see cref="By"/> object for the name of the first inventory item.</value>
    public static By FirstInventoryItemName =>
        By.CssSelector("[data-test='inventory-list'] [data-test='inventory-item']:first-child [data-test='inventory-item-name']");

    /// <summary>
    /// Gets an array of <see cref="By"/> locators representing elements considered critical
    /// for the Inventory Page to be deemed loaded and operational.
    /// Used by <see cref="BasePage.EnsureCriticalElementsAreDisplayed"/>.
    /// </summary>
    /// <value>An array of <see cref="By"/> objects for critical page elements.</value>
    /// <remarks>
    /// This collection currently includes the <see cref="SortDropdown"/>, <see cref="InventoryContainer"/>,
    /// and <see cref="InventoryItemList"/>.
    /// </remarks>
    public static By[] InventoryPageElements { get; } = [
        SortDropdown,
        InventoryContainer,
        InventoryItemList
    ];
}
