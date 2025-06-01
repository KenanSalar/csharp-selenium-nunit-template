namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

public static class InventoryPageMap
{
    public static By SortDropdown => SmartLocators.DataTest("product-sort-container");
    public static By InventoryContainer => SmartLocators.DataTest("inventory-container");
    public static By InventoryItemList => SmartLocators.DataTest("inventory-list");
    public static By InventoryItem => SmartLocators.DataTest("inventory-item");

    public static By[] InventoryPageElements { get; } = [
        SortDropdown,
        InventoryContainer,
        InventoryItemList
    ];
}
