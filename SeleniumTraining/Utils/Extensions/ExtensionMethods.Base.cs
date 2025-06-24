using System.Collections.ObjectModel;
using System.Reflection;

namespace SeleniumTraining.Utils.Extensions;

/// <summary>
/// Provides a collection of static extension methods designed to enhance and simplify common operations
/// for various types used within the Selenium test automation framework.
/// This includes extensions for Selenium WebDriver types (like <see cref="IWebDriver"/>, <see cref="IWebElement"/>, <see cref="WebDriverWait"/>, <see cref="By"/>),
/// .NET base types (like <see cref="Enum"/>), and other framework-specific utilities.
/// </summary>
/// <remarks>
/// This class is implemented as a <see langword="partial"/> class, allowing its extension methods
/// to be organized into separate files based on the type they extend or the functionality they provide.
/// This modular approach improves readability and maintainability of the utility code.
/// Current parts include:
/// <list type="bullet">
///   <item>
///     <term>ExtensionMethods.Base.cs:</term>
///     <description>Contains fundamental extensions for Selenium WebDriver and WebDriverWait, such as waiting for page titles,
///     finding elements with explicit waits, ensuring element visibility, and helper methods for Enum display names.</description>
///   </item>
///   <item>
///     <term>ExtensionMethods.Dropdown.cs:</term>
///     <description>Provides specialized extensions for interacting with HTML dropdown (select) elements,
///     including selecting options by text or value and handling multi-selects.</description>
///   </item>
///   <item>
///     <term>ExtensionMethods.Login.cs:</term>
///     <description>Offers focused extensions for common login-related actions, such as entering text into
///     username and password input fields identified by <see cref="By"/> locators.</description>
///   </item>
///   <item>
///     <term>WebElementHighlightingExtensions.cs (Conceptually related though in its own file):</term>
///     <description>Provides extensions for visually highlighting web elements during test execution for debugging.</description>
///   </item>
///   <item>
///     <term>WebDriverExtensions.cs (Conceptually related though in its own file):</term>
///     <description>Offers extensions for safer WebDriver operations, like <c>QuitSafely</c>.</description>
///   </item>
/// </list>
/// These extensions aim to make test scripts more fluent, reduce boilerplate code, and promote consistent
/// interaction patterns with web elements and framework components. Many methods also integrate with Allure reporting
/// by using the <see cref="AllureStepAttribute"/> to provide clearer steps in test execution reports.
/// </remarks>
public static partial class ExtensionMethods
{
    /// <summary>
    /// Waits for the web page's title to match the specified expected title within the WebDriverWait's timeout period.
    /// </summary>
    /// <param name="wait">The <see cref="WebDriverWait"/> instance to extend.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance associated with the page.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance for logging the operation's progress and outcome.</param>
    /// <param name="pageName">A descriptive name of the page being checked, used for logging.</param>
    /// <param name="expectedTitle">The exact string the page title is expected to be.</param>
    /// <remarks>
    /// Logs the attempt, success, or failure (including timeout or mismatch).
    /// If the title does not match within the timeout, a <see cref="WebDriverTimeoutException"/> is thrown.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if the page title does not match <paramref name="expectedTitle"/> within the timeout.</exception>
    [AllureStep("Wait for page title to be {expectedTitle}")]
    public static void WaitForPageTitle(this WebDriverWait wait, IWebDriver driver, ILogger logger, string pageName, string expectedTitle)
    {
        logger.LogDebug(
            "Waiting for page title. Page: {PageName}, ExpectedTitle: {ExpectedTitle}, Timeout: {TimeoutSeconds}s",
            pageName,
            expectedTitle,
            wait.Timeout.TotalSeconds
        );

        try
        {
            bool titleMatches = wait.Until(driver => driver.Title == expectedTitle);

            if (titleMatches)
            {
                logger.LogInformation(
                    "Page title confirmed for {PageName}. Expected: '{ExpectedTitle}', Actual: '{ActualTitle}'.",
                    pageName,
                    expectedTitle,
                    driver.Title
                );
            }
            else
            {
                logger.LogWarning(
                    "Page title check for {PageName} returned false but did not time out. Expected: '{ExpectedTitle}', Actual: '{ActualTitle}'.",
                    pageName,
                    expectedTitle,
                    driver.Title
                );
            }
        }
        catch (WebDriverTimeoutException ex)
        {
            logger.LogError(
                ex,
                "Timeout waiting for page title for {PageName}. Expected: '{ExpectedTitle}', Actual: '{ActualTitle}', Timeout: {TimeoutSeconds}s.",
                pageName,
                expectedTitle,
                driver.Title,
                wait.Timeout.TotalSeconds
            );
            throw;
        }
    }

