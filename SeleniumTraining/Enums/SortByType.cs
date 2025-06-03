using System.ComponentModel.DataAnnotations;

namespace SeleniumTraining.Enums;

/// <summary>
/// Defines the criteria by which an option in a dropdown list (or similar selection UI element)
/// should be selected during web automation ([3]).
/// </summary>
/// <remarks>
/// This enumeration allows test automation scripts to specify whether a selection
/// should be made based on the visible text of an option or its underlying 'value' attribute.
/// The <see cref="DisplayAttribute"/> is used here to provide alternative string representations
/// or metadata for each member, which can be useful for logging or framework logic.
/// </remarks>
public enum SortByType
{
    /// <summary>
    /// Specifies that the dropdown option should be selected based on its visible text content.
    /// The <see cref="DisplayAttribute"/> is set to "text".
    /// </summary>
    /// <example>
    /// If a dropdown has an option like <c>&lt;option value="az"&gt;Name (A to Z)&lt;/option&gt;</c>,
    /// selecting by <c>Text</c> would use "Name (A to Z)".
    /// </example>
    [Display(Name = "text")]
    Text = 0,

    /// <summary>
    /// Specifies that the dropdown option should be selected based on the value of its 'value' attribute.
    /// The <see cref="DisplayAttribute"/> is set to "value".
    /// </summary>
    /// <example>
    /// If a dropdown has an option like <c>&lt;option value="az"&gt;Name (A to Z)&lt;/option&gt;</c>,
    /// selecting by <c>Value</c> would use "az".
    /// </example>
    [Display(Name = "value")]
    Value = 1
}
