namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// A private readonly list containing tuples that represent the expected options
    /// in the inventory product sort dropdown on SauceDemo.
    /// Each tuple consists of a <see cref="SortByType"/> (Text or Value) and the corresponding
    /// string expected for that selection method.
    /// </summary>
    /// <remarks>
    /// This list is used in tests to iterate through all possible sort options and verify
    /// that they can be selected and potentially that the sorting is applied correctly.
    /// The options include:
    /// <list type="bullet">
    ///   <item><description>Sort by Text: "Name (A to Z)", "Name (Z to A)", "Price (low to high)", "Price (high to low)"</description></item>
    ///   <item><description>Sort by Value: "az", "za", "lohi", "hilo"</description></item>
    /// </list>
    /// </remarks>
    private readonly List<KeyValuePair<SortByType, string>> _inventoryProductsDropdownOptions = [
        new(SortByType.Text, "Name (A to Z)"),
        new(SortByType.Text, "Name (Z to A)"),
        new(SortByType.Text, "Price (low to high)"),
        new(SortByType.Text, "Price (high to low)"),
        new(SortByType.Value, "az"),
        new(SortByType.Value, "za"),
        new(SortByType.Value, "lohi"),
        new(SortByType.Value, "hilo")
    ];

    /// <summary>
    /// Provides test case data for checkout scenarios by reading from a JSON file.
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static IEnumerable<TestCaseData> CheckoutScenarios
    {
        get
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Tests/SauceDemoTests/TestData/CheckoutScenarios.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The test data file 'CheckoutScenarios.json' was not found.", filePath);
            }

            string jsonContent = File.ReadAllText(filePath);
            List<CheckoutTestData>? testDataList = JsonConvert.DeserializeObject<List<CheckoutTestData>>(jsonContent)
                ?? throw new InvalidOperationException("Failed to deserialize test data from CheckoutScenarios.json.");

            foreach (CheckoutTestData testData in testDataList)
            {
                var testCase = new TestCaseData(testData.FirstName, testData.LastName, testData.PostalCode, testData.ItemsToOrder);
                _ = testCase.SetName(testData.TestCaseName);

                yield return testCase;
            }
        }
    }
}