    /// <summary>
    /// Waits for an element to exist in the DOM and returns it.
    /// </summary>
    /// <param name="wait">The <see cref="WebDriverWait"/> instance to extend.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance for logging.</param>
    /// <param name="pageName">A descriptive name of the page where the element is expected, used for logging.</param>
    /// <param name="locator">The <see cref="By"/> locator used to find the element.</param>
    /// <returns>The located <see cref="IWebElement"/> once it exists.</returns>
    /// <remarks>
    /// This method uses <see cref="ExpectedConditions.ElementExists(By)"/>.
    /// If the element does not exist within the timeout, a <see cref="WebDriverTimeoutException"/> is thrown.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if the element specified by <paramref name="locator"/> does not exist in the DOM within the timeout.</exception>
    [AllureStep("Wait and find element: {locator}")]
    public static IWebElement WaitForElement(this WebDriverWait wait, ILogger logger, string pageName, By locator)
    {
        logger.LogDebug(
            "Waiting for element to exist on {PageName}. Locator: {ElementLocator}, Timeout: {TimeoutSeconds}s",
            pageName,
            locator.ToString(),
            wait.Timeout.TotalSeconds
        );

        try
        {
            IWebElement element = wait.Until(ExpectedConditions.ElementExists(locator));
            logger.LogInformation(
                "Element found successfully on {PageName}. Locator: {ElementLocator}",
                pageName,
                locator.ToString()
            );

            return element;
        }
        catch (WebDriverTimeoutException ex)
        {
            logger.LogError(
                ex,
                "Timeout waiting for element to exist on {PageName}. Locator: {ElementLocator}, Timeout: {TimeoutSeconds}s.",
                pageName,
                locator.ToString(),
                wait.Timeout.TotalSeconds
            );
            throw;
        }
    }

    /// <summary>
    /// Waits for an element to exist in the DOM and returns it.
    /// </summary>
    /// <param name="wait">The <see cref="WebDriverWait"/> instance to extend.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance for logging.</param>
    /// <param name="locator">The <see cref="By"/> locator used to find the element.</param>
    /// <returns>The located <see cref="IWebElement"/> once it exists.</returns>
    /// <remarks>
    /// This method uses <see cref="ExpectedConditions.ElementExists(By)"/>.
    /// If the element does not exist within the timeout, a <see cref="WebDriverTimeoutException"/> is thrown.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if the element specified by <paramref name="locator"/> does not exist in the DOM within the timeout.</exception>
    [AllureStep("Wait and find element: {locator}")]
    public static IWebElement WaitForElement(this WebDriverWait wait, ILogger logger, By locator)
    {
        logger.LogDebug(
            "Waiting for element to exist. Locator: {ElementLocator}, Timeout: {TimeoutSeconds}s",
            locator.ToString(),
            wait.Timeout.TotalSeconds
        );

        try
        {
            IWebElement element = wait.Until(ExpectedConditions.ElementExists(locator));
            logger.LogInformation(
                "Element found successfully. Locator: {ElementLocator}",
                locator.ToString()
            );

            return element;
        }
        catch (WebDriverTimeoutException ex)
        {
            logger.LogError(
                ex,
                "Timeout waiting for element to exist. Locator: {ElementLocator}, Timeout: {TimeoutSeconds}s.",
                locator.ToString(),
                wait.Timeout.TotalSeconds
            );
            throw;
        }
    }

    /// <summary>
    /// Gets the display name of an enum value, typically from its <see cref="DisplayAttribute"/>.
    /// If the attribute or its Name property is not present, the enum value's string representation is returned.
    /// </summary>
    /// <param name="enumValue">The <see cref="Enum"/> value for which to retrieve the display name.</param>
    /// <returns>The display name specified by the <see cref="DisplayAttribute.Name"/> property,
    /// or the result of <c>enumValue.ToString()</c> if the attribute or name is not found.</returns>
    /// <remarks>
    /// This method uses reflection to access the <see cref="DisplayAttribute"/>.
    /// It is useful for presenting user-friendly names for enum values in logs, reports, or UI.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="enumValue"/> is null.</exception>
    [AllureStep("Get display name for enum: {enumValue}")]
    public static string GetDisplayName(this Enum enumValue)
    {
        Type enumType = enumValue.GetType();

        MemberInfo? memberInfo = enumType.GetMember(enumValue.ToString()).FirstOrDefault();

        if (memberInfo != null)
        {
            DisplayAttribute? displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();

            if (displayAttribute != null)
                return displayAttribute.Name ?? enumValue.ToString();
        }

        return enumValue.ToString();
    }

