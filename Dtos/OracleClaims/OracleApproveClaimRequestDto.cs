using System.ComponentModel.DataAnnotations;

namespace PolicyClaimHub.Dtos.OracleClaims;

public sealed class OracleApproveClaimRequestDto
{
    [Range(typeof(decimal), "0", "100000000", ErrorMessage = "ยอดอนุมัติต้องไม่ติดลบ")]
    public decimal ApprovedAmount { get; set; }

    [Required(ErrorMessage = "กรุณาระบุผู้อนุมัติ")]
    [StringLength(100)]
    public string ChangedBy { get; set; } = "PORTFOLIO_APPROVER";

    [StringLength(500)]
    public string? ChangeNote { get; set; }
}
