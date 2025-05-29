namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    private readonly List<KeyValuePair<SortByType, string>> _inventoryProductsDropdownOptions =
    [
        new(SortByType.Text, "Name (A to Z)"),
        new(SortByType.Text, "Name (Z to A)"),
        new(SortByType.Text, "Price (low to high)"),
        new(SortByType.Text, "Price (high to low)"),
        new(SortByType.Value, "az"),
        new(SortByType.Value, "za"),
        new(SortByType.Value, "lohi"),
        new(SortByType.Value, "hilo")
    ];
}
