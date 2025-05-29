using System.CodeDom;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SeleniumTraining.Utils.Extensions;

public static partial class ExtensionMethods
{
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
}
