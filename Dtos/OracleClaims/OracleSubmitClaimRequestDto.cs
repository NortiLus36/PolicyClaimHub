using System.ComponentModel.DataAnnotations;

namespace PolicyClaimHub.Dtos.OracleClaims;

public sealed class OracleSubmitClaimRequestDto
{
    [Required(ErrorMessage = "กรุณาระบุเลขที่เคลม")]
    [StringLength(30)]
    public string ClaimNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาระบุเลขกรมธรรม์")]
    [StringLength(30)]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาระบุทะเบียนรถ")]
    [StringLength(20)]
    public string VehicleRegistration { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาระบุวันที่เกิดเหตุ")]
    [DataType(DataType.Date)]
    public DateTime IncidentDate { get; set; }

    [Required(ErrorMessage = "กรุณาระบุเขต")]
    [StringLength(100)]
    public string District { get; set; } = string.Empty;

    [Range(typeof(decimal), "-90", "90", ErrorMessage = "Latitude ต้องอยู่ระหว่าง -90 ถึง 90")]
    public decimal Latitude { get; set; }

    [Range(typeof(decimal), "-180", "180", ErrorMessage = "Longitude ต้องอยู่ระหว่าง -180 ถึง 180")]
    public decimal Longitude { get; set; }

    [Range(typeof(decimal), "0", "500", ErrorMessage = "ระดับน้ำต้องอยู่ระหว่าง 0 ถึง 500 ซม.")]
    public decimal WaterDepthCm { get; set; }

    [Range(typeof(decimal), "0.01", "100000000", ErrorMessage = "ยอดเรียกร้องต้องมากกว่า 0")]
    public decimal RequestedAmount { get; set; }

    [Range(typeof(decimal), "0", "100000000", ErrorMessage = "ค่าเสียหายส่วนแรกต้องไม่ติดลบ")]
    public decimal DeductibleAmount { get; set; }

    [Range(typeof(decimal), "0", "100000000", ErrorMessage = "ยอดหนี้คงเหลือต้องไม่ติดลบ")]
    public decimal OutstandingDebt { get; set; }

    [Required(ErrorMessage = "กรุณาระบุผู้ทำรายการ")]
    [StringLength(100)]
    public string ChangedBy { get; set; } = "PORTFOLIO_WEB";
}
