namespace SeleniumTraining.Utils.Locators;

/// <summary>
/// Provides a collection of static helper methods for creating robust Selenium <see cref="By"/> locators.
/// These methods often target attributes like 'data-test' or 'aria-label', or utilize XPath
/// functions for text-based searching, aiming for more maintainable and less brittle locators
/// compared to relying solely on dynamic IDs or complex CSS/XPath structures.
/// </summary>
/// <remarks>
/// This utility class is designed to promote best practices in element location strategies
/// by encouraging the use of test-specific attributes (like 'data-test') or accessibility attributes ('aria-label').
/// Such locators are generally more resilient to UI changes that do not alter the core functionality
/// or accessibility contract of an element.
/// Using these helpers can lead to more readable and stable automated tests.
/// </remarks>
public static class SmartLocators
{
    /// <summary>
    /// Creates a By object for elements with a specific data-test attribute.
    /// </summary>
    /// <param name="testId">The value of the data-test attribute.</param>
    /// <returns>A By object for CSS selector.</returns>
    public static By DataTest(string testId)
    {
        return By.CssSelector($"[data-test='{testId}']");
    }

    /// <summary>
    /// Creates a By object for elements with a specific aria-label attribute.
    /// </summary>
    /// <param name="label">The value of the aria-label attribute.</param>
    /// <returns>A By object for CSS selector.</returns>
    public static By AriaLabel(string label)
    {
        return By.CssSelector($"[aria-label='{label}']");
    }

    /// <summary>
    /// Creates a By object to find an element by its exact normalized text content.
    /// Best for elements where text is unique and static.
    /// </summary>
    /// <param name="text">The exact text content to match (case-sensitive, ignores leading/trailing/excess internal spaces).</param>
    /// <param name="tagName">Optional. The tag name of the element (e.g., "button", "div"). Defaults to "*" (any element).</param>
    /// <returns>A By object for XPath.</returns>
    public static By TextEquals(string text, string tagName = "*")
    {
        return By.XPath($"//{tagName}[normalize-space(.)='{text}']");
    }

    /// <summary>
    /// Creates a <see cref="By"/> object to find an element whose normalized text content contains the specified text (case-sensitive).
    /// </summary>
    /// <param name="text">The partial text content to match. The <c>normalize-space()</c> XPath function is used on the element's text before checking for containment.</param>
    /// <param name="tagName">Optional. The tag name of the element. Defaults to "*" (any element).</param>
    /// <returns>A <see cref="By"/> object using an XPath selector.</returns>
    /// <remarks>
    /// Useful for elements where only part of the text is static or relevant.
    /// The XPath generated is <c>//tagName[contains(normalize-space(.), 'text')]</c>.
    /// </remarks>
    public static By TextContains(string text, string tagName = "*")
    {
        return By.XPath($"//{tagName}[contains(normalize-space(.), '{text}')]");
    }

    /// <summary>
    /// Creates a <see cref="By"/> object to find an element of a specific tag that contains a given class
    /// and has an exact text match (case-sensitive).
    /// </summary>
    /// <param name="text">The exact text content the element should have.</param>
    /// <param name="classNameContains">A class name that the element's class attribute must contain.</param>
    /// <param name="tagName">Optional. The tag name of the element (e.g., "button"). Defaults to "*" (any element).</param>
    /// <returns>A <see cref="By"/> object using an XPath selector.</returns>
    /// <remarks>
    /// This locator is useful for elements like buttons where matching text and a class provides specificity.
    /// The XPath generated is <c>.//tagName[contains(@class, 'classNameContains') and text()='text']</c>.
    /// The <c>.//</c> makes the search relative to the context node if used with <c>FindElement</c> on an existing element.
    /// Note: This uses an exact text match via <c>text()='{text}'</c> rather than normalizing space for this specific overload.
    /// </remarks>
    public static By TextContains(string text, string classNameContains, string tagName = "*")
    {
        return By.XPath($".//{tagName}[contains(@class, '{classNameContains}') and text()='{text}']");
    }
}
