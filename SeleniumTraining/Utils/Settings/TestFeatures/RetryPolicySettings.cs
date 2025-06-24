namespace SeleniumTraining.Utils.Settings.TestFeatures;

/// <summary>
/// Defines the settings for the Polly retry policy as an immutable record,
/// specifically which exceptions should be handled by the retry mechanism.
/// </summary>
/// <remarks>
/// This record is bound to the "RetryPolicySettings" configuration section and ensures
/// that the list of retryable exceptions is fixed once the application starts.
/// </remarks>
public record RetryPolicySettings
{
    /// <summary>
    /// Gets a list of the full type names of exceptions that are considered transient
    /// and should trigger a retry.
    /// </summary>
    /// <value>A list of strings, where each string is a full exception type name (e.g., "OpenQA.Selenium.NoSuchElementException").</value>
    [Required]
    public required List<string> RetryableExceptionFullNames { get; init; } = [];
}
