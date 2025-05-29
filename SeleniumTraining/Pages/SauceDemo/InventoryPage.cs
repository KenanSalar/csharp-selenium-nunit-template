using System.Collections.ObjectModel;

namespace SeleniumTraining.Pages.SauceDemo;

public class InventoryPage : BasePage
{
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => InventoryPageMap.InventoryPageElements;

    public InventoryPage(IWebDriver driver, ILoggerFactory loggerFactory) : base(driver, loggerFactory)
    {
        Logger.LogDebug("{PageName} instance fully created and validated (critical elements checked by BasePage).", PageName);
    }

    [AllureStep("Sort products by {selectorType} using option '{sortOption}'")]
    public InventoryPage SortProducts(SortByType selectorType, string sortOption)
    {
        Logger.LogInformation(
            "Attempting to sort products on {PageName} by {SortSelectorType} using option '{SortOptionValue}'.",
            PageName,
            selectorType.ToString(),
            sortOption
        );

        try
        {
            IWebElement sortContainer = Wait.WaitForElement(Logger, PageName, InventoryPageMap.SortDropdown);
            sortContainer.SelectDropDown(selectorType, sortOption, Wait);

            string sortOptionDisplay = selectorType.GetDisplayName();
            Logger.LogInformation(
                "Products on {PageName} successfully sorted by '{SortOptionDisplay}' (option value: '{SortOptionValue}').",
                PageName,
                string.IsNullOrEmpty(sortOptionDisplay)
                    ? sortOption
                    : sortOptionDisplay,
                sortOption
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Failed to sort products on {PageName} by {SortSelectorType} using option '{SortOptionValue}'.",
                PageName,
                selectorType.ToString(),
                sortOption
            );
            throw;
        }

        return this;
    }

    [AllureStep("Get selected sort option text")]
    public string GetSelectedSortText()
    {
        Logger.LogDebug("Getting selected sort option text from product sort container on {PageName}.", PageName);

        try
        {
            string selectedText = new SelectElement(Wait.WaitForElement(
                Logger,
                PageName,
                InventoryPageMap.SortDropdown
            )).SelectedOption.Text;

            Logger.LogInformation("Retrieved selected sort option text from {PageName}: '{SelectedSortText}'.", PageName, selectedText);

            return selectedText;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get selected sort option text from {PageName}.", PageName);
            throw;
        }
    }

    [AllureStep("Get selected sort option value")]
    public string GetSelectedSortValue()
    {
        Logger.LogDebug("Getting selected sort option value attribute from product sort container on {PageName}.", PageName);

        try
        {
            string selectedValue = new SelectElement(Wait.WaitForElement(
                Logger,
                PageName,
                InventoryPageMap.SortDropdown
            )).SelectedOption.GetAttribute("value") ?? string.Empty;

            Logger.LogInformation("Retrieved selected sort option value from {PageName}: '{SelectedSortValue}'.", PageName, selectedValue);

            return selectedValue;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get selected sort option value from {PageName}.", PageName);
            throw;
        }
    }
}
