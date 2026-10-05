using System.ComponentModel.DataAnnotations;

namespace PolicyClaimHub.Dtos.FloodClaims;

public sealed class CreateFloodClaimRequestDto : FloodClaimEstimateRequestDto
{
    [Required(ErrorMessage = "กรุณาระบุเลขที่เคลม")]
    [StringLength(30)]
    public string ClaimNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาระบุทะเบียนรถ")]
    [StringLength(20)]
    public string VehicleRegistration { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาระบุเขต")]
    [StringLength(100)]
    public string District { get; set; } = string.Empty;

    [Range(-90, 90, ErrorMessage = "Latitude ต้องอยู่ระหว่าง -90 ถึง 90")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude ต้องอยู่ระหว่าง -180 ถึง 180")]
    public double Longitude { get; set; }
}
