namespace SeleniumTraining.Core.Services.Contracts;

public interface ISettingsProviderService
{
    public IConfiguration Configuration { get; }

    [AllureStep("Retrieving browser settings")]
    public BaseBrowserSettings GetBrowserSettings(BrowserType browserType);

    [AllureStep("Retrieving settings for section: {sectionName}")]
    public TClassSite GetSettings<TClassSite>(string sectionName) where TClassSite : class;
}
