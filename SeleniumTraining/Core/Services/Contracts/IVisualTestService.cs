using SixLabors.ImageSharp;

namespace SeleniumTraining.Core.Services.Contracts;

public interface IVisualTestService
{
    public void AssertVisualMatch(
        string baselineIdentifier,
        string testName,
        BrowserType browserType,
        IWebElement? elementToCapture = null,
        Rectangle? cropArea = null,
        double? tolerancePercent = null
    );
}
