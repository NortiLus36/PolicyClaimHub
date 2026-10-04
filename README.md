# Policy Claim Hub

Portfolio project สำหรับเรียนรู้และสาธิตการพัฒนา ASP.NET Core MVC (.NET 10)
โดยจำลองระบบจัดการกรมธรรม์ การรับชำระเบี้ย การเคลมรถยนต์จากน้ำท่วม
และการวิเคราะห์ข้อมูลเชิงพื้นที่ด้วย GIS

> โปรเจกต์นี้ใช้ข้อมูลจำลองเพื่อการศึกษาเท่านั้น ไม่ใช่ระบบประกันภัยจริง

## เอกสารโครงการ

- [แผนการเรียนและพัฒนารายวัน](docs/DAILY-PLAN.md)
- [ขอบเขตและความสามารถของระบบ](docs/PROGRAM-SPECIFICATION.md)

## เทคโนโลยีเริ่มต้น

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 10
- SQLite สำหรับ Demo ที่เปิดใช้งานง่าย
- Bootstrap
- Leaflet สำหรับแผนที่
- Stripe Checkout Test Mode สำหรับจำลองการชำระเบี้ย
- Oracle SQL, PL/SQL และ Oracle Spatial เป็นส่วนสาธิตเพิ่มเติม
- Git สำหรับบันทึกประวัติการพัฒนา

## สถานะปัจจุบัน

- สร้างโครงการ MVC ใหม่แล้ว
- ติดตั้ง EF Core SQLite และ EF Core Design แล้ว
- ติดตั้ง `dotnet-ef` เป็น Local Tool แล้ว
- สร้าง Git repository และ `.gitignore` แล้ว
- ยังไม่ได้สร้างฟังก์ชันธุรกิจ เพื่อให้เรียนและพัฒนาทีละขั้น

## คำสั่งตรวจสอบพื้นฐาน

```powershell
dotnet tool restore
dotnet restore
dotnet build
dotnet run
```

