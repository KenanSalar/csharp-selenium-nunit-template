using System.Runtime.CompilerServices;

namespace SeleniumTraining.Utils.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IWebElement"/> instances,
/// primarily for visually highlighting elements in the browser using JavaScript.
/// This is useful for debugging and observing test execution.
/// </summary>
/// <remarks>
/// The highlighting mechanism temporarily alters the element's style (border, outline, background color)
/// and can optionally revert the changes after a specified duration or be undone explicitly.
/// Original styles are stored and restored to avoid permanent changes to the page's appearance.
/// These methods require the <see cref="IWebDriver"/> instance to support <see cref="IJavaScriptExecutor"/>.
/// In CI/CD environments, highlighting might be disabled or its behavior controlled by framework settings
/// to avoid unnecessary overhead or visual noise in headless runs.
/// </remarks>
public static class WebElementHighlightingExtensions
{
    /// <summary>
    /// JavaScript code to apply visual highlighting styles to a web element.
    /// Sets a red border, orange outline, and yellow background.
    /// </summary>
    private const string HighlightJsScript = @"
        arguments[0].style.border='3px solid red';
        arguments[0].style.outline='3px solid orange';
        arguments[0].style.backgroundColor='yellow';";

    /// <summary>
    /// JavaScript code to remove the visual highlighting styles applied by <see cref="HighlightJsScript"/>.
    /// Clears border, outline, and background color styles.
    /// </summary>
    private const string UnhighlightJsScript = @"
        arguments[0].style.border='';
        arguments[0].style.outline='';
        arguments[0].style.backgroundColor='';";

    /// <summary>
    /// A thread-safe table to store the original inline style of elements before highlighting.
    /// This allows restoring the element's appearance accurately when unhighlighted.
    /// The key is the <see cref="IWebElement"/> and the value is its original 'style' attribute string.
    /// </summary>
    /// <remarks>
    /// <see cref="ConditionalWeakTable{TKey, TValue}"/> is used because it allows the garbage collector
    /// to collect the element (key) if it's no longer referenced elsewhere, automatically removing the entry.
    /// </remarks>
    private static readonly ConditionalWeakTable<IWebElement, string> _originalStyles = [];

