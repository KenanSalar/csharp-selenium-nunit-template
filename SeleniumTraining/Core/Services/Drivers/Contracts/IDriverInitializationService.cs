namespace SeleniumTraining.Core.Services.Drivers.Contracts;

public interface IDriverInitializationService
{
    public IWebDriver InitializeDriver(BrowserType browserType, string testName, string correlationId);
}
