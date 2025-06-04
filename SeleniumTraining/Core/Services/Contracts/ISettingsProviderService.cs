namespace SeleniumTraining.Core.Services.Contracts;

/// <summary>
/// Defines the contract for a service that provides access to application settings and configurations.
/// </summary>
/// <remarks>
/// This service typically interacts with configuration sources like appsettings.json,
/// environment variables, or other configuration providers to retrieve and deserialize
/// settings into strongly-typed objects. It plays a crucial role in making the framework
/// configurable and adaptable to different environments and requirements.
/// </remarks>
public interface ISettingsProviderService
{
    /// <summary>
    /// Gets the root <see cref="IConfiguration"/> instance.
    /// This provides access to the complete set of application configuration data.
    /// </summary>
    /// <remarks>
    /// The IConfiguration instance can be used to directly access configuration values
    /// by key or to bind entire sections to objects.
    /// </remarks>
    /// <value>The root <see cref="IConfiguration"/> instance.</value>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Retrieves browser-specific settings based on the provided <see cref="BrowserType"/>.
    /// </summary>
    /// <param name="browserType">The type of browser (e.g., Chrome, Firefox) for which to retrieve settings.</param>
    /// <returns>A <see cref="BaseBrowserSettings"/> object containing the configuration for the specified browser.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if settings for the specified <paramref name="browserType"/> are not found or not configured.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the configuration section for the browser settings is malformed or missing required values.</exception>
    [AllureStep("Retrieving browser settings")]
    public BaseBrowserSettings GetBrowserSettings(BrowserType browserType);

    /// <summary>
    /// Retrieves and deserializes a configuration section into an instance of the specified class <typeparamref name="TClassSite"/>.
    /// </summary>
    /// <typeparam name="TClassSite">The type of the class to deserialize the configuration section into. Must be a reference type and have a public parameterless constructor for binding.</typeparam>
    /// <param name="sectionName">The name (key) of the configuration section to retrieve (e.g., "SauceDemo", "TestFrameworkSettings").</param>
    /// <returns>An instance of <typeparamref name="TClassSite"/> populated with values from the specified configuration section.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sectionName"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the configuration section <paramref name="sectionName"/> is not found,
    /// or if the settings cannot be bound to <typeparamref name="TClassSite"/> (e.g., missing required fields, type mismatches).
    /// </exception>
    /// <remarks>
    /// This method uses Microsoft.Extensions.Configuration's binding capabilities.
    /// Ensure that the properties in <typeparamref name="TClassSite"/> match the keys in the configuration section.
    /// Data annotation validation (e.g., [Required]) on <typeparamref name="TClassSite"/> properties is typically handled
    /// during the options registration process in DI, not directly by this method's retrieval.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Assuming SauceDemoSettings class and "SauceDemo" section in appsettings.json
    /// SauceDemoSettings sauceSettings = settingsProvider.GetSettings&lt;SauceDemoSettings&gt;("SauceDemo");
    /// string username = sauceSettings.LoginUsernameStandardUser;
    /// </code>
    /// </example>
    [AllureStep("Retrieving settings for section: {sectionName}")]
    public TClassSite GetSettings<TClassSite>(string sectionName) where TClassSite : class;
}
