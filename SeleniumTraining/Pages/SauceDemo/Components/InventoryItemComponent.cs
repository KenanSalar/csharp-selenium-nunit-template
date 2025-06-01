namespace SeleniumTraining.Pages.SauceDemo.Components;

public class InventoryItemComponent : BasePageComponent
{
    public InventoryItemComponent(IWebElement rootElement, IWebDriver driver, ILoggerFactory loggerFactory)
        : base(rootElement, driver, loggerFactory)
    {
        string outerHtml = RootElement.GetAttribute("outerHTML") ?? string.Empty;
        ComponentLogger.LogDebug(
            "InventoryItemComponent initialized for element: {RootElementId}",
            outerHtml[..Math.Min(100, outerHtml.Length)]
        );
    }
    public IWebElement ItemImage
    {
        get
        {
            ComponentLogger.LogTrace(
                "Attempting to find ItemImage using locator: {Locator}",
                InventoryItemComponentMap.ItemImage
            );
            return FindElement(InventoryItemComponentMap.ItemImage);
        }
    }

    public string ItemName
    {
        get
        {
            ComponentLogger.LogTrace("Attempting to find ItemName element using locator: {Locator}", InventoryItemComponentMap.ItemName);
            string name = FindElement(InventoryItemComponentMap.ItemName).Text;
            ComponentLogger.LogDebug("Retrieved ItemName: {Name}", name);

            return name;
        }
    }

    public string ItemDescription
    {
        get
        {
            ComponentLogger.LogTrace("Attempting to find ItemDescription element using locator: {Locator}", InventoryItemComponentMap.ItemDescription);
            string description = FindElement(InventoryItemComponentMap.ItemDescription).Text;
            ComponentLogger.LogDebug("Retrieved ItemDescription: {Description}", description);
            return description;
        }
    }

    public string ItemPrice
    {
        get
        {
            ComponentLogger.LogTrace("Attempting to find ItemPrice element using locator: {Locator}", InventoryItemComponentMap.ItemPrice);
            string price = FindElement(InventoryItemComponentMap.ItemPrice).Text;
            ComponentLogger.LogDebug("Retrieved ItemPrice: {Price}", price);
            return price;
        }
    }

    public void ClickActionButton()
    {
        ComponentLogger.LogTrace("Attempting to find ActionButton element using locator: {Locator}", InventoryItemComponentMap.ActionButton);
        IWebElement button = FindElement(InventoryItemComponentMap.ActionButton);
        string buttonText = button.Text;
        ComponentLogger.LogInformation("Clicking action button with text '{ButtonText}' for item '{ItemName}'.", buttonText, ItemName); // Using ItemName property for context
        button.Click();
    }

    public string GetActionButtonText()
    {
        ComponentLogger.LogTrace("Attempting to find ActionButton element to get text, using locator: {Locator}", InventoryItemComponentMap.ActionButton);
        string text = FindElement(InventoryItemComponentMap.ActionButton).Text;
        ComponentLogger.LogDebug("Retrieved ActionButton text: {Text}", text);
        return text;
    }

}
