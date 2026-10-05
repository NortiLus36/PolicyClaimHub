using Microsoft.EntityFrameworkCore;
using PolicyClaimHub.Models;
using PolicyClaimHub.Services.Claims;

namespace PolicyClaimHub.Data;

public static class DbSeeder
{
    private const string PortfolioPrefix = "PORT-2026-";

    public static async Task SeedAsync(
        ApplicationDbContext context,
        IClaimEstimationService estimationService)
    {
        await SeedProductsAsync(context);
        await SeedPortfolioPoliciesAsync(context);
        await SeedClaimHistoriesAsync(context);
        await SeedFloodClaimsAsync(context, estimationService);
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        if (await context.MotorProducts.AnyAsync()) return;

        context.MotorProducts.AddRange(
            Product("T1-REGION", "ประเภท 1 Motor by Region", MotorProductClass.Type1, "แผนประเภท 1 ที่กำหนดเบี้ยตามภูมิภาค"),
            Product("T1-GENX", "ประเภท 1 Motor Gen X", MotorProductClass.Type1, "แผนประเภท 1 สำหรับกลุ่มผู้ขับขี่ช่วงวัยทำงาน"),
            Product("T1-USED", "ประเภท 1 Used Car Special", MotorProductClass.Type1, "แผนประเภท 1 สำหรับรถยนต์มือสอง"),
            Product("T1-VALUE", "ประเภท 1 คุ้มครอง-คุ้มค่า", MotorProductClass.Type1, "แผนสำหรับผู้มีประวัติการขับขี่ดีและมี Deductible"),
            Product("T1-LUXURY", "ประเภท 1 Mercedes-Benz/BMW", MotorProductClass.Type1, "แผนประเภท 1 สำหรับรถยนต์ Mercedes-Benz และ BMW"),
            Product("T1-DEALER", "ประเภท 1 ซ่อมศูนย์ 10 ปี", MotorProductClass.Type1, "แผนประเภท 1 รองรับการซ่อมศูนย์ตัวแทนจำหน่าย"),
            Product("T1-SENIOR", "ประเภท 1 อุ่นใจวัยเก๋า", MotorProductClass.Type1, "แผนประเภท 1 สำหรับผู้ขับขี่อายุ 55-75 ปี"),
            Product("T1-FLEX", "ประเภท 1 เลือกทุนเองได้", MotorProductClass.Type1, "แผนที่ลูกค้าเลือกทุนประกันภัยได้"),
            Product("T1-TRUCK", "Truck We Insure", MotorProductClass.Type1, "แผนประเภท 1 สำหรับรถบรรทุกขนาดใหญ่"),
            Product("T2P-SPECIAL", "2+ Special โดนใจ", MotorProductClass.Type2Plus, "แผน 2+ สำหรับรถยนต์นั่งและรถกระบะ"),
            Product("T3P-SUPER", "3+ Super Special", MotorProductClass.Type3Plus, "แผน 3+ พร้อมความคุ้มครองภัยน้ำท่วม"),
            Product("T3P-SPECIAL", "3+ Special", MotorProductClass.Type3Plus, "แผน 3+ คุ้มครองกรณีชนกับยานพาหนะทางบก"),
            Product("T3-EXTRA", "ประเภท 3 Extra", MotorProductClass.Type3, "แผนประเภท 3 พร้อมความคุ้มครองเพิ่มเติม"),
            Product("T3-SAVE", "ประเภท 3 Super Save", MotorProductClass.Type3, "แผนประเภท 3 เน้นเบี้ยประหยัด"),
            Product("FLEET", "ประกันภัยรถยนต์กลุ่ม (Fleet)", MotorProductClass.Other, "แผนสำหรับองค์กรที่มีรถหลายคัน"),
            Product("MOTOR-TOPUP", "Motor Top Up", MotorProductClass.Other, "แผนชดเชยผลประโยชน์จากอุบัติเหตุการใช้รถยนต์"),
            Product("CTP", "ประกันภัยรถยนต์ภาคบังคับ (พ.ร.บ.)", MotorProductClass.Compulsory, "ความคุ้มครองภาคบังคับตามกฎหมาย"));

        await context.SaveChangesAsync();
    }

