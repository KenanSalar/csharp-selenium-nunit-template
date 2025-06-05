using System.Collections.ObjectModel;

namespace SeleniumTraining.Pages.SauceDemo;

/// <summary>
/// Represents the Inventory Page of the saucedemo.com application, displaying product listings.
/// This page object provides functionalities for interacting with inventory items,
/// sorting products, and verifying page state.
/// </summary>
/// <remarks>
/// This page object inherits from <see cref="BasePage"/> to utilize common page functionalities,
/// including page-level element caching via <see cref="BasePage.FindElementOnPage(By)"/>.
/// It defines critical elements specific to the inventory page and uses custom expected conditions
/// for robust synchronization, particularly after sort operations and during initial page load verification
/// via <see cref="WaitForPageToBeFullyReady(int)"/>.
/// It also overrides methods from <see cref="BasePage"/> to define additional page readiness conditions,
/// interactions with which may benefit from the underlying page cache.
/// </remarks>
public class InventoryPage : BasePage
{
    /// <summary>
    /// Gets the collection of locators for critical elements that must be visible
    /// for the Inventory Page to be considered properly loaded.
    /// These include the inventory container, sort dropdown, and shopping cart link.
    /// </summary>
    /// <inheritdoc cref="BasePage.CriticalElementsToEnsureVisible" />
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => InventoryPageMap.InventoryPageElements;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryPage"/> class.
    /// It calls the base constructor and then invokes <see cref="WaitForPageToBeFullyReady(int)"/>
    /// to ensure the inventory page specific elements are loaded and ready.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance for browser interaction. Passed to base.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers. Passed to base.</param>
    /// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations. Passed to base.</param>
    /// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic. Passed to base.</param>
    /// <exception cref="WebDriverTimeoutException">Thrown by base constructor or <see cref="WaitForPageToBeFullyReady(int)"/> if page elements do not load within the timeout.</exception>
    /// <exception cref="ArgumentNullException">Thrown by the base constructor if any of the required service parameters are null.</exception>
    public InventoryPage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, IRetryService retryService)
        : base(driver, loggerFactory, settingsProvider, retryService)
    {
        PageLogger.LogDebug("{PageName} instance fully created and validated (critical elements checked by BasePage).", PageName);
        WaitForPageToBeFullyReady();
    }

    /// <summary>
    /// Sorts the products displayed on the inventory page based on the specified selector type and sort option.
    /// After performing the sort action, it waits for the product list to re-render and stabilize
    /// using the custom expected condition <see cref="CustomExpectedConditions.ListIsRenderedAndFirstItemIsReady"/>.
    /// </summary>
    /// <param name="selectorType">The <see cref="SortByType"/> criteria (e.g., Text, Value) used to select the sort option in the dropdown.</param>
    /// <param name="sortOption">The specific sort option to select (e.g., "Name (A to Z)", "lohi" for price low to high).</param>
    /// <returns>The current instance of the <see cref="InventoryPage"/>, allowing for fluent method chaining.</returns>
    /// <remarks>
    /// This method measures the performance of the sort operation. It utilizes extension methods
    /// like <c>WaitForElement</c> and <c>SelectDropDown</c> for interacting with the sort dropdown.
    /// Error handling is in place for timeouts or other exceptions during the sort process.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if the sort dropdown is not found, or if the list does not re-render as expected after sorting.</exception>
    /// <exception cref="Exception">Re-throws other unexpected exceptions that occur during the sorting process.</exception>
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
            sortContainer.Click();
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

    /// <summary>
    /// Retrieves the visible text of the currently selected option in the product sort dropdown.
    /// </summary>
    /// <returns>The text of the selected sort option.</returns>
    /// <exception cref="Exception">Re-throws exceptions if the sort dropdown or selected option cannot be found or interacted with.</exception>
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

    /// <summary>
    /// Retrieves the 'value' attribute of the currently selected option in the product sort dropdown.
    /// </summary>
    /// <returns>The 'value' attribute of the selected sort option, or an empty string if the attribute is not present or the option is not found.</returns>
    /// <exception cref="Exception">Re-throws exceptions if the sort dropdown or selected option cannot be found or interacted with.</exception>
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

    /// <summary>
    /// Retrieves all inventory items currently displayed on the page as a collection of <see cref="InventoryItemComponent"/> objects.
    /// It waits for at least a minimum number of items to be present before processing.
    /// </summary>
    /// <param name="minExpectedItems">The minimum number of inventory items expected to be present and rendered. Defaults to 1.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="InventoryItemComponent"/> representing the products.</returns>
    /// <remarks>
    /// This method measures its performance. It uses the custom expected condition
    /// <see cref="CustomExpectedConditions.ElementCountToBeGreaterThanOrEqual"/> to wait for items.
    /// Each found web element representing an item is then wrapped in an <see cref="InventoryItemComponent"/>
    /// for easier interaction. Logs detailed information about the process, including outer HTML snippets for tracing.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if the minimum number of expected items are not found within the timeout.</exception>
    /// <exception cref="Exception">Re-throws other unexpected exceptions during item retrieval or component creation.</exception>
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

    /// <summary>
    /// Waits for the Inventory Page to be fully loaded and ready for interaction.
    /// This method uses a composite wait (<see cref="CustomExpectedConditions.AllOf"/>)
    /// to ensure several key elements are present and visible. Elements are located
    /// using the page-level caching mechanism <see cref="BasePage.FindElementOnPage(By)"/>.
    /// <list type="bullet">
    ///   <item><description>The main inventory container (<see cref="InventoryPageMap.InventoryContainer"/>).</description></item>
    ///   <item><description>The shopping cart link (<see cref="InventoryPageMap.ShoppingCartLink"/>).</description></item>
    ///   <item><description>At least a minimum number of inventory items (<see cref="InventoryPageMap.InventoryItem"/>),
    ///   all of which must also be displayed.</description></item>
    /// </list>
    /// </summary>
    /// <param name="expectedMinItemCount">The minimum number of inventory items expected to be fully rendered. Defaults to 6.</param>
    /// <remarks>
    /// This method is called during the <see cref="InventoryPage"/> constructor to ensure page stability
    /// before any further interactions are attempted. It logs the process and re-throws a
    /// <see cref="WebDriverTimeoutException"/> if not all conditions are met within the configured wait time.
    /// The usage of <c>FindElementOnPage</c> within the conditions means that once an element is confirmed
    /// present and visible by a sub-condition, subsequent checks for it (if any within the same AllOf or later)
    /// may benefit from the cache.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if the page does not meet all readiness conditions within the timeout.</exception>
    [AllureStep("Wait for inventory page to be fully loaded and ready")]
    public void WaitForPageToBeFullyReady(int expectedMinItemCount = 6)
    {
        PageLogger.LogInformation("Waiting for Inventory Page to be fully ready (container, cart, items). Expecting at least {ItemCount} items.", expectedMinItemCount);
        try
        {
            bool isPageReady = Wait.Until(CustomExpectedConditions.AllOf(
                driver =>
                {
                    try
                    {
                        return FindElementOnPage(InventoryPageMap.InventoryContainer).Displayed;
                    }
                    catch
                    {
                        return false;
                    }
                },

                driver =>
                {
                    try
                    {
                        return FindElementOnPage(InventoryPageMap.ShoppingCartLink).Displayed;
                    }
                    catch
                    {
                        return false;
                    }
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

    /// <summary>
    /// Indicates whether this page defines additional base readiness conditions beyond
    /// what <see cref="BasePage"/> handles by default (document ready and critical elements).
    /// For InventoryPage, this is true as it checks for sort dropdown clickability.
    /// </summary>
    /// <returns><c>true</c> for InventoryPage.</returns>
    /// <inheritdoc cref="BasePage.DefinesAdditionalBaseReadinessConditions()" />
    protected override bool DefinesAdditionalBaseReadinessConditions()
    {
        return true;
    }

    /// <summary>
    /// Provides additional custom wait conditions specific to the Inventory Page that are
    /// checked during the base page initialization sequence if <see cref="DefinesAdditionalBaseReadinessConditions"/> is true.
    /// This implementation specifically checks if the sort dropdown is clickable, using the
    /// page-level caching mechanism <see cref="BasePage.FindElementOnPage(By)"/> to locate the dropdown.
    /// </summary>
    /// <returns>An enumerable containing a function that checks if the sort dropdown is displayed and enabled.</returns>
    /// <inheritdoc cref="BasePage.GetAdditionalBaseReadinessConditions()" />
    protected override IEnumerable<Func<IWebDriver, bool>> GetAdditionalBaseReadinessConditions()
    {
        yield return driver =>
        {
            try
            {
                IWebElement sortDropdown = FindElementOnPage(InventoryPageMap.SortDropdown);
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

    /// <summary>
    /// Gets the current number of items displayed in the shopping cart badge.
    /// </summary>
    /// <returns>The number of items in the cart, or 0 if the badge is not displayed or empty.</returns>
    [AllureStep("Get shopping cart badge count")]
    public int GetShoppingCartBadgeCount()
    {
        PageLogger.LogDebug("Attempting to get shopping cart badge count.");
        try
        {
            IWebElement badge = FindElementOnPage(InventoryPageMap.ShoppingCartBadge);
            if (badge.Displayed)
            {
                string countText = badge.Text;
                if (int.TryParse(countText, out int count))
                {
                    PageLogger.LogInformation("Shopping cart badge count: {Count}", count);

                    return count;
                }
                PageLogger.LogWarning("Could not parse cart badge text '{BadgeText}' to an integer.", countText);
            }
            else
            {
                PageLogger.LogInformation("Shopping cart badge is not displayed, assuming 0 items.");
            }
        }
        catch (NoSuchElementException)
        {
            PageLogger.LogInformation("Shopping cart badge element not found, assuming 0 items.");
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Error getting shopping cart badge count.");
        }

        return 0;
    }

    [AllureStep("Navigate to Shopping Cart")]
    public ShoppingCartPage ClickShoppingCartLink()
    {
        PageLogger.LogInformation("Clicking shopping cart link.");
        FindElementOnPage(InventoryPageMap.ShoppingCartLink).Click();
        return new ShoppingCartPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }
}
