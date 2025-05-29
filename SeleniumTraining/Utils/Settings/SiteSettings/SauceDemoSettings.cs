namespace SeleniumTraining.Utils.Settings.SiteSettings;

public class SauceDemoSettings
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo PageUrl is required.")]
    [Url(ErrorMessage = "SauceDemo PageUrl must be a valid absolute URL (e.g., http://example.com).")]
    public required string PageUrl { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameStandardUser is required.")]
    public required string LoginUsernameStandardUser { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameLockedOutUser is required.")]
    public required string LoginUsernameLockedOutUser { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameProblemUser is required.")]
    public required string LoginUsernameProblemUser { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernamePerformanceGlitchUser is required.")]
    public required string LoginUsernamePerformanceGlitchUser { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameErrorUser is required.")]
    public required string LoginUsernameErrorUser { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginUsernameVisualUser is required.")]
    public required string LoginUsernameVisualUser { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "SauceDemo LoginPassword is required.")]
    public required string LoginPassword { get; set; }
}
