namespace SeleniumTraining.Utils.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IConfigurationBuilder"/> to standardize
/// the way application configuration is built across the solution.
/// </summary>
/// <remarks>
/// This class helps in centralizing the configuration setup logic, ensuring that
/// all parts of the application or test framework load settings from consistent sources
/// (e.g., appsettings.json, environment-specific appsettings, environment variables, user secrets).
/// This is particularly useful for managing configurations in different environments,
/// such as development, staging, and production, including CI/CD environments.
/// </remarks>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Builds the application configuration by adding standard configuration sources
    /// to the <see cref="IConfigurationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> instance to extend.</param>
    /// <param name="userSecretsAssemblyType">A <see cref="Type"/> from the assembly that contains user secrets.
    /// This is typically a type from your entry point assembly (e.g., test project assembly)
    /// if user secrets are used for development.</param>
    /// <returns>An <see cref="IConfigurationRoot"/> instance representing the built application configuration.</returns>
    /// <remarks>
    /// This extension method configures the following sources in order:
    /// <list type="number">
    ///   <item><description>Sets the base path to the current directory using <c>Directory.GetCurrentDirectory()</c>.</description></item>
    ///   <item><description>Adds <c>appsettings.json</c> (required, reloads on change).</description></item>
    ///   <item><description>Adds <c>appsettings.{environmentName}.json</c> (optional, reloads on change).
    ///   The <c>environmentName</c> is determined by the "ASPNETCORE_ENVIRONMENT" environment variable, defaulting to "Development".</description></item>
    ///   <item><description>Adds environment variables.</description></item>
    ///   <item><description>Adds user secrets from the assembly specified by <paramref name="userSecretsAssemblyType"/> (optional).</description></item>
    /// </list>
    /// The resulting <see cref="IConfigurationRoot"/> allows access to the combined configuration values.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In your application startup or TestHost initialization:
    /// IConfiguration configuration = new ConfigurationBuilder()
    ///     .BuildApplicationConfiguration(typeof(Startup)) // or typeof(MyTestAssemblyMarker)
    ///     .Build();
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null.</exception>
    public static IConfiguration BuildApplicationConfiguration(this IConfigurationBuilder builder, Type userSecretsAssemblyType)
    {
        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        return builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(userSecretsAssemblyType.Assembly, optional: true)
            .Build();
    }
}
