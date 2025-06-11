namespace SeleniumTraining.Utils.Settings.TestFeatures;

/// <summary>
/// Defines the settings for the Polly retry policy, specifically which
/// exceptions should be handled by the retry mechanism.
/// </summary>
public class RetryPolicySettings
{
    /// <summary>
    /// Gets or sets a list of the full type names of exceptions that are considered transient
    /// and should trigger a retry.
    /// </summary>
    /// <value>A list of strings, where each string is a full exception type name (e.g., "OpenQA.Selenium.NoSuchElementException").</value>
    [Required]
    public required List<string> RetryableExceptionFullNames { get; set; } = [];
}
