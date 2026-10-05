using System.ComponentModel.DataAnnotations;

namespace PolicyClaimHub.Models;

public enum MotorProductClass
{
    [Display(Name = "ประเภท 1")]
    Type1 = 1,

    [Display(Name = "ประเภท 2+")]
    Type2Plus = 2,

    [Display(Name = "ประเภท 3+")]
    Type3Plus = 3,

    [Display(Name = "ประเภท 3")]
    Type3 = 4,

    [Display(Name = "ประเภทอื่น")]
    Other = 5,

    [Display(Name = "พ.ร.บ.")]
    Compulsory = 6
}

public class MotorProduct
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    public MotorProductClass ProductClass { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
