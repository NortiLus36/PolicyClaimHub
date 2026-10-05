using System.ComponentModel.DataAnnotations;

namespace PolicyClaimHub.Dtos.FloodClaims;

public class FloodClaimEstimateRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "กรุณาระบุกรมธรรม์")]
    public int PolicyId { get; set; }

    public DateTime IncidentDate { get; set; }

    [Range(typeof(decimal), "0", "500",
        ErrorMessage = "ระดับน้ำต้องอยู่ระหว่าง 0 ถึง 500 เซนติเมตร")]
    public decimal WaterDepthCm { get; set; }

    [Range(typeof(decimal), "1", "100000000",
        ErrorMessage = "ยอดเรียกร้องต้องมากกว่า 0")]
    public decimal RequestedAmount { get; set; }

    [Range(typeof(decimal), "0", "100000000",
        ErrorMessage = "ค่าเสียหายส่วนแรกต้องไม่ติดลบ")]
    public decimal DeductibleAmount { get; set; }

    [Range(typeof(decimal), "0", "100000000",
        ErrorMessage = "ยอดหนี้คงเหลือต้องไม่ติดลบ")]
    public decimal OutstandingDebt { get; set; }
}
