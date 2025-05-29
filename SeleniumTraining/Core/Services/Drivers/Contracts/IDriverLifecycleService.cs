namespace SeleniumTraining.Core.Services.Drivers.Contracts;

public interface IDriverLifecycleService
{
    public void QuitDriver(IWebDriver driver, string testClassName, string correlationId);
}