    /// <summary>
    /// Ensures that all elements specified by the given locators are visible on the page.
    /// It iterates through each locator and waits for the corresponding element to become visible.
    /// </summary>
    /// <param name="wait">The <see cref="WebDriverWait"/> instance to extend.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance for logging.</param>
    /// <param name="pageName">A descriptive name of the page where elements are being checked, used for logging.</param>
    /// <param name="locators">An <see cref="IEnumerable"/> of <see cref="By"/> locators for the critical elements to check.</param>
    /// <remarks>
    /// If <paramref name="locators"/> is null or empty, the method returns without action.
    /// If any element is not visible within the <see cref="WebDriverWait"/>'s timeout,
    /// a <see cref="WebDriverTimeoutException"/> is thrown for that specific element.
    /// This method is typically used to verify that a page or component has loaded correctly by checking its essential elements.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if any element specified by a locator in <paramref name="locators"/> is not visible within the timeout.</exception>
    /// <exception cref="Exception">Re-throws other unexpected exceptions encountered during visibility checks.</exception>
    [AllureStep("Ensuring {pageName} critical elements are visible")]
    public static void EnsureElementsAreVisible(this WebDriverWait wait, ILogger logger, string pageName, IEnumerable<By> locators)
    {
        if (locators?.Any() != true)
        {
            logger.LogTrace("No locators provided for EnsureElementsAreVisible on {PageName}. Skipping check.", pageName);
            return;
        }

        int locatorCount = locators.Count();
        logger.LogInformation(
            "Starting visibility check for {LocatorCount} critical element(s) on {PageName}. TimeoutPerElement: {TimeoutSeconds}s (approx).",
            locatorCount,
            pageName,
            wait.Timeout.TotalSeconds
        );

        int checkedCount = 0;
        foreach (By locator in locators)
        {
            checkedCount++;
            logger.LogDebug(
                "({CheckedCount}/{TotalCount}) Checking visibility of element on {PageName}. Locator: {ElementLocator}",
                checkedCount,
                locatorCount,
                pageName,
                locator.ToString()
            );

            try
            {
                _ = wait.Until(ExpectedConditions.ElementIsVisible(locator));
                logger.LogDebug(
                    "({CheckedCount}/{TotalCount}) Element IS VISIBLE on {PageName}. Locator: {ElementLocator}",
                    checkedCount,
                    locatorCount,
                    pageName,
                    locator.ToString()
                );
            }
            catch (WebDriverTimeoutException ex)
            {
                logger.LogError(
                    ex,
                    "({CheckedCount}/{TotalCount}) Timeout waiting for element to be VISIBLE on {PageName}. Locator: {ElementLocator}, Timeout: {TimeoutSeconds}s.",
                    checkedCount,
                    locatorCount,
                    pageName,
                    locator.ToString(),
                    wait.Timeout.TotalSeconds
                );
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "({CheckedCount}/{TotalCount}) Unexpected error checking visibility of element on {PageName}. Locator: {ElementLocator}",
                    checkedCount,
                    locatorCount,
                    pageName,
                    locator.ToString()
                );
                throw;
            }
        }

