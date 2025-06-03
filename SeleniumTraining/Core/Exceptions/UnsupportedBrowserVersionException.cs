namespace SeleniumTraining.Core.Exceptions;

/// <summary>
/// Represents an error that occurs when an attempt is made to use a browser version
/// that is not supported by the test framework or the application under test.
/// </summary>
/// <remarks>
/// This exception is typically thrown during WebDriver initialization if the detected
/// browser version does not meet the minimum requirements specified in the configuration
/// or defined by the testing strategy.
/// </remarks>
public class UnsupportedBrowserVersionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedBrowserVersionException"/> class.
    /// </summary>
    public UnsupportedBrowserVersionException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedBrowserVersionException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UnsupportedBrowserVersionException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedBrowserVersionException"/> class
    /// with a specified error message and a reference to the inner exception that is
    /// the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception,
    /// or a null reference if no inner exception is specified.</param>
    public UnsupportedBrowserVersionException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedBrowserVersionException"/> class
    /// with a formatted error message indicating the browser, detected version, and minimum required version.
    /// </summary>
    /// <param name="browserName">The name of the browser (e.g., "Chrome", "Firefox").</param>
    /// <param name="detectedVersion">The version of the browser that was detected.</param>
    /// <param name="minimumRequiredVersion">The minimum version of the browser that is supported.</param>
    public UnsupportedBrowserVersionException(string browserName, string detectedVersion, string minimumRequiredVersion)
        : base($"Browser '{browserName}' version '{detectedVersion}' is not supported. Minimum required version is '{minimumRequiredVersion}'.")
    {

    }
}
