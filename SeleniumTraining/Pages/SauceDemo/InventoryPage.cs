using System.Collections.ObjectModel;

namespace SeleniumTraining.Pages.SauceDemo;

public class InventoryPage : BasePage
{
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => InventoryPageMap.InventoryPageElements;

    public InventoryPage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider)
        : base(driver, loggerFactory, settingsProvider)
    {
        PageLogger.LogDebug("{PageName} instance fully created and validated (critical elements checked by BasePage).", PageName);
    }

    [AllureStep("Sort products by {selectorType} using option '{sortOption}'")]
    public InventoryPage SortProducts(SortByType selectorType, string sortOption)
    {
        var additionalProps = new Dictionary<string, object>
        {
            { "SortByType", selectorType.ToString() },
            { "SortOption", sortOption }
        };
        var timer = new PerformanceTimer(
            $"SortProducts_{PageName}",
            PageLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            additionalProps
        );
        bool success;

        try
        {
            PageLogger.LogInformation(
                "Attempting to sort products on {PageName} by {SortSelectorType} using option '{SortOptionValue}'.",
                PageName,
                selectorType.ToString(),
                sortOption
            );

            IWebElement sortContainer = Wait.WaitForElement(PageLogger, PageName, InventoryPageMap.SortDropdown);
            sortContainer.SelectDropDown(selectorType, sortOption, Wait, Driver, PageLogger, FrameworkSettings);

            success = true;

            string sortOptionDisplay = selectorType.GetDisplayName();
            PageLogger.LogInformation(
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
            PageLogger.LogError(
                ex,
                "Failed to sort products on {PageName} by {SortSelectorType} using option '{SortOptionValue}'.",
                PageName,
                selectorType.ToString(),
                sortOption
            );

            timer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: null);
            timer.Dispose();
            throw;
        }

        timer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: success ? 2000 : null);
        timer.Dispose();

        return this;
    }

    [AllureStep("Get selected sort option text")]
    public string GetSelectedSortText()
    {
        PageLogger.LogDebug("Getting selected sort option text from product sort container on {PageName}.", PageName);

        try
        {
            string selectedText = new SelectElement(Wait.WaitForElement(
                PageLogger,
                PageName,
                InventoryPageMap.SortDropdown
            )).SelectedOption.Text;

            PageLogger.LogInformation("Retrieved selected sort option text from {PageName}: '{SelectedSortText}'.", PageName, selectedText);

            return selectedText;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Failed to get selected sort option text from {PageName}.", PageName);
            throw;
        }
    }

    [AllureStep("Get selected sort option value")]
    public string GetSelectedSortValue()
    {
        PageLogger.LogDebug("Getting selected sort option value attribute from product sort container on {PageName}.", PageName);

        try
        {
            string selectedValue = new SelectElement(Wait.WaitForElement(
                PageLogger,
                PageName,
                InventoryPageMap.SortDropdown
            )).SelectedOption.GetAttribute("value") ?? string.Empty;

            PageLogger.LogInformation("Retrieved selected sort option value from {PageName}: '{SelectedSortValue}'.", PageName, selectedValue);

            return selectedValue;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Failed to get selected sort option value from {PageName}.", PageName);
            throw;
        }
    }

    [AllureStep("Get all inventory items on the page")]
    public IEnumerable<InventoryItemComponent> GetInventoryItems()
    {
        PageLogger.LogDebug("Attempting to find all inventory item elements on {PageName} using locator: {Locator}.", PageName, InventoryPageMap.InventoryItem);

        IEnumerable<IWebElement> itemElements = Wait.WaitForElements(PageLogger, PageName, InventoryPageMap.InventoryItem);

        if (!itemElements.Any())
        {
            PageLogger.LogWarning("No inventory items found on {PageName} using locator {Locator}.", PageName, InventoryPageMap.InventoryItem);
            return [];
        }

        PageLogger.LogInformation("Found {Count} inventory item elements on {PageName}. Creating components.", itemElements.Count(), PageName);

        return itemElements.Select(element =>
        {
            string? outerHtml = element.GetAttribute("outerHTML");
            string elementIdSnippet;

            if (string.IsNullOrEmpty(outerHtml))
            {
                elementIdSnippet = "[outerHTML not available or empty]";
                PageLogger.LogWarning("outerHTML for an inventory item element was null or empty.");
            }
            else
            {
                elementIdSnippet = outerHtml.Length <= 100 ? outerHtml : string.Concat(outerHtml.AsSpan(0, 100), "...");
            }

            PageLogger.LogTrace("Creating InventoryItemComponent for element snippet: {ElementIdSnippet}", elementIdSnippet);

            return new InventoryItemComponent(element, Driver, LoggerFactory, PageSettingsProvider);
        }).ToList();
    }
}
