namespace SeleniumTraining.Utils.Settings.SiteSettings;

/// <summary>
/// Defines application-specific settings for interacting with the SauceDemo website as an immutable record.
/// This includes the base URL for the site and various sets of login credentials for different test user types.
/// </summary>
/// <remarks>
/// This record is bound to the "SauceDemo" configuration section (e.g., in appsettings.json).
/// As an immutable record with `init`-only properties, its state is fixed after being loaded, preventing accidental changes during runtime.
/// All properties are marked as <c>required</c> and validated using data annotations to ensure that
/// necessary configuration values are present and valid when loaded.
/// </remarks>
public record SauceDemoSettings
{
    /// <summary>
    /// Gets or sets the base URL for the SauceDemo application (e.g., "https://www.saucedemo.com").
    /// </summary>
    /// <value>
    /// The required page URL. Must be a valid absolute URL.
    /// </value>
    /// <remarks>
    /// This property is validated by <see cref="RequiredAttribute"/> (cannot be empty) and
    /// <see cref="UrlAttribute"/> (must be a well-formed absolute URL).
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo PageUrl is required.")]
    [Url(ErrorMessage = "SauceDemo PageUrl must be a valid absolute URL (e.g., http://example.com).")]
    public required string PageUrl { get; init; }

    /// <summary>
    /// Gets or sets the username for the standard, successfully logging-in user on SauceDemo.
    /// </summary>
    /// <value>
    /// The required username for the standard user.
    /// </value>
    /// <remarks>
    /// This property is validated by <see cref="RequiredAttribute"/> (cannot be empty).
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameStandardUser is required.")]
    public required string LoginUsernameStandardUser { get; init; }

    /// <summary>
    /// Gets or sets the username for a user who is "locked out" and cannot log in to SauceDemo.
    /// </summary>
    /// <value>
    /// The required username for the locked-out user.
    /// </value>
    /// <remarks>
    /// This property is validated by <see cref="RequiredAttribute"/> (cannot be empty).
    /// Used for testing locked-out user scenarios.
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameLockedOutUser is required.")]
    public required string LoginUsernameLockedOutUser { get; init; }

    /// <summary>
    /// Gets or sets the username for a "problem user" on SauceDemo, who might exhibit
    /// issues with the application's functionality (e.g., broken images, slow performance).
    /// </summary>
    /// <value>
    /// The required username for the problem user.
    /// </value>
    /// <remarks>
    /// This property is validated by <see cref="RequiredAttribute"/> (cannot be empty).
    /// Used for testing how the application handles problematic user scenarios or data.
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameProblemUser is required.")]
    public required string LoginUsernameProblemUser { get; init; }

    /// <summary>
    /// Gets or sets the username for a "performance glitch user" on SauceDemo, who experiences
    /// intentional performance delays in the application.
    /// </summary>
    /// <value>
    /// The required username for the performance glitch user.
    /// </value>
    /// <remarks>
    /// This property is validated by <see cref="RequiredAttribute"/> (cannot be empty).
    /// Used for testing application behavior under simulated performance issues or for performance testing itself.
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernamePerformanceGlitchUser is required.")]
    public required string LoginUsernamePerformanceGlitchUser { get; init; }

    /// <summary>
    /// Gets or sets the username for an "error user" on SauceDemo.
    /// This user type might encounter specific errors or edge cases within the application.
    /// </summary>
    /// <value>
    /// The required username for the error user.
    /// </value>
    /// <remarks>
    /// This property is validated by <see cref="RequiredAttribute"/> (cannot be empty).
    /// Used for testing specific error handling paths in the application.
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameErrorUser is required.")]
    public required string LoginUsernameErrorUser { get; init; }

    /// <summary>
    /// Gets or sets the username for a "visual user" on SauceDemo.
    /// This user type might be used to test pages or scenarios where specific visual
    /// anomalies or states are expected or being tested.
    /// </summary>
    /// <value>
    /// The required username for the visual user.
    /// </value>
    /// <remarks>
    /// This property is validated by <see cref="RequiredAttribute"/> (cannot be empty).
    /// Often used in conjunction with visual regression testing tools.
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameVisualUser is required.")]
    public required string LoginUsernameVisualUser { get; init; }

    /// <summary>
    /// Gets or sets the common password used for all SauceDemo test users.
    /// </summary>
    /// <value>
    /// The required password for all SauceDemo test users.
    /// </value>
    /// <remarks>
    /// This property is validated by <see cref="RequiredAttribute"/> (cannot be empty).
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginPassword is required.")]
    public required string LoginPassword { get; init; }
}
