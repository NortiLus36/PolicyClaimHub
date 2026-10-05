using System.ComponentModel.DataAnnotations;

namespace PolicyClaimHub.Models;

public enum ClaimType
{
    [Display(Name = "รถชน")]
    Collision = 1,

    [Display(Name = "ไฟไหม้")]
    Fire = 2,

    [Display(Name = "น้ำท่วม")]
    Flood = 3,

    [Display(Name = "รถสูญหาย")]
    Theft = 4,

    [Display(Name = "อื่น ๆ")]
    Other = 5
}

public class ClaimHistory
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string ClaimNumber { get; set; } = string.Empty;

    public int InsurancePolicyId { get; set; }

    public InsurancePolicy? InsurancePolicy { get; set; }

    public ClaimType ClaimType { get; set; }

    public DateTime IncidentDate { get; set; }

    public decimal PaidAmount { get; set; }

    [StringLength(250)]
    public string Note { get; set; } = string.Empty;
}
