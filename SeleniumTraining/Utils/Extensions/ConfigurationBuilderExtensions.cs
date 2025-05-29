namespace SeleniumTraining.Utils.Extensions;

public static class ConfigurationBuilderExtensions
{
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
