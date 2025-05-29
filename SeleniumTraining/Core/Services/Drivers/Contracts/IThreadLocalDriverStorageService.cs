namespace SeleniumTraining.Core.Services.Drivers.Contracts;

public interface IThreadLocalDriverStorageService : IDisposable
{
    public void SetDriverContext(IWebDriver driver, string testName, string correlationId);
    public IWebDriver GetDriver();
    public string GetTestName();
    public string GetCorrelationId();
    public bool IsDriverInitialized();
    public void ClearDriverContext();
}
