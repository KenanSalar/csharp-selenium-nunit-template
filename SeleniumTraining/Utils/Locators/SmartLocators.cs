namespace SeleniumTraining.Utils.Locators;

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
    /// Creates a By object to find an element containing specific text (case-sensitive).
    /// </summary>
    /// <param name="text">The partial text content to match.</param>
    /// <param name="tagName">Optional. The tag name of the element. Defaults to "*" (any element).</param>
    /// <returns>A By object for XPath.</returns>
    public static By TextContains(string text, string tagName = "*")
    {
        return By.XPath($"//{tagName}[contains(normalize-space(.), '{text}')]");
    }
}


