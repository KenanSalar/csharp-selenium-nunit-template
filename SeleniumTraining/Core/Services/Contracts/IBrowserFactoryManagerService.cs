namespace SeleniumTraining.Core.Services.Contracts;

public interface IBrowserFactoryManagerService
{
    public IWebDriver UseBrowserDriver(BrowserType browserType, BaseBrowserSettings settings, DriverOptions? options = null);
}
