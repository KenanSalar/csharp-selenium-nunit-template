namespace SeleniumTraining.Core.Services.Contracts;

public interface ITestReporterService
{
    public void InitializeTestReport(string allureDisplayName, string browserName, string correlationId);
    public void FinalizeTestReport(
        TestContext testContext,
        IWebDriver? driver,
        BrowserType browserType,
        string screenshotDirectory,
        string correlationId
    );
}
