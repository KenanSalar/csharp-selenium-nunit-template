namespace SeleniumTraining.Tests.SauceDemoTests.TestDataModels;

/// <summary>
/// Represents a single, self-contained test case for the end-to-end checkout flow as an immutable data record.
/// </summary>
/// <remarks>
/// This record serves as a strongly-typed model that directly maps to the structure of a JSON object
/// in a test data file (e.g., CheckoutScenarios.json). It is used by Newtonsoft.Json to deserialize
/// test scenarios into C# objects, which are then fed into a data-driven test via NUnit's TestCaseSource.
/// As a record, it provides value-based equality and immutability, ensuring that test data cannot be accidentally
/// modified during a test run.
/// </remarks>
public record CheckoutTestData
{
    /// <summary>
    /// Gets the descriptive name for the test case.
    /// </summary>
    /// <remarks>
    /// This name is used by NUnit's `TestCaseData.SetName()` method to provide a readable and
    /// unique name for each test variation in the Test Explorer and in test reports.
    /// </remarks>
    /// <value>The unique name of the test case.</value>
    public required string TestCaseName { get; init; }

    /// <summary>
    /// Gets the first name to be entered into the checkout form.
    /// </summary>
    /// <value>The user's first name.</value>
    public required string FirstName { get; init; }

    /// <summary>
    /// Gets the last name to be entered into the checkout form.
    /// </summary>
    /// <value>The user's last name.</value>
    public required string LastName { get; init; }

    /// <summary>
    /// Gets the postal code to be entered into the checkout form.
    /// </summary>
    /// <value>The postal or zip code.</value>
    public required string PostalCode { get; init; }

    /// <summary>
    /// Gets the list of product names that should be added to the cart
    /// as a precondition for this specific test case.
    /// </summary>
    /// <value>A list of strings, where each string is the exact name of a product.</value>
    public required List<string> ItemsToOrder { get; init; }
}
