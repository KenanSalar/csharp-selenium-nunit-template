using OpenQA.Selenium;

namespace SeleniumTraining.Utils.Extensions;

public static partial class ExtensionMethods
{
    [AllureStep("Select dropdown by {selectorType}")]
    public static void SelectDropDown(this IWebElement element, SortByType selectorType, string selectorValue, WebDriverWait wait)
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
                throw new ArgumentOutOfRangeException(nameof(selectorType), selectorType, null);
        }
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