        logger.LogInformation(
            "All {LocatorCount} critical elements on {PageName} confirmed visible.",
            locatorCount,
            pageName
        );
    }

    /// <summary>
    /// Ensures that a single element specified by the given locator is visible on the page.
    /// </summary>
    /// <param name="wait">The <see cref="WebDriverWait"/> instance to extend.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance for logging.</param>
    /// <param name="pageName">A descriptive name of the page where the element is being checked, used for logging.</param>
    /// <param name="locator">The <see cref="By"/> locator for the element to check. Must not be null.</param>
    /// <remarks>
    /// If the <paramref name="locator"/> is null, an <see cref="ArgumentNullException"/> is thrown.
    /// If the element is not visible within the <see cref="WebDriverWait"/>'s timeout,
    /// a <see cref="WebDriverTimeoutException"/> is thrown.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="locator"/> is null.</exception>
    /// <exception cref="WebDriverTimeoutException">Thrown if the element specified by <paramref name="locator"/> is not visible within the timeout.</exception>
    /// <exception cref="Exception">Re-throws other unexpected exceptions encountered during the visibility check.</exception>
    [AllureStep("Ensuring {pageName} critical element is visible")]
    public static void EnsureElementIsVisible(this WebDriverWait wait, ILogger logger, string pageName, By locator)
    {
        if (locator == null)
        {
            logger.LogError("Locator provided to EnsureElementIsVisible for {PageName} was null. This indicates a setup issue.", pageName);
            throw new ArgumentNullException(nameof(locator), $"Locator cannot be null when calling EnsureElementIsVisible for page {pageName}.");
        }

        logger.LogInformation(
            "Starting visibility check for element on {PageName}. Locator: {ElementLocator}, Timeout: {TimeoutSeconds}s",
            pageName,
            locator.ToString(),
            wait.Timeout.TotalSeconds
        );

        try
        {
            _ = wait.Until(ExpectedConditions.ElementIsVisible(locator));
            logger.LogInformation(
                "Element IS VISIBLE on {PageName}. Locator: {ElementLocator}",
                pageName,
                locator.ToString()
            );
        }
        catch (WebDriverTimeoutException ex)
        {
            logger.LogError(
                ex,
                "Timeout waiting for element to be VISIBLE on {PageName}. Locator: {ElementLocator}, Timeout: {TimeoutSeconds}s.",
                pageName,
                locator.ToString(),
                wait.Timeout.TotalSeconds
            );
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error checking visibility of element on {PageName}. Locator: {ElementLocator}",
                pageName,
                locator.ToString()
            );
            throw;
        }
    }

    /// <summary>
    /// Waits for all elements matching the given locator to be present in the DOM and returns them.
    /// </summary>
    /// <param name="wait">The <see cref="WebDriverWait"/> instance to extend.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance for logging.</param>
    /// <param name="pageName">A descriptive name of the page where the elements are expected, used for logging.</param>
    /// <param name="locator">The <see cref="By"/> locator used to find the elements.</param>
    /// <returns>An <see cref="IEnumerable{IWebElement}"/> containing all found elements. Returns an empty collection if no elements are found or if a timeout occurs while waiting for presence.</returns>
    /// <remarks>
    /// This method uses <see cref="ExpectedConditions.PresenceOfAllElementsLocatedBy(By)"/>.
    /// If a timeout occurs while waiting for elements to be present, a warning is logged, and an empty collection is returned
    /// (it does not throw <see cref="WebDriverTimeoutException"/> directly from this method in that case, differing from single element waits).
    /// Other unexpected exceptions during the process are re-thrown.
    /// </remarks>
    /// <exception cref="Exception">Re-throws unexpected exceptions encountered, other than <see cref="WebDriverTimeoutException"/> which is handled by returning an empty list.</exception>
    [AllureStep("Wait and find all elements: {locator}")]
    public static IEnumerable<IWebElement> WaitForElements(this WebDriverWait wait, ILogger logger, string pageName, By locator)
    {
        logger.LogDebug(
            "Waiting for all elements to be present on {PageName}. Locator: {ElementLocator}, Timeout: {TimeoutSeconds}s",
            pageName,
            locator.ToString(),
            wait.Timeout.TotalSeconds
        );

        try
        {
            ReadOnlyCollection<IWebElement> elements = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(locator));

            if (elements != null && elements.Count != 0)
            {
                logger.LogInformation(
                    "{Count} element(s) found successfully on {PageName}. Locator: {ElementLocator}",
                    elements.Count,
                    pageName,
                    locator.ToString()
                );
                return elements;
            }

            logger.LogWarning(
                "No elements found on {PageName} for locator {ElementLocator} after wait, though no timeout occurred.",
                pageName,
                locator.ToString()
            );

            return [];
        }
        catch (WebDriverTimeoutException ex)
        {
            logger.LogWarning(
                ex,
                "Timeout waiting for elements to be present on {PageName}. Locator: {ElementLocator}, Timeout: {TimeoutSeconds}s. Returning empty collection.",
                pageName,
                locator.ToString(),
                wait.Timeout.TotalSeconds
            );

            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An unexpected error occurred while waiting for elements on {PageName}. Locator: {ElementLocator}.",
                pageName,
                locator.ToString()
            );

            throw;
        }
    }
}
