using System.Collections.ObjectModel;

namespace SeleniumTraining.Pages.SauceDemo;

public class InventoryPage : BasePage
{
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => InventoryPageMap.InventoryPageElements;

    public InventoryPage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, IRetryService retryService)
        : base(driver, loggerFactory, settingsProvider, retryService)
    {
        PageLogger.LogDebug("{PageName} instance fully created and validated (critical elements checked by BasePage).", PageName);
        WaitForPageToBeFullyReady();
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

        bool success = false;

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

            string sortOptionDisplay = selectorType.GetDisplayName();
            PageLogger.LogInformation(
                "Sort action performed on {PageName} using '{SortOptionDisplay}' (value: '{SortOptionValue}'). Waiting for sort to apply.",
                PageName,
                string.IsNullOrEmpty(sortOptionDisplay) ? sortOption : sortOptionDisplay,
                sortOption
            );

            PageLogger.LogDebug("Waiting for inventory items to re-render after sort on {PageName} using CustomExpectedCondition.", PageName);

            bool sortAppliedSuccessfully = Wait.Until(
                CustomExpectedConditions.ListIsRenderedAndFirstItemIsReady(
                    InventoryPageMap.InventoryItem,
                    InventoryPageMap.FirstInventoryItemName,
                    PageLogger
                )
            );

            if (sortAppliedSuccessfully)
            {
                PageLogger.LogInformation("Inventory items re-rendered and sort confirmed as applied on {PageName}.", PageName);
                success = true;
            }
            else
            {
                PageLogger.LogWarning("Sort application verification did not confirm true within timeout on {PageName} (Wait.Until returned false).", PageName);
                success = false;
            }
        }
        catch (WebDriverTimeoutException ex)
        {
            PageLogger.LogWarning(ex, "Timeout waiting for inventory items to re-render and sort to apply on {PageName}. The sort might not have visually completed or failed to apply as expected.", PageName);
            success = false;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(
                ex,
                "Failed to sort products on {PageName} by {SortSelectorType} using option '{SortOptionValue}'. Error: {ErrorMessage}",
                PageName,
                selectorType.ToString(),
                sortOption,
                ex.Message
            );
            success = false;
            throw;
        }
        finally
        {
            long expectedDuration = 2000;
            timer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: success ? expectedDuration : null);
            timer.Dispose();
        }

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
    public IEnumerable<InventoryItemComponent> GetInventoryItems(int minExpectedItems = 1)
    {
        PageLogger.LogDebug(
            "Attempting to find at least {MinCount} inventory item elements on {PageName} using locator: {Locator}.",
            minExpectedItems,
            PageName,
            InventoryPageMap.InventoryItem
        );

        var getItemsTimer = new PerformanceTimer(
            $"GetInventoryItems_{PageName}",
            PageLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            new Dictionary<string, object>
            {
                { "PageType", PageName },
                { "MinExpectedItems", minExpectedItems }
            }
        );

        IEnumerable<IWebElement> itemElements;
        try
        {
            itemElements = Wait.Until(
                CustomExpectedConditions.ElementCountToBeGreaterThanOrEqual(InventoryPageMap.InventoryItem, minExpectedItems)
            );

            if (itemElements == null || (minExpectedItems > 0 && !itemElements.Any()))
            {
                PageLogger.LogWarning(
                    "Condition for at least {MinCount} items met, but returned collection is null or empty. This should ideally be caught by timeout if MinCount > 0. Locator: {Locator}",
                    minExpectedItems,
                    InventoryPageMap.InventoryItem
                );
                getItemsTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: null);
                return [];
            }

            PageLogger.LogInformation(
                "Found {Count} inventory item elements on {PageName} (met minimum of {MinCount}). Creating components.",
                itemElements.Count(),
                PageName,
                minExpectedItems
            );
        }
        catch (WebDriverTimeoutException ex)
        {
            PageLogger.LogError(
                ex,
                "Timeout waiting for at least {MinCount} inventory items on {PageName}. Locator: {Locator}. Timeout: {TimeoutSeconds}s.",
                minExpectedItems,
                PageName,
                InventoryPageMap.InventoryItem,
                Wait.Timeout.TotalSeconds
            );
            getItemsTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: null);
            throw;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "An unexpected error occurred while trying to get inventory items on {PageName}.", PageName);
            getItemsTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: null);
            throw;
        }

        var components = itemElements.Select(element =>
        {
            string? outerHtml = element.GetAttribute("outerHTML");
            string elementIdSnippet;

            if (string.IsNullOrEmpty(outerHtml))
            {
                elementIdSnippet = "[outerHTML not available or empty]";
                PageLogger.LogWarning("outerHTML for an inventory item element was null or empty during component creation.");
            }
            else
            {
                elementIdSnippet = outerHtml.Length <= 100
                    ? outerHtml
                    : string.Concat(outerHtml.AsSpan(0, 100), "...");
            }

            PageLogger.LogTrace("Creating InventoryItemComponent for element snippet: {ElementIdSnippet}", elementIdSnippet);

            return new InventoryItemComponent(element, Driver, LoggerFactory, PageSettingsProvider, Retry);
        }).ToList();

        getItemsTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 1000);
        return components;
    }

    [AllureStep("Wait for inventory page to be fully loaded and ready")]
    public void WaitForPageToBeFullyReady(int expectedMinItemCount = 6)
    {
        PageLogger.LogInformation("Waiting for Inventory Page to be fully ready (container, cart, items). Expecting at least {ItemCount} items.", expectedMinItemCount);
        try
        {
            bool isPageReady = Wait.Until(CustomExpectedConditions.AllOf(
                driver =>
                {
                    try { return driver.FindElement(InventoryPageMap.InventoryContainer).Displayed; }
                    catch { return false; }
                },

                driver =>
                {
                    try { return driver.FindElement(InventoryPageMap.ShoppingCartLink).Displayed; }
                    catch { return false; }
                },

                driver =>
                {
                    Func<IWebDriver, IEnumerable<IWebElement>?> itemsFunc = CustomExpectedConditions.ElementCountToBeGreaterThanOrEqual(InventoryPageMap.InventoryItem, expectedMinItemCount);
                    IEnumerable<IWebElement>? items = itemsFunc(driver);

                    return items != null && items.Any() && items.All(item =>
                        {
                            try
                            {
                                return item.Displayed;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    );
                }
            ));

            if (isPageReady)
            {
                PageLogger.LogInformation("Inventory Page is fully loaded and ready with expected elements.");
            }
        }
        catch (WebDriverTimeoutException ex)
        {
            PageLogger.LogError(ex, "Timeout waiting for Inventory Page to be fully ready. One or more conditions were not met.");
            throw;
        }
    }

    protected override bool DefinesAdditionalBaseReadinessConditions()
    {
        return true;
    }

    protected override IEnumerable<Func<IWebDriver, bool>> GetAdditionalBaseReadinessConditions()
    {
        yield return driver =>
        {
            try
            {
                IWebElement sortDropdown = driver.FindElement(InventoryPageMap.SortDropdown);
                bool clickable = sortDropdown.Displayed && sortDropdown.Enabled;
                PageLogger.LogTrace("AdditionalBaseCondition (InventoryPage) - SortDropdown Clickable: {IsClickable}", clickable);
                return clickable;
            }
            catch (Exception ex)
            {
                PageLogger.LogTrace(ex, "AdditionalBaseCondition (InventoryPage) - SortDropdown check exception.");
                return false;
            }
        };
    }
}
