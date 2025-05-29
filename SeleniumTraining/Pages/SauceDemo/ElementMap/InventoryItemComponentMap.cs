namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

public static class InventoryItemComponentMap
{
    public static By ItemImage => By.CssSelector("img.inventory_item_img");
    public static By ItemName => By.CssSelector(".inventory_item_name");
    public static By ItemDescription => By.CssSelector(".inventory_item_desc");
    public static By ItemPrice => By.CssSelector(".inventory_item_price");
    public static By AddToCartButton => By.CssSelector("button.btn_inventory");
}
