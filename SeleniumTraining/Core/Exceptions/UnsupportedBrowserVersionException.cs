namespace SeleniumTraining.Core.Exceptions;

public class UnsupportedBrowserVersionException : Exception
{
    public UnsupportedBrowserVersionException() { }
    public UnsupportedBrowserVersionException(string message) : base(message) { }
    public UnsupportedBrowserVersionException(string message, Exception innerException) : base(message, innerException) { }

    public UnsupportedBrowserVersionException(string browserName, string detectedVersion, string minimumRequiredVersion)
        : base($"Browser '{browserName}' version '{detectedVersion}' is not supported. Minimum required version is '{minimumRequiredVersion}'.")
    {

    }
}
