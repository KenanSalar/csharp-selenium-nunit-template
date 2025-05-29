using System.ComponentModel.DataAnnotations;

namespace SeleniumTraining.Enums;

public enum SortByType
{
    [Display(Name = "text")]
    Text = 0,

    [Display(Name = "value")]
    Value = 1
}
