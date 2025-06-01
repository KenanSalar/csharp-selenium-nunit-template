namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

public static class LoginPageMap
{
    public static readonly string PageTitle = "Swag Labs";
    public static By UsernameInput => SmartLocators.DataTest("username");
    public static By PasswordInput => SmartLocators.DataTest("password");
    public static By LoginButton => SmartLocators.DataTest("login-button");
    public static By ErrorMessageContainer => SmartLocators.DataTest("error");

    public static By[] LoginPageElements { get; } = [
        UsernameInput,
        PasswordInput,
        LoginButton
    ];
}
