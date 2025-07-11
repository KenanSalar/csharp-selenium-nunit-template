namespace SeleniumTraining.Utils.Waits;

/// <summary>
/// Provides a collection of custom <see cref="Func{IWebDriver, TResult}"/> delegates
/// for use as conditions with <see cref="WebDriverWait.Until{TResult}(Func{IWebDriver, TResult})"/>.
/// These conditions extend the standard Selenium <see cref="ExpectedConditions"/>
/// to allow waiting for more specific or complex application states.
/// </summary>
/// <remarks>
/// Each method in this class returns a function that can be passed to <c>WebDriverWait.Until()</c>.
/// These functions typically check for a specific state of the WebDriver or web elements
/// and return a "truthy" value (e.g., true, a non-null IWebElement) when the condition is met,
/// or a "falsy" value (e.g., false, null) otherwise, prompting the wait to continue.
/// </remarks>
public static class CustomExpectedConditions
{
    /// <summary>
    /// Waits for an element to have a specific attribute with a specific value.
    /// </summary>
    /// <param name="locator">The locator used to find the element.</param>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <param name="expectedValue">The expected value of the attribute.</param>
    /// <returns>A function that returns the IWebElement once the condition is met, or null otherwise.</returns>
    public static Func<IWebDriver, IWebElement?> ElementAttributeToBe(By locator, string attributeName, string expectedValue)
    {
        return (driver) =>
        {
            try
            {
                IWebElement element = driver.FindElement(locator);
                string? actualValue = element.GetAttribute(attributeName);

                return element.Displayed && actualValue?.Equals(expectedValue, StringComparison.OrdinalIgnoreCase) == true
                    ? element
                    : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        };
    }

    /// <summary>
    /// Waits for a specific number of elements to be found by the locator.
    /// </summary>
    /// <param name="locator">The locator used to find the elements.</param>
    /// <param name="expectedCount">The expected number of elements.</param>
    /// <returns>A function that returns the collection of IWebElements if the count matches, or null otherwise.</returns>
    public static Func<IWebDriver, IEnumerable<IWebElement>?> ElementCountToBe(By locator, int expectedCount)
    {
        return (driver) =>
        {
            try
            {
                ReadOnlyCollection<IWebElement> elements = driver.FindElements(locator);

                return elements.Count == expectedCount
                    ? elements
                    : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        };
    }

    /// <summary>
    /// Waits for at least a minimum number of elements to be found by the locator.
    /// </summary>
    /// <param name="locator">The locator used to find the elements.</param>
    /// <param name="minCount">The minimum number of elements expected. This parameter is documented inline in your XML documentation but should include specific description details about it being an integer value specifying the maximum retry attempts or similar context depending on its use case.</param>
    /// <param name="logger"><see langword="ILogger"/> instance for tracking wait operations and potential errors. The logger helps monitor the execution process, including tracing element counts found during each attempt and logging any exceptions encountered. It can also be used to implement custom logging behavior or record metrics about wait times if required.</param>
    /// <returns>A function that returns the collection of IWebElements if the count is met or exceeded, or null otherwise.</returns>
    public static Func<IWebDriver, IEnumerable<IWebElement>?> ElementCountToBeGreaterThanOrEqual(By locator, int minCount, ILogger? logger = null)
    {
        return (driver) =>
        {
            try
            {
                ReadOnlyCollection<IWebElement> elements = driver.FindElements(locator);
                if (elements.Count >= minCount)
                {
                    logger?.LogTrace(
                        "CustomCondition: ElementCountToBeGreaterThanOrEqual - Found {ActualCount} elements (>= min {Min}), condition met for locator: {Locator}",
                        elements.Count,
                        minCount,
                        locator
                    );

                    return elements;
                }

                logger?.LogTrace(
                    "CustomCondition: ElementCountToBeGreaterThanOrEqual - Found {ActualCount} elements (< min {Min}), retrying for locator: {Locator}",
                    elements.Count,
                    minCount,
                    locator
                );

                return null;
            }
            catch (StaleElementReferenceException ex)
            {
                logger?.LogTrace(
                    ex,
                    "CustomCondition: ElementCountToBeGreaterThanOrEqual - StaleElementReferenceException encountered for locator: {Locator}. Retrying find.",
                    locator
                );

                return null;
            }
        };
    }

    /// <summary>
    /// Waits for the text of an element to change from a specified initial value.
    /// </summary>
    /// <param name="locator">The locator used to find the element.</param>
    /// <param name="initialText">The text value the element should initially not be equal to (or different from).</param>
    /// <returns>A function that returns true if the text has changed, false otherwise.</returns>
    public static Func<IWebDriver, bool> ElementTextToChangeFrom(By locator, string initialText)
    {
        return (driver) =>
        {
            try
            {
                IWebElement element = driver.FindElement(locator);
                string currentText = element.Text;

                return element.Displayed && !currentText.Equals(initialText, StringComparison.Ordinal);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// Waits for an element to become stale (i.e., removed from the DOM or no longer attached).
    /// This is an inversion of waiting for presence.
    /// </summary>
    /// <param name="element">The element expected to become stale.</param>
    /// <returns>A function that returns true if the element is stale, false otherwise.</returns>
    public static Func<IWebDriver, bool> ElementToBeStale(IWebElement element)
    {
        return (driver) =>
        {
            try
            {
                _ = driver;
                _ = element.Displayed;
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        };
    }

    /// <summary>
    /// Waits for a JavaScript expression to evaluate to true.
    /// </summary>
    /// <param name="script">The JavaScript to execute. It should return a boolean.</param>
    /// <returns>A function that returns true if the script evaluates to true, false otherwise.</returns>
    public static Func<IWebDriver, bool> JavaScriptToReturnTrue(string script)
    {
        return (driver) =>
        {
            try
            {
                if (driver is not IJavaScriptExecutor jsExecutor)
                    return false;

                object? result = jsExecutor.ExecuteScript(script);

                return result is bool boolResult && boolResult;
            }
            catch (Exception)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that the document.readyState is 'complete'.
    /// This indicates that the page has fully loaded all its resources.
    /// </summary>
    /// <returns>
    /// A <see cref="Func{IWebDriver, Boolean}"/> that returns <see langword="true"/>
    /// when document.readyState is 'complete', or <see langword="false"/> otherwise.
    /// </returns>
    public static Func<IWebDriver, bool> DocumentIsReady()
    {
        return (driver) =>
        {
            try
            {
                if (driver is IJavaScriptExecutor jsExecutor)
                {
                    object? readyState = jsExecutor.ExecuteScript("return document.readyState;");
                    return readyState?.ToString()?.Equals("complete", StringComparison.OrdinalIgnoreCase) ?? false;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// Waits for a list of items to be populated and the first item in that list
    /// to be displayed with non-empty text.
    /// This is useful for verifying that a list has re-rendered after an action like sorting or filtering.
    /// </summary>
    /// <param name="allItemsContainerLocator">Locator for the container of all items, or individual items themselves if flat.</param>
    /// <param name="firstItemSpecificElementLocator">Locator for a specific element within the first item (e.g., its name or title) whose text content should be checked.</param>
    /// <param name="logger">Logger for detailed trace messages during the wait.</param>
    /// <returns>A function that returns true if the condition is met, false otherwise.</returns>
    public static Func<IWebDriver, bool> ListIsRenderedAndFirstItemIsReady(By allItemsContainerLocator, By firstItemSpecificElementLocator, ILogger logger)
    {
        return (driver) =>
        {
            try
            {
                ReadOnlyCollection<IWebElement> allItems = driver.FindElements(allItemsContainerLocator);
                if (allItems.Count == 0)
                {
                    logger.LogTrace("CustomCondition (ListIsRenderedAndFirstItemIsReady): No items found yet with locator '{AllItemsLocator}'.", allItemsContainerLocator);
                    return false;
                }

                IWebElement firstItemSpecificElement = driver.FindElement(firstItemSpecificElementLocator);
                bool isDisplayedAndNotEmpty = firstItemSpecificElement.Displayed && !string.IsNullOrEmpty(firstItemSpecificElement.Text);

                if (isDisplayedAndNotEmpty)
                {
                    logger.LogTrace(
                        "CustomCondition (ListIsRenderedAndFirstItemIsReady): First item's specific element ('{SpecificElementLocator}') is displayed and has text '{Text}'. Condition met.",
                        firstItemSpecificElementLocator,
                        firstItemSpecificElement.Text
                    );
                }
                else
                {
                    logger.LogTrace(
                        "CustomCondition (ListIsRenderedAndFirstItemIsReady): First item's specific element ('{SpecificElementLocator}') not yet displayed or text is empty. Current text: '{Text}', Displayed: {DisplayedState}",
                        firstItemSpecificElementLocator,
                        firstItemSpecificElement.Text,
                        firstItemSpecificElement.Displayed
                    );
                }

                return isDisplayedAndNotEmpty;
            }
            catch (NoSuchElementException ex)
            {
                logger.LogTrace(ex, "CustomCondition (ListIsRenderedAndFirstItemIsReady): Element not found. AllItemsLocator: '{AllItemsLocator}', FirstItemSpecificLocator: '{SpecificLocator}'.",
                    allItemsContainerLocator, firstItemSpecificElementLocator);
                return false;
            }
            catch (StaleElementReferenceException ex)
            {
                logger.LogTrace(ex, "CustomCondition (ListIsRenderedAndFirstItemIsReady): Stale element reference encountered, list likely re-rendering. AllItemsLocator: '{AllItemsLocator}', FirstItemSpecificLocator: '{SpecificLocator}'.",
                    allItemsContainerLocator, firstItemSpecificElementLocator);
                return false;
            }
        };
    }

    /// <summary>
    /// Waits until all provided individual wait conditions are met.
    /// Each condition is a Func<IWebDriver, bool> that should return true when satisfied.
    /// </summary>
    /// <param name="driver">The IWebDriver instance (passed by WebDriverWait.Until).</param>
    /// <param name="logger">Optional logger for diagnosing issues within sub-conditions.</param>
    /// <param name="conditions">An array of functions, each representing a wait condition.</param>
    /// <returns>True if all conditions are met, false otherwise.</returns>
    private static bool CheckAllConditions(IWebDriver driver, ILogger? logger, Func<IWebDriver, bool>[] conditions)
    {
        foreach (Func<IWebDriver, bool> condition in conditions)
        {
            try
            {
                if (!condition(driver))
                {
                    logger?.LogTrace("A sub-condition in AllOf evaluated to false.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "A sub-condition within AllOf threw an exception. Treating as false for this attempt.");
                return false;
            }
        }
        return true;
    }


    /// <summary>
    /// Waits until all provided individual wait conditions are met.
    /// Each condition is a Func<IWebDriver, bool> that should return true when satisfied.
    /// </summary>
    /// <param name="logger">Optional logger for diagnosing issues within sub-conditions.</param>
    /// <param name="conditions">An array of functions, each representing a wait condition.</param>
    /// <returns>A function for WebDriverWait.Until that returns true if all conditions are met, false otherwise.</returns>
    public static Func<IWebDriver, bool> AllOf(ILogger? logger, params Func<IWebDriver, bool>[] conditions)
    {
        return (driver) => CheckAllConditions(driver, logger, conditions);
    }

    public static Func<IWebDriver, bool> AllOf(params Func<IWebDriver, bool>[] conditions)
    {
        return (driver) => CheckAllConditions(driver, null, conditions);
    }


    /// <summary>
    /// Waits until any one of the provided individual wait conditions is met.
    /// Each condition is a Func<IWebDriver, bool> that should return true when satisfied.
    /// </summary>
    /// <param name="driver">The IWebDriver instance (passed by WebDriverWait.Until).</param>
    /// <param name="logger">Optional logger for diagnosing issues within sub-conditions.</param>
    /// <param name="conditions">An array of functions, each representing a wait condition.</param>
    /// <returns>True if any condition is met, false otherwise.</returns>
    private static bool CheckAnyConditions(IWebDriver driver, ILogger? logger, Func<IWebDriver, bool>[] conditions)
    {
        foreach (Func<IWebDriver, bool> condition in conditions)
        {
            try
            {
                if (condition(driver))
                {
                    logger?.LogTrace("A sub-condition in AnyOf evaluated to true.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "A sub-condition within AnyOf threw an exception. This sub-condition is treated as false; checking next conditions.");
            }
        }
        return false;
    }

    /// <summary>
    /// Waits until any one of the provided individual wait conditions is met.
    /// Each condition is a Func<IWebDriver, bool> that should return true when satisfied.
    /// </summary>
    /// <param name="logger">Optional logger for diagnosing issues within sub-conditions.</param>
    /// <param name="conditions">An array of functions, each representing a wait condition.</param>
    /// <returns>A function for WebDriverWait.Until that returns true if any condition is met, false otherwise.</returns>
    public static Func<IWebDriver, bool> AnyOf(ILogger? logger, params Func<IWebDriver, bool>[] conditions)
    {
        return (driver) => CheckAnyConditions(driver, logger, conditions);
    }

    public static Func<IWebDriver, bool> AnyOf(params Func<IWebDriver, bool>[] conditions)
    {
        return (driver) => CheckAnyConditions(driver, null, conditions);
    }
}
