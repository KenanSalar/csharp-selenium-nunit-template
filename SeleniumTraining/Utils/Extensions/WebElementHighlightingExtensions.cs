using System.Runtime.CompilerServices;

namespace SeleniumTraining.Utils.Extensions;

public static class WebElementHighlightingExtensions
{
    private const string HighlightJsScript = @"
        arguments[0].style.border='3px solid red';
        arguments[0].style.outline='3px solid orange';
        arguments[0].style.backgroundColor='yellow';";

    private const string UnhighlightJsScript = @"
        arguments[0].style.border='';
        arguments[0].style.outline='';
        arguments[0].style.backgroundColor='';";

    private static readonly ConditionalWeakTable<IWebElement, string> _originalStyles = [];

    /// <summary>
    /// Highlights the specified IWebElement by drawing a border and changing background color.
    /// Remember to call UnhighlightElement to revert changes.
    /// </summary>
    /// <param name="element">The IWebElement to highlight.</param>
    /// <param name="driver">The IWebDriver instance (must support IJavaScriptExecutor).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="durationMs">Optional. If greater than 0, the highlight will persist for this duration and then automatically unhighlight.</param>
    /// <returns>The original IWebElement, for chaining.</returns>
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
    /// Removes the highlight from the specified IWebElement by reverting its style.
    /// </summary>
    /// <param name="element">The IWebElement to unhighlight.</param>
    /// <param name="driver">The IWebDriver instance (must support IJavaScriptExecutor).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <returns>The original IWebElement, for chaining.</returns>
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

    public static string GetElementDescription(IWebElement element)
    {
        try
        {
            string tagName = element.TagName ?? "unknown_tag";
            string? id = element.GetAttribute("id");
            string? name = element.GetAttribute("name");
            string? classAttr = element.GetAttribute("class");
            string? text = element.Text?.Length > 50 ? string.Concat(element.Text.AsSpan(0, 50), "...") : element.Text;

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
