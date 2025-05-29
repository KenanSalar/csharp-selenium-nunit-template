namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

public static class InventoryPageMap
{
    public static By SortDropdown => By.CssSelector(".product_sort_container");
    public static By InventoryContainer => By.Id("inventory_container");
    public static By InventoryItem => By.ClassName("inventory_item");
    public static By InventoryItemList => By.ClassName("inventory_list");

    public static By[] InventoryPageElements { get; } = [
        SortDropdown,
        InventoryContainer,
        InventoryItemList
    ];
}
