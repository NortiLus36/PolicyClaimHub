using System.ComponentModel.DataAnnotations;

namespace PolicyClaimHub.Models;

public enum FloodClaimStatus
{
    [Display(Name = "ฉบับร่าง")]
    Draft = 1,

    [Display(Name = "ยื่นคำร้องแล้ว")]
    Submitted = 2,

    [Display(Name = "กำลังตรวจสอบ")]
    UnderReview = 3,

    [Display(Name = "อนุมัติ")]
    Approved = 4,

    [Display(Name = "ปฏิเสธ")]
    Rejected = 5
}

public class MotorFloodClaim
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string ClaimNumber { get; set; } = string.Empty;

    public int InsurancePolicyId { get; set; }

    public InsurancePolicy? InsurancePolicy { get; set; }

    [Required]
    [StringLength(20)]
    public string VehicleRegistration { get; set; } = string.Empty;

    public DateTime IncidentDate { get; set; }

    [Required]
    [StringLength(100)]
    public string District { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public decimal WaterDepthCm { get; set; }

    public decimal RequestedAmount { get; set; }

    public decimal DeductibleAmount { get; set; }

    public decimal OutstandingDebt { get; set; }

    public decimal EstimatedPayout { get; set; }

    public FloodClaimStatus Status { get; set; } = FloodClaimStatus.Submitted;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
