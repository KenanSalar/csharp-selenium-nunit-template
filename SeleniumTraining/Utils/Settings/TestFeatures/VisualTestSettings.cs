namespace SeleniumTraining.Utils.Settings.TestFeatures;

public class VisualTestSettings
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "BaselineDirectory for visual tests is required.")]
    public string BaselineDirectory { get; set; } = "ProjectVisualBaselines";

    public bool AutoCreateBaselineIfMissing { get; set; } = true;

    [System.ComponentModel.DataAnnotations.Range(0, 100, ErrorMessage = "DefaultComparisonTolerancePercent must be between 0 and 100.")]
    public double DefaultComparisonTolerancePercent { get; set; } = 0.05;

    public bool WarnOnAutomaticBaselineCreation { get; set; } = true;
}
