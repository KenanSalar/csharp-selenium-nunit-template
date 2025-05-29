namespace SeleniumTraining;

[SetUpFixture]
public class AssemblyInitialize
{
    [OneTimeSetUp]
    public void GlobalSetupBeforeAllTests()
    {
        TestHost.Initialize();

        IDirectoryManagerService directoryManager = TestHost.Services.GetRequiredService<IDirectoryManagerService>();
        directoryManager.EnsureBaseDirectoriesExist();
    }

    [OneTimeTearDown]
    public void GlobalTeardownAfterAllTests()
    {
        TestHost.TearDown();
    }
}
