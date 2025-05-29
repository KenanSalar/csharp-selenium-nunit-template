namespace SeleniumTraining.Pages.SauceDemo.ElementMap;

public static class LoginPageMap
{
    public static readonly string PageTitle = "Swag Labs";
    public static By UsernameInput => By.Id("user-name");
    public static By PasswordInput => By.Id("password");
    public static By LoginButton => By.Id("login-button");
    public static By ErrorMessageContainer => By.CssSelector("h3[data-test='error']");

    public static By[] LoginPageElements { get; } = [
        UsernameInput,
        PasswordInput,
        LoginButton
    ];
}
