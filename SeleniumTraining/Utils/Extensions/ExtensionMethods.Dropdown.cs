using OpenQA.Selenium;

namespace SeleniumTraining.Utils.Extensions;

public static partial class ExtensionMethods
{
    /// <summary>
    /// Selects an option in a dropdown (HTML select element) based on the specified selector type (text or value).
    /// It waits for the dropdown element to be enabled and displayed before attempting the selection.
    /// If configured, it also highlights the element before interaction.
    /// </summary>
    /// <param name="element">The <see cref="IWebElement"/> representing the HTML select (dropdown) element.</param>
    /// <param name="selectorType">The <see cref="SortByType"/> criteria to use for selection (e.g., <see cref="SortByType.Text"/> or <see cref="SortByType.Value"/>).</param>
    /// <param name="selectorValue">The actual text or value of the option to select.</param>
    /// <param name="wait">The <see cref="WebDriverWait"/> instance to use for waiting for the element to be interactable.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance, used for element highlighting if enabled.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance for logging actions and errors.</param>
    /// <param name="settings">The <see cref="TestFrameworkSettings"/> containing configurations like highlighting preferences.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported <paramref name="selectorType"/> is provided.</exception>
    /// <exception cref="NoSuchElementException">Thrown by <see cref="SelectElement"/> methods if the specified option (text or value) does not exist in the dropdown.</exception>
    /// <exception cref="WebDriverTimeoutException">Thrown by <paramref name="wait"/> if the element does not become enabled and displayed within the timeout period.</exception>
    /// <remarks>
    /// This method first ensures the dropdown element is interactable. It then uses <see cref="SelectElement"/>
    /// to perform the selection. The action is logged with details about the selection.
    /// </remarks>
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

    /// <summary>
    /// Selects multiple options in a multi-select dropdown (HTML select element with 'multiple' attribute)
    /// by their 'value' attributes.
    /// </summary>
    /// <param name="webElement">The <see cref="IWebElement"/> representing the HTML multi-select element.</param>
    /// <param name="values">An array of string values corresponding to the 'value' attributes of the options to be selected.</param>
    /// <remarks>
    /// This method iterates through the provided <paramref name="values"/> and calls <see cref="SelectElement.SelectByValue(string)"/>
    /// for each one. It assumes the target select element supports multiple selections.
    /// If an option with a specified value does not exist, a <see cref="NoSuchElementException"/> will be thrown.
    /// No de-selection of other options is performed by this method; it only adds to the current selection.
    /// </remarks>
    /// <exception cref="NoSuchElementException">Thrown if any of the specified values do not correspond to an existing option in the multi-select dropdown.</exception>
    [AllureStep("Multi-selecting dropdown: {webElement} by values: {values}")]
    public static void MultiSelectElements(this IWebElement webElement, string[] values)
    {
        SelectElement multiSelect = new(webElement);

        foreach (string value in values)
        {
            multiSelect.SelectByValue(value);
        }
    }

    /// <summary>
    /// Gets the text of all currently selected options from a dropdown or multi-select list.
    /// </summary>
    /// <param name="webElement">The <see cref="IWebElement"/> representing the HTML select element.</param>
    /// <returns>An array of strings, where each string is the visible text of a selected option.
    /// Returns an empty array if no options are selected.</returns>
    /// <remarks>
    /// This method uses <see cref="SelectElement.AllSelectedOptions"/> to retrieve the selected
    /// <see cref="IWebElement"/>s and then extracts their text content.
    /// </remarks>
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
