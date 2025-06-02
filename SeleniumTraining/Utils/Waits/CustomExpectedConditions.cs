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
public class CustomExpectedConditions
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

                return element.Displayed && actualValue != null && actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase)
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
    /// <param name="minCount">The minimum number of elements expected.</param>
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
    public static Func<IWebDriver, bool> ListIsRenderedAndFirstItemIsReady(By allItemsContainerLocator,By firstItemSpecificElementLocator,ILogger logger)
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
}
