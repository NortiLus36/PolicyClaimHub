using System.ComponentModel.DataAnnotations;

namespace PolicyClaimHub.Models;

public enum PolicyStatus
{
    [Display(Name = "ฉบับร่าง")]
    Draft = 1,

    [Display(Name = "มีผลคุ้มครอง")]
    Active = 2,

    [Display(Name = "สิ้นสุดความคุ้มครอง")]
    Expired = 3,

    [Display(Name = "ยกเลิก")]
    Cancelled = 4
}

public enum CustomerType
{
    [Display(Name = "ลูกค้าใหม่")]
    New = 1,

    [Display(Name = "ลูกค้าเดิม")]
    Existing = 2
}

public class InsurancePolicy
{
    public int Id { get; set; }

    [Display(Name = "เลขกรมธรรม์")]
    [Required(ErrorMessage = "กรุณาระบุเลขกรมธรรม์")]
    [StringLength(
        30,
        ErrorMessage = "เลขกรมธรรม์ต้องมีความยาวไม่เกิน 30 ตัวอักษร")]
    public string PolicyNumber { get; set; } = "";

    [Display(Name = "ชื่อผู้เอาประกัน")]
    [Required(ErrorMessage = "กรุณาระบุชื่อผู้เอาประกัน")]
    [StringLength(
        150,
        ErrorMessage = "ชื่อต้องมีความยาวไม่เกิน 150 ตัวอักษร")]
    public string InsuredName { get; set; } = "";

    [Display(Name = "ประเภทลูกค้า")]
    public CustomerType CustomerType { get; set; } = CustomerType.New;

    [Display(Name = "เบอร์โทรศัพท์")]
    [StringLength(20, ErrorMessage = "เบอร์โทรศัพท์ต้องไม่เกิน 20 ตัวอักษร")]
    public string PhoneNumber { get; set; } = "";

    [Display(Name = "ผลิตภัณฑ์ประกันรถยนต์")]
    public int? MotorProductId { get; set; }

    public MotorProduct? MotorProduct { get; set; }

    [Display(Name = "ทะเบียนรถ")]
    [Required(ErrorMessage = "กรุณาระบุทะเบียนรถ")]
    [StringLength(20)]
    public string VehicleRegistration { get; set; } = "";

    [Display(Name = "ยี่ห้อรถ")]
    [Required(ErrorMessage = "กรุณาระบุยี่ห้อรถ")]
    [StringLength(50)]
    public string VehicleMake { get; set; } = "";

    [Display(Name = "รุ่นรถ")]
    [Required(ErrorMessage = "กรุณาระบุรุ่นรถ")]
    [StringLength(80)]
    public string VehicleModel { get; set; } = "";

    [Display(Name = "ปีรถ")]
    [Range(1990, 2100, ErrorMessage = "ปีรถต้องอยู่ระหว่าง 1990 ถึง 2100")]
    public int VehicleYear { get; set; } = DateTime.Today.Year;

    [Display(Name = "ทุนประกัน (บาท)")]
    [Range(
        typeof(decimal),
        "1",
        "100000000",
        ErrorMessage = "ทุนประกันต้องอยู่ระหว่าง 1 ถึง 100,000,000 บาท")]
    public decimal SumAssured { get; set; }

    [Display(Name = "เบี้ยประกัน (บาท)")]
    [Range(
        typeof(decimal),
        "1",
        "10000000",
        ErrorMessage = "เบี้ยประกันต้องอยู่ระหว่าง 1 ถึง 10,000,000 บาท")]
    public decimal PremiumAmount { get; set; }

    [Display(Name = "วันที่เริ่มคุ้มครอง")]
    [DataType(DataType.Date)]
    public DateTime CoverageStartDate { get; set; } = DateTime.Today;

    [Display(Name = "วันที่สิ้นสุดความคุ้มครอง")]
    [DataType(DataType.Date)]
    public DateTime CoverageEndDate { get; set; } = DateTime.Today.AddYears(1);

    [Display(Name = "สถานะกรมธรรม์")]
    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    public ICollection<ClaimHistory> ClaimHistories { get; set; }
        = new List<ClaimHistory>();
}
