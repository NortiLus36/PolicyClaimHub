# PolicyClaimHub — Interview Demo Guide

## เตรียมก่อนเริ่ม

```powershell
dotnet tool restore
dotnet ef database update
dotnet test PolicyClaimHub.slnx --configuration Release
dotnet run
```

เปิดล่วงหน้า:

- `/Policies`
- `/Portfolio`
- `/Dashboard`
- `PolicyClaimHub.http`
- `Services/Claims/ClaimEstimationService.cs`
- `database/oracle/03_pkg_flood_claim.sql`
- GitHub Commit History

## ลำดับ Demo 5 นาที

### 1. โจทย์ธุรกิจ (30 วินาที)

ระบบจำลองการจัดการกรมธรรม์และประเมินภาระสินไหมรถยนต์จากน้ำท่วม
โดยเชื่อมข้อมูลกรมธรรม์ Business Rules พิกัด GIS และฐานข้อมูล Oracle

### 2. MVC และ Validation (45 วินาที)

1. เปิดหน้ารายการกรมธรรม์
2. เพิ่มกรมธรรม์หนึ่งรายการ
3. ทดลองวันที่สิ้นสุดก่อนวันที่เริ่มเพื่อแสดง Validation
4. เปิด Details/Edit และอธิบาย MVC Request Flow

### 3. REST API และ DTO (60 วินาที)

ใช้ `PolicyClaimHub.http` เรียก:

- `GET /api/policies` → `200 OK`
- `POST /api/policies` → `201 Created`
- ข้อมูลผิด → `400 Bad Request`
- เลขซ้ำ → `409 Conflict`
- Id ไม่มีอยู่ → `404 Not Found`

อธิบายว่า DTO ป้องกันการเปิดเผย Entity และควบคุม Contract ของ API

### 4. Customer Analytics (60 วินาที)

เปิด `/Portfolio` แล้วสาธิต:

- ลูกค้าใหม่/ลูกค้าเดิมและผลิตภัณฑ์รถยนต์
- ประวัติเคลมรถชน ไฟไหม้ น้ำท่วม และรถสูญหาย
- การจัดกลุ่ม Low/Standard/Watchlist/High Risk
- Renewal Probability และเบี้ยแนะนำ
- Search/Filter และ `GET /api/portfolio/renewal-insights`

ย้ำว่าเป็น Rule-based Simulation ที่อธิบายได้ ไม่ใช่ AI หรือกฎบริษัทจริง

### 5. Business Service (45 วินาที)

เปิด `ClaimEstimationService` และอธิบาย:

- ตรวจสถานะกรมธรรม์
- ตรวจช่วงคุ้มครอง
- เลือก Damage Rate ตามระดับน้ำ
- หัก Deductible และหนี้
- จำกัดผลลัพธ์ไม่เกินทุนประกัน
- Logic แยกจาก Controller และมี Unit Tests รวม 11 cases

### 6. GIS Dashboard (60 วินาที)

เปิด `/Dashboard` แล้วชี้ให้เห็น:

- Marker จุดเกิดเหตุ
- Polygon ผลวิเคราะห์/คาดการณ์น้ำท่วมที่ตัดกับกรุงเทพฯ จาก GISTDA GFlood
- จุดน้ำท่วมถนนล่าสุดของกรุงเทพฯ จาก NOW Bangkok พร้อมระดับน้ำ เขต และเวลาตรวจวัด
- Layer พื้นที่ประสบภัยปัจจุบันจาก GISTDA พร้อมสถานะการเชื่อมต่อ
- Layer พื้นที่เสี่ยงภัยน้ำท่วมจาก GISTDA แยกสีและความหมายจากเหตุปัจจุบัน
- Polygon พื้นที่น้ำท่วมจำลองเป็น fallback เมื่อ Live API ไม่มีข้อมูล
- สี Marker ตามระดับน้ำ
- ยอดเรียกร้องและยอดประมาณการรวม
- Summary แยกตามเขตจาก API

### 7. Oracle PL/SQL (60 วินาที)

เปิด `PKG_FLOOD_CLAIM` ใน SQL Developer แล้วอธิบาย:

- Function คำนวณยอดประเมิน
- Procedure ยื่นเคลมพร้อมตรวจสถานะและช่วงคุ้มครอง
- Procedure อนุมัติด้วย `SELECT ... FOR UPDATE`
- `RAISE_APPLICATION_ERROR` สำหรับ Business Error
- Audit History และ Caller เป็นผู้ควบคุม `COMMIT/ROLLBACK`
- `SDO_RELATE` ตรวจ Point อยู่ภายใน Flood Polygon

## คำตอบสั้นที่ควรเตรียม

### MVC Controller ต่างจาก API Controller อย่างไร

MVC คืน View/HTML ให้ Browser ส่วน API คืนข้อมูลและ HTTP Status Code
สำหรับ Client เช่น JavaScript, Mobile App หรือระบบอื่น

### Entity ต่างจาก DTO อย่างไร

Entity สะท้อนโครงสร้างข้อมูลภายใน ส่วน DTO เป็น Contract ภายนอก
ช่วยจำกัดฟิลด์ ป้องกัน Overposting และเปลี่ยน API โดยไม่ผูกกับ Database

### ทำไมใช้ SQLite และ Oracle ร่วมกัน

SQLite ทำให้ Clone และ Demo ได้ง่าย ส่วน Oracle scripts แสดงการออกแบบ
PL/SQL, Transaction, Audit และ Spatial สำหรับบริบทระบบองค์กร

## ข้อจำกัดที่ต้องพูดอย่างตรงไปตรงมา

- Flood Polygon และข้อมูลลูกค้าเป็นข้อมูลจำลอง
- สูตรคำนวณไม่ใช่เงื่อนไขประกันจริง
- Oracle scripts ต้องรันยืนยันบน Autonomous Database ก่อนวัน Demo
- ยังไม่มี Authentication และ Payment Gateway ใน MVP รอบสัมภาษณ์
