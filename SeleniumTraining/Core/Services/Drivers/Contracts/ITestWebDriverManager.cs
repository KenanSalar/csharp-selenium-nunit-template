namespace SeleniumTraining.Core.Services.Drivers.Contracts;

public interface ITestWebDriverManager : IDisposable
{
    public void InitializeDriver(BrowserType browserType, string testName, string correlationId);
    public IWebDriver GetDriver();
    public void QuitDriver();
    public bool IsDriverActive { get; }
}
