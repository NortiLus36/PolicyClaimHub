using System.ComponentModel.DataAnnotations;
using PolicyClaimHub.Models;

namespace PolicyClaimHub.Dtos.Policies;

public sealed class PolicyRequestDto
{
    [Required(ErrorMessage = "กรุณาระบุเลขกรมธรรม์")]
    [StringLength(30, ErrorMessage = "เลขกรมธรรม์ต้องมีความยาวไม่เกิน 30 ตัวอักษร")]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาระบุชื่อผู้เอาประกัน")]
    [StringLength(150, ErrorMessage = "ชื่อต้องมีความยาวไม่เกิน 150 ตัวอักษร")]
    public string InsuredName { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "100000000",
        ErrorMessage = "ทุนประกันต้องอยู่ระหว่าง 1 ถึง 100,000,000 บาท")]
    public decimal SumAssured { get; set; }

    [Range(typeof(decimal), "1", "10000000",
        ErrorMessage = "เบี้ยประกันต้องอยู่ระหว่าง 1 ถึง 10,000,000 บาท")]
    public decimal PremiumAmount { get; set; }

    public DateTime CoverageStartDate { get; set; }

    public DateTime CoverageEndDate { get; set; }

    [EnumDataType(typeof(PolicyStatus), ErrorMessage = "สถานะกรมธรรม์ไม่ถูกต้อง")]
    public PolicyStatus Status { get; set; }
}
