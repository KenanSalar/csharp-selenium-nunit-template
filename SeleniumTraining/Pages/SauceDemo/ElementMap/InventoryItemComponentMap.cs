namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

public static class InventoryItemComponentMap
{
    public static By ItemImage => By.CssSelector("img.inventory_item_img");
    public static By ItemName => SmartLocators.DataTest("inventory-item-name");
    public static By ItemDescription => SmartLocators.DataTest("inventory-item-desc");
    public static By ItemPrice => SmartLocators.DataTest("inventory-item-price");
    public static By ActionButton => By.CssSelector("button.btn_inventory");
}
