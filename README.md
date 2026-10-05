# Policy Claim Hub

Portfolio project สำหรับสาธิต ASP.NET Core MVC (.NET 10), RESTful API,
Business Logic, GIS Dashboard และ Oracle PL/SQL ผ่านโจทย์จำลองการจัดการ
กรมธรรม์ สินไหมรถยนต์จากน้ำท่วม และการวิเคราะห์ลูกค้าเพื่อต่ออายุกรมธรรม์

> ข้อมูล บุคคล ตำแหน่ง และสูตรคำนวณทั้งหมดเป็นข้อมูลจำลองเพื่อการศึกษา
> ไม่ใช่ระบบหรือเงื่อนไขของบริษัทประกันจริง

## ความสามารถที่ทำงานแล้ว

- MVC จัดการกรมธรรม์: List, Create, Details, Edit และ Delete
- DataAnnotations และ Server-side/Client-side Validation
- SQLite + Entity Framework Core Migrations
- Unique Index สำหรับเลขกรมธรรม์และเลขที่เคลม
- REST API แยก Request/Response DTO ออกจาก Entity
- HTTP Status `200`, `201`, `204`, `400`, `404` และ `409`
- Business Service ประเมินสินไหมตามสถานะกรมธรรม์ ช่วงคุ้มครอง และระดับน้ำ
- Flood Claim API สำหรับประเมิน บันทึก และสรุปยอดตามเขต
- Leaflet GIS Dashboard พร้อม Marker และ Flood Polygon จำลอง
- Development Seed Data แบบเพิ่มครั้งเดียวและไม่สร้างซ้ำ
- Oracle DDL, PL/SQL Package, Function, Procedures, Audit และ Spatial Query
- Product Catalog ประกันรถยนต์ 17 แผน แยกประเภท 1, 2+, 3+, 3, อื่น ๆ และ พ.ร.บ.
- ข้อมูลลูกค้าจำลอง 50 คน พร้อมข้อมูลรถ เบี้ย วันหมดอายุ และประวัติเคลม
- Rule-based Classification: Low, Standard, Watchlist และ High Risk
- Renewal Probability, เบี้ยแนะนำ และคำแนะนำสำหรับเจ้าหน้าที่
- Search/Filter Dashboard และ REST API สำหรับ Renewal Insights

## เทคโนโลยี

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 10
- SQLite
- Bootstrap 5
- Leaflet 1.9.4 + OpenStreetMap
- Oracle SQL / PL/SQL / Spatial
- Git

## เริ่มต้นใช้งาน

ข้อกำหนด: .NET 10 SDK

```powershell
git clone https://github.com/NortiLus36/PolicyClaimHub.git
cd PolicyClaimHub
dotnet tool restore
dotnet restore
dotnet ef database update
dotnet run
```

เมื่อรันใน Development ระบบจะเพิ่มผลิตภัณฑ์ ลูกค้าจำลอง 50 คน ประวัติเคลม
และข้อมูล GIS แบบ idempotent จึงรันซ้ำได้โดยไม่สร้างข้อมูลชุดเดิมซ้ำ

## หน้าสำคัญ

- `/Policies` — จัดการกรมธรรม์
- `/Products` — Product Catalog ประกันรถยนต์สำหรับทีมหลังบ้าน
- `/Portfolio` — วิเคราะห์ลูกค้า ความเสี่ยง โอกาสต่ออายุ และเบี้ยแนะนำ
- `/Dashboard` — Dashboard และแผนที่สินไหมน้ำท่วม
- `/api/policies` — REST API กรมธรรม์
- `/api/flood-claims` — รายการและบันทึกเคลมน้ำท่วม
- `/api/flood-claims/estimate` — ประเมินสินไหมโดยยังไม่บันทึก
- `/api/flood-claims/summary` — สรุปยอดสินไหมแยกตามเขต
- `/api/portfolio/renewal-insights` — ผลวิเคราะห์ลูกค้าสำหรับระบบอื่น

เปิดไฟล์ `PolicyClaimHub.http` ใน Visual Studio เพื่อทดลอง Request และตรวจ
HTTP Status Code โดยเปลี่ยน `policyId` ให้ตรงกับข้อมูลในเครื่อง

## Business Logic สินไหมจำลอง

