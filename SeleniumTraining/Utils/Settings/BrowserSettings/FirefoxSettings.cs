namespace SeleniumTraining.Utils.Settings.BrowserSettings;

public class FirefoxSettings : BaseBrowserSettings
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "FirefoxHeadlessArgument is required for Firefox.")]
    public string? FirefoxHeadlessArgument { get; set; } = "--headless";

    public List<string> FirefoxArguments { get; set; } = [];
}