    private static async Task SeedPortfolioPoliciesAsync(ApplicationDbContext context)
    {
        if (await context.InsurancePolicies.AnyAsync(p => p.PolicyNumber.StartsWith(PortfolioPrefix))) return;

        var products = await context.MotorProducts.OrderBy(p => p.Id).ToListAsync();
        var firstNames = new[] { "กิตติพงษ์", "วราภรณ์", "ธนกร", "พิมพ์ชนก", "ณัฐวุฒิ", "สุภาวดี", "ปกรณ์", "ชลธิชา", "อภิสิทธิ์", "กัญญารัตน์" };
        var lastNames = new[] { "สุขใจ", "มั่นคง", "รุ่งเรือง", "ศรีนคร", "วงศ์วัฒนา" };
        var makes = new[] { "Toyota", "Honda", "Isuzu", "Mazda", "Nissan" };
        var models = new[] { "Corolla Cross", "City", "D-Max", "CX-30", "Kicks" };
        var today = DateTime.Today;

        for (var index = 1; index <= 50; index++)
        {
            var product = products[(index - 1) % products.Count];
            var endDate = today.AddDays(-120 + ((index * 17) % 300));
            var fullName = $"{firstNames[(index - 1) % firstNames.Length]} {lastNames[((index - 1) / firstNames.Length) % lastNames.Length]}";

            context.InsurancePolicies.Add(new InsurancePolicy
            {
                PolicyNumber = $"{PortfolioPrefix}{index:000}",
                InsuredName = fullName,
                CustomerType = index % 4 == 0 ? CustomerType.New : CustomerType.Existing,
                PhoneNumber = $"08{(10000000 + index * 7919):00000000}",
                MotorProductId = product.Id,
                VehicleRegistration = $"{ThaiLetter(index)}-{1000 + index}",
                VehicleMake = makes[(index - 1) % makes.Length],
                VehicleModel = models[(index - 1) % models.Length],
                VehicleYear = 2015 + (index % 11),
                SumAssured = 250_000m + ((index * 75_000m) % 1_250_000m),
                PremiumAmount = 6_000m + ((index * 1_375m) % 28_000m),
                CoverageStartDate = endDate.AddYears(-1),
                CoverageEndDate = endDate,
                Status = endDate < today ? PolicyStatus.Expired : PolicyStatus.Active
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedClaimHistoriesAsync(ApplicationDbContext context)
    {
        if (await context.ClaimHistories.AnyAsync()) return;

        var policies = await context.InsurancePolicies
            .Where(p => p.PolicyNumber.StartsWith(PortfolioPrefix))
            .OrderBy(p => p.PolicyNumber)
            .ToListAsync();
        var claimTypes = new[] { ClaimType.Collision, ClaimType.Fire, ClaimType.Flood, ClaimType.Theft };
        var runningNumber = 1;

        for (var index = 0; index < policies.Count; index++)
        {
            for (var claimIndex = 0; claimIndex < index % 6; claimIndex++)
            {
                var claimType = claimTypes[(index + claimIndex) % claimTypes.Length];
                var paidAmount = claimType switch
                {
                    ClaimType.Fire => 180_000m + (index * 4_500m),
                    ClaimType.Flood => 90_000m + (index * 3_250m),
                    ClaimType.Theft => 220_000m + (index * 5_000m),
                    _ => 25_000m + (claimIndex * 18_000m) + (index * 750m)
                };

                context.ClaimHistories.Add(new ClaimHistory
                {
                    ClaimNumber = $"HIS-2026-{runningNumber++:0000}",
                    InsurancePolicyId = policies[index].Id,
                    ClaimType = claimType,
                    IncidentDate = DateTime.Today.AddDays(-(45 + index * 5 + claimIndex * 30)),
                    PaidAmount = paidAmount,
                    Note = "ข้อมูลจำลองสำหรับวิเคราะห์การต่ออายุ"
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedFloodClaimsAsync(ApplicationDbContext context, IClaimEstimationService estimationService)
    {
        if (await context.MotorFloodClaims.AnyAsync()) return;

        var policies = await context.InsurancePolicies
            .Where(p => p.PolicyNumber.StartsWith(PortfolioPrefix) && p.Status == PolicyStatus.Active)
            .Take(4)
            .ToListAsync();
        var seeds = new[]
        {
            new FloodSeed("DEMO-CLM-001", "บางเขน", 13.8739, 100.5964, 65, 620_000, 10_000, 20_000),
            new FloodSeed("DEMO-CLM-002", "ดอนเมือง", 13.9133, 100.6042, 110, 790_000, 15_000, 0),
            new FloodSeed("DEMO-CLM-003", "หลักสี่", 13.8872, 100.5795, 42, 410_000, 10_000, 12_000),
            new FloodSeed("DEMO-CLM-004", "บางเขน", 13.8516, 100.6250, 85, 550_000, 10_000, 5_000)
        };

        for (var index = 0; index < Math.Min(seeds.Length, policies.Count); index++)
        {
            var seed = seeds[index];
            var policy = policies[index];
            var incidentDate = policy.CoverageStartDate.AddDays(30);
            var input = new ClaimEstimateInput(incidentDate, seed.WaterDepthCm, seed.RequestedAmount, seed.DeductibleAmount, seed.OutstandingDebt);
            var estimate = estimationService.Estimate(policy, input);

            context.MotorFloodClaims.Add(new MotorFloodClaim
            {
                ClaimNumber = seed.ClaimNumber,
                InsurancePolicyId = policy.Id,
                VehicleRegistration = policy.VehicleRegistration,
                IncidentDate = incidentDate,
                District = seed.District,
                Latitude = seed.Latitude,
                Longitude = seed.Longitude,
                WaterDepthCm = seed.WaterDepthCm,
                RequestedAmount = seed.RequestedAmount,
                DeductibleAmount = seed.DeductibleAmount,
                OutstandingDebt = seed.OutstandingDebt,
                EstimatedPayout = estimate.NetEstimatedPayout,
                Status = FloodClaimStatus.Submitted
            });
        }

        await context.SaveChangesAsync();
    }

    private static MotorProduct Product(string code, string name, MotorProductClass productClass, string description) =>
        new() { Code = code, Name = name, ProductClass = productClass, Description = description };

    private static string ThaiLetter(int index)
    {
        var letters = new[] { "กข", "ขค", "คง", "งจ", "จฉ", "ฉช", "ชซ", "ซญ", "ญฎ", "ฎฐ" };
        return letters[(index - 1) % letters.Length];
    }

    private sealed record FloodSeed(string ClaimNumber, string District, double Latitude, double Longitude,
        decimal WaterDepthCm, decimal RequestedAmount, decimal DeductibleAmount, decimal OutstandingDebt);
}
