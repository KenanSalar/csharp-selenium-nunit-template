namespace SeleniumTraining.Utils.Settings.BrowserSettings;

public abstract class BaseBrowserSettings
{
    public bool Headless { get; set; }

    [System.ComponentModel.DataAnnotations.Range(0, 180, ErrorMessage = "TimeoutSeconds must be between 0 and 300.")]
    public int TimeoutSeconds { get; set; } = 10;

    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "WindowWidth, if specified, must be a positive integer.")]
    public int? WindowWidth { get; set; }

    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "WindowHeight, if specified, must be a positive integer.")]
    public int? WindowHeight { get; set; }

    public bool LeaveBrowserOpenAfterTest { get; set; }
}