1. กรมธรรม์ต้องเป็น `Active`
2. วันที่เกิดเหตุต้องอยู่ในช่วงความคุ้มครอง
3. ระดับน้ำกำหนดอัตราความเสียหาย 15%, 35%, 60% หรือ 85%
4. Gross Loss คือค่าต่ำสุดระหว่างยอดเรียกร้องกับทุนประกันคูณอัตราความเสียหาย
5. หัก Deductible และยอดหนี้คงเหลือ
6. ยอดสุทธิไม่ต่ำกว่า 0 และไม่เกินทุนประกัน

```text
Gross Loss = min(Requested Amount, Sum Assured × Damage Rate)
Net Estimated Payout = max(0, Gross Loss - Deductible - Outstanding Debt)
```

Logic อยู่ใน `Services/Claims/ClaimEstimationService.cs` เพื่อให้ MVC และ API
นำกลับมาใช้ร่วมกันและสามารถเขียน Unit Test แยกได้

รัน Unit Tests:

```powershell
dotnet test PolicyClaimHub.slnx --configuration Release
```

ปัจจุบันมี 11 test cases ครอบคลุมสถานะกรมธรรม์ ช่วงความคุ้มครอง
ระดับความรุนแรง การหักยอด และกฎประเมินความเสี่ยง/การต่ออายุ

## Business Logic การต่ออายุจำลอง

- ลูกค้าใหม่เริ่มด้วยคะแนนโอกาสต่ออายุต่ำกว่าลูกค้าเดิม
- สถานะกรมธรรม์ วันหมดอายุ จำนวนครั้งและยอดเคลมมีผลต่อคะแนน
- ไม่มีประวัติเคลมเป็น `Low Risk` และเสนอส่วนลดเบี้ยจำลอง 10%
- เคลมระดับเฝ้าระวังเสนอปรับเบี้ย 15%
- ความเสี่ยงสูงเสนอปรับเบี้ย 30% และส่งเจ้าหน้าที่พิจารณา
- คะแนนอยู่ในช่วง 5-95% และเป็นกฎจำลองที่อธิบายได้ ไม่ใช่ Machine Learning

ชื่อผลิตภัณฑ์รถยนต์ใช้อ้างอิงจากหน้าเว็บสาธารณะของ
[กรุงเทพประกันภัย](https://www.bangkokinsurance.com/th/product/motor1#type)
ณ วันที่ 5 ตุลาคม 2569 โดยไม่คัดลอกโปรโมชั่น ราคา รูปภาพ โลโก้
หรือข้อมูลลูกค้าจริง รายละเอียดในระบบเป็นข้อมูลจำลองเพื่อ Portfolio

## Oracle / PL/SQL

ไฟล์อยู่ใน `database/oracle` และเรียงลำดับสำหรับเปิดด้วย SQL Developer:

1. `01_schema.sql`
2. `02_seed_data.sql`
3. `03_pkg_flood_claim.sql`
4. `04_views_and_queries.sql`
5. `05_demo_calls.sql`

รายละเอียด Business Rules และวิธีตรวจ Compilation Error อยู่ใน
`database/oracle/README.md`

Oracle scripts เตรียมไว้สำหรับ Oracle Autonomous Database และต้องรันยืนยัน
กับ Oracle instance ก่อนใช้ในการสาธิตจริง

## โครงสร้างหลัก

```text
Controllers/Api        REST endpoints
Dtos                   API contracts
Models                 Database entities
Services/Claims        Business logic
Services/Renewals      Renewal scoring และ risk classification
Data                   EF Core context และ seed data
Views/Policies         MVC policy management
Views/Dashboard        GIS dashboard
Views/Portfolio        Back-office customer analytics
Migrations             SQLite schema history
database/oracle        Oracle SQL และ PL/SQL
```

## เอกสารโครงการ

- [แผนพัฒนารายวัน](docs/DAILY-PLAN.md)
- [Program Specification](docs/PROGRAM-SPECIFICATION.md)

## สิ่งที่ยังอยู่ใน Roadmap

- รันและเก็บผล Oracle scripts บน Autonomous Database จริง
- Pagination สำหรับรายการขนาดใหญ่
- Authentication/Authorization
- Payment Gateway Test Mode
- Deployment และภาพสำรองสำหรับวันสัมภาษณ์