    /// <summary>
    /// Highlights the specified web element by temporarily changing its style (border, outline, background).
    /// If a duration is provided, the element is automatically unhighlighted after that duration.
    /// </summary>
    /// <param name="element">The <see cref="IWebElement"/> to highlight. If null, a warning is logged, and null is returned.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance, which must implement <see cref="IJavaScriptExecutor"/>.</param>
    /// <param name="logger">Optional. The <see cref="ILogger"/> for logging actions, warnings, or errors related to highlighting.</param>
    /// <param name="durationMs">Optional. The duration in milliseconds for which the element should remain highlighted.
    /// If 0 or less, the element remains highlighted until <see cref="UnhighlightElement"/> is called.
    /// If greater than 0, <see cref="UnhighlightElement"/> is called automatically after the duration.</param>
    /// <returns>The original <see cref="IWebElement"/> instance, allowing for fluent chaining. Returns <c>null!</c> if the input element was null.</returns>
    /// <remarks>
    /// Before applying highlight styles, this method attempts to store the element's original inline 'style' attribute.
    /// This original style is restored when <see cref="UnhighlightElement"/> is called.
    /// If the WebDriver does not support JavaScript execution, a warning is logged, and the element is returned unhighlighted.
    /// Any exceptions during JavaScript execution are caught and logged.
    /// </remarks>
    public static IWebElement HighlightElement(
        this IWebElement element,
        IWebDriver driver,
        ILogger? logger = null,
        int durationMs = 0
    )
    {
        if (element == null)
        {
            logger?.LogWarning("Attempted to highlight a null element.");
            return null!;
        }

        if (driver is not IJavaScriptExecutor jsExecutor)
        {
            logger?.LogWarning("WebDriver does not support IJavaScriptExecutor. Cannot highlight element.");
            return element;
        }

        try
        {
            if (!_originalStyles.TryGetValue(element, out string? originalStyle))
            {
                originalStyle = jsExecutor.ExecuteScript("return arguments[0].getAttribute('style');", element) as string;
                if (originalStyle != null)
                {
                    _originalStyles.Add(element, originalStyle);
                }
            }

            _ = jsExecutor.ExecuteScript(HighlightJsScript, element);
            logger?.LogTrace("Element highlighted: {ElementDescription}", GetElementDescription(element));

            if (durationMs > 0)
            {
                Thread.Sleep(durationMs);
                _ = UnhighlightElement(element, driver, logger);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to highlight element: {ElementDescription}", GetElementDescription(element));
        }
        return element;
    }

    /// <summary>
    /// Removes the highlighting styles from the specified web element, attempting to restore its original inline style.
    /// </summary>
    /// <param name="element">The <see cref="IWebElement"/> to unhighlight. If null, a warning is logged, and null is returned.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance, which must implement <see cref="IJavaScriptExecutor"/>.</param>
    /// <param name="logger">Optional. The <see cref="ILogger"/> for logging actions, warnings, or errors related to unhighlighting.</param>
    /// <returns>The original <see cref="IWebElement"/> instance, allowing for fluent chaining. Returns <c>null!</c> if the input element was null.</returns>
    /// <remarks>
    /// This method checks if an original style was stored for the element in <see cref="_originalStyles"/>.
    /// <list type="bullet">
    ///   <item><description>If an original style string was stored (even an empty one), it's reapplied using <c>setAttribute('style', originalStyle)</c>.</description></item>
    ///   <item><description>If the stored original style was <c>null</c> (meaning no 'style' attribute was initially present), the 'style' attribute is removed using <c>removeAttribute('style')</c>.</description></item>
    ///   <item><description>If no original style was found in the table (e.g., element wasn't highlighted by this utility or table entry was lost), it applies the <see cref="UnhighlightJsScript"/> to clear common highlight styles.</description></item>
    /// </list>
    /// The element's entry is removed from the <see cref="_originalStyles"/> table after unhighlighting.
    /// If the WebDriver does not support JavaScript execution, a warning is logged, and the element is returned as is.
    /// Any exceptions during JavaScript execution are caught and logged.
    /// </remarks>
    public static IWebElement UnhighlightElement(
        this IWebElement element,
        IWebDriver driver,
        ILogger? logger = null)
    {
        if (element == null)
        {
            logger?.LogWarning("Attempted to unhighlight a null element.");
            return null!;
        }

        if (driver is not IJavaScriptExecutor jsExecutor)
        {
            logger?.LogWarning("WebDriver does not support IJavaScriptExecutor. Cannot unhighlight element.");
            return element;
        }

        try
        {
            if (_originalStyles.TryGetValue(element, out string? originalStyle))
            {
                if (originalStyle != null)
                {
                    _ = jsExecutor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);", element, originalStyle);
                }
                else
                {
                    _ = jsExecutor.ExecuteScript("arguments[0].removeAttribute('style');", element);
                }
                _ = _originalStyles.Remove(element);
            }
            else
            {
                _ = jsExecutor.ExecuteScript(UnhighlightJsScript, element);
            }
            logger?.LogTrace("Element unhighlighted: {ElementDescription}", GetElementDescription(element));
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to unhighlight element: {ElementDescription}", GetElementDescription(element));
        }
        return element;
    }

    /// <summary>
    /// Generates a descriptive string for a given web element, including its tag name,
    /// and common identifying attributes like ID, name, class, and a snippet of its text content.
    /// </summary>
    /// <param name="element">The <see cref="IWebElement"/> to describe. If null or if properties cannot be accessed, a default error message or partial description is returned.</param>
    /// <returns>A string containing a summary of the element's properties, or an error message if description fails.</returns>
    /// <remarks>
    /// This helper method is used for creating more informative log messages when highlighting or unhighlighting elements.
    /// It attempts to retrieve several attributes and text, truncating long text to keep the description concise.
    /// Catches exceptions during attribute retrieval to prevent logging failures.
    /// </remarks>
    public static string GetElementDescription(IWebElement element)
    {
        try
        {
            string tagName = element.TagName ?? "unknown_tag";
            string? id = element.GetAttribute("id");
            string? name = element.GetAttribute("name");
            string? classAttr = element.GetAttribute("class");
            string? text = element.Text?.Length > 50 ? $"{element.Text.AsSpan(0, 50)}..." : element.Text;

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(tagName))
                parts.Add($"Tag:'{tagName}'");

            if (!string.IsNullOrWhiteSpace(id))
                parts.Add($"Id:'{id}'");

            if (!string.IsNullOrWhiteSpace(name))
                parts.Add($"Name:'{name}'");

            if (!string.IsNullOrWhiteSpace(classAttr))
                parts.Add($"Class:'{classAttr}'");

            if (!string.IsNullOrWhiteSpace(text))
                parts.Add($"Text:'{text}'");

            return string.Join(", ", parts);
        }
        catch
        {
            return "ErrorFetchingElementDescription";
        }
    }
}
