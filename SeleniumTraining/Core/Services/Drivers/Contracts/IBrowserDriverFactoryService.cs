namespace SeleniumTraining.Core.Services.Drivers.Contracts;

public interface IBrowserDriverFactoryService
{
    public BrowserType Type { get; }
    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null);
}
