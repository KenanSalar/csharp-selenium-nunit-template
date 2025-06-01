using OpenQA.Selenium;

namespace SeleniumTraining.Utils.Extensions;

public static partial class ExtensionMethods
{
    [AllureStep("Select dropdown by {selectorType}")]
    public static void SelectDropDown(
        this IWebElement element,
        SortByType selectorType,
        string selectorValue,
        WebDriverWait wait,
        IWebDriver driver,
        ILogger logger,
        TestFrameworkSettings settings
    )
    {
        _ = wait.Until(_ =>
        {
            try
            {
                return element.Enabled && element.Displayed;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });

        if (settings.HighlightElementsOnInteraction)
            _ = element.HighlightElement(driver, logger, settings.HighlightDurationMs);

        var select = new SelectElement(element);

        switch (selectorType)
        {
            case SortByType.Text:
                select.SelectByText(selectorValue);
                break;
            case SortByType.Value:
                select.SelectByValue(selectorValue);
                break;
            default:
                logger.LogError("Unsupported sort selector type: {SelectorType}", selectorType);
                throw new ArgumentOutOfRangeException(nameof(selectorType), selectorType, null);
        }

        logger.LogInformation(
            "Selected dropdown option by {SelectorType} with value '{SelectorValue}' for element: {ElementDescription}",
            selectorType,
            selectorValue,
            WebElementHighlightingExtensions.GetElementDescription(element)
        );
    }

    [AllureStep("Multi-selecting dropdown: {webElement} by values: {values}")]
    public static void MultiSelectElements(this IWebElement webElement, string[] values)
    {
        SelectElement multiSelect = new(webElement);

        foreach (string value in values)
        {
            multiSelect.SelectByValue(value);
        }
    }

    [AllureStep("Getting all selected options from Element: {webElement}")]
    public static string[] GetAllSelectedLists(this IWebElement webElement)
    {
        SelectElement multiSelect = new(webElement);
        IList<IWebElement> selectedElements = multiSelect.AllSelectedOptions;
        int count = selectedElements.Count;
        string[] options = new string[count];

        for (int i = 0; i < count; i++)
        {
            options[i] = selectedElements[i].Text;
        }

        return options;
    }
}
