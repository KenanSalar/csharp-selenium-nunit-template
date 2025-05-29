namespace SeleniumTraining.Utils.Settings.BrowserSettings;

public class ChromiumBasedSettings : BaseBrowserSettings
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "ChromeHeadlessArgument is required for Chromeium based browsers.")]
    public string? ChromeHeadlessArgument { get; set; } = "--headless=new";

    public List<string> ChromeArguments { get; set; } = [];
}
