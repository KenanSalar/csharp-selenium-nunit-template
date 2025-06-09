namespace SeleniumTraining.Pages.Enums;

/// <summary>
/// Defines the different methods or actions that can be used to trigger a login attempt
/// within the web automation framework.
/// </summary>
/// <remarks>
/// This enumeration helps in parameterizing login actions in page objects or test steps,
/// allowing flexibility in how the login form is submitted (e.g., by simulating an Enter key press
/// on a form field or by clicking a dedicated login button).
/// </remarks>
public enum LoginMode
{
    /// <summary>
    /// Indicates that the login should be triggered by submitting the login form,
    /// typically by simulating an "Enter" key press on a form field (e.g., password input)
    /// or by calling a form's submit method if available.
    /// </summary>
    Submit = 0,

    /// <summary>
    /// Indicates that the login should be triggered by clicking a dedicated login button
    /// on the web page.
    /// </summary>
    Click = 1,
}
