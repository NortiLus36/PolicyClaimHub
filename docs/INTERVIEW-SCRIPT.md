# PolicyClaimHub — สคริปต์พรีเซนต์สำหรับสัมภาษณ์

## ก่อนเริ่ม Demo

เปิดหน้า Home, `/Policies`, `/Portfolio`, `/Dashboard`, `/OracleClaims`,
`PolicyClaimHub.http`, Package ฉบับคอมเมนต์, Oracle SQL Worksheet และ GitHub
รอไว้ก่อน จากนั้น Build, Test และ Run รอบสุดท้าย

## สคริปต์หลัก 5–7 นาที

### 1. ปัญหาและเป้าหมาย — 30 วินาที

> โปรเจกต์นี้ชื่อ PolicyClaimHub เป็นระบบจำลองงานหลังบ้านประกันภัยรถยนต์
> ผมเริ่มจากเหตุการณ์น้ำท่วมในกรุงเทพฯ ซึ่งทำให้มีรถยนต์เสียหายพร้อมกันจำนวนมาก
> จึงออกแบบระบบที่เชื่อมข้อมูลกรมธรรม์ การประเมินสินไหม พิกัด GIS
> การวิเคราะห์ลูกค้า REST API และ Oracle PL/SQL เข้าด้วยกัน
> ข้อมูลและสูตรทั้งหมดเป็นข้อมูลจำลองเพื่อ Portfolio ไม่ใช่กฎบริษัทจริงครับ

### 2. MVC และ CRUD — 45 วินาที

เปิด `/Policies`

> ส่วนแรกเป็น ASP.NET Core MVC สำหรับจัดการกรมธรรม์ สามารถดูรายการ เพิ่ม
> ดูรายละเอียด แก้ไข และลบได้ ผมใช้ Entity Framework Core กับ SQLite
> เพื่อให้ Clone แล้วรัน Demo ได้ง่าย หน้า List มี Server-side Pagination
> แบบฟอร์มใช้ DataAnnotations ตรวจทั้ง Client และ Server เช่น เลขกรมธรรม์ห้ามซ้ำ
> จำนวนเงินต้องมากกว่าศูนย์ และวันสิ้นสุดต้องอยู่หลังวันเริ่มคุ้มครองครับ

Request Flow:

```text
Browser → Routing → Controller → DbContext/Service → ViewModel → Razor View
```

### 3. RESTful API และ DTO — 45 วินาที

เปิด `PolicyClaimHub.http`

> ผมแยก API Controller จาก MVC Controller เพราะ MVC คืน HTML ส่วน API คืน JSON
> และ HTTP Status Code ผมใช้ DTO แยกจาก Entity เพื่อควบคุม Contract และป้องกัน
> Overposting เช่น POST สำเร็จคืน 201, ข้อมูลผิดคืน 400, ไม่พบคืน 404
> และเลขซ้ำคืน 409 Conflict ครับ

### 4. Business Logic ประเมินสินไหม — 45 วินาที

เปิด `Services/Claims/ClaimEstimationService.cs`

> ผมแยกสูตรออกจาก Controller เป็น Service เพื่อให้ MVC และ API ใช้กฎเดียวกัน
> และเขียน Unit Test ได้ ขั้นแรกตรวจว่ากรมธรรม์ Active และวันเกิดเหตุอยู่ใน
> ช่วงคุ้มครอง จากนั้นเลือกราคาประเมินตามระดับน้ำ ต่ำกว่า 20 ยังไม่เข้าเกณฑ์,
> 20 ถึงต่ำกว่า 40 เป็น 15%, 40 ถึงต่ำกว่า 60 เป็น 35%, 60 ถึงต่ำกว่า 100
> เป็น 60% และตั้งแต่ 100 เป็น 85% ของทุนประกัน แต่ยอดขั้นต้นต้องไม่เกิน
> ยอดเรียกร้อง แล้วจึงหัก Deductible และยอดหนี้ครับ

```text
Gross = min(Requested Amount, Sum Assured × Damage Rate)
Net = max(0, Gross - Deductible - Outstanding Debt)
```

### 5. Customer Analytics — 45 วินาที

เปิด `/Portfolio`

> ผมเพิ่มลูกค้าจำลอง 50 คนและผลิตภัณฑ์รถยนต์หลายประเภท ระบบดูประวัติเคลม
> รถชน ไฟไหม้ น้ำท่วม และรถสูญหาย แล้วคำนวณ Risk Classification,
> Renewal Probability และเบี้ยแนะนำด้วย Rule-based Logic ที่อธิบายเหตุผลได้
> หน้า Filter ใช้ Fetch เพื่อเปลี่ยนผลลัพธ์โดยไม่โหลดทั้งหน้าใหม่ครับ

ย้ำว่าไม่ใช่ Machine Learning หรือ Underwriting Rule จริง

### 6. GIS Dashboard — 45 วินาที

เปิด `/Dashboard`

> Dashboard ใช้ Leaflet และ OpenStreetMap แสดง Polygon น้ำท่วมกับจุดเคลม
> ผมสรุปจำนวนเคลม ยอดเรียกร้อง และยอดประมาณการตามพื้นที่ มีการเรียกข้อมูล
> จาก GISTDA และ NOW Bangkok และมีข้อมูลจำลองสำรอง เพื่อให้ Demo ทำงานต่อได้
> เมื่อ External API ช้าหรือไม่มีข้อมูลครับ พิกัดนี้เป็นจุดเกิดเหตุ ไม่ใช่ GPS Tracking

### 7. Oracle PL/SQL — 90 วินาที (Highlight)

เปิด `/OracleClaims` และ Package

> จุดเด่นที่ผมต้องการนำเสนอคือ Oracle Autonomous Database และ Package
> `PKG_FLOOD_CLAIM` ผมไม่ได้เขียน SQL ไว้อ่านอย่างเดียว แต่เชื่อม ASP.NET
> ผ่าน ODP.NET และเปิดเป็นทั้งหน้า MVC กับ REST API จริงครับ
>
> Package Specification เป็น Public Contract มี Function หนึ่งตัวและ Procedure
> สามตัว Function `FN_CALCULATE_ESTIMATED_PAYOUT` เป็นการคำนวณและประกาศ
> DETERMINISTIC เพราะ Input เดิมให้ผลเดิม `PR_SUBMIT_CLAIM` ตรวจสถานะกรมธรรม์
> และช่วงคุ้มครอง เรียก Function คำนวณ บันทึก Claim พร้อม `SDO_GEOMETRY`
> และเขียน Audit History ใน Transaction เดียว
>
> `PR_APPROVE_CLAIM` ใช้ `SELECT FOR UPDATE` ล็อกแถว ป้องกันเจ้าหน้าที่สองคน
> อนุมัติเคลมเดียวกันพร้อมกัน และห้ามอนุมัติเกินยอดประเมิน ส่วน
> `PR_GET_CLAIM_PAGE` ใช้ `OFFSET FETCH` และคืน `SYS_REFCURSOR` ให้หน้าเว็บ
> ทำ Pagination ครับ
>
> กฎผิดปกติส่งกลับด้วย `RAISE_APPLICATION_ERROR` เช่น -20003 คือไม่พบกรมธรรม์
> และ -20004 คือเลขเคลมซ้ำ ฝั่ง API แปลงเป็น 404 และ 409 Package ไม่ Commit เอง
> เพื่อให้ ASP.NET ควบคุม Transaction ถ้าทุกขั้นสำเร็จจึง Commit ครับ

ลำดับสาธิตหน้า Oracle:

1. ชี้ Database Connected และ Package VALID
2. ยื่นเคลมด้วยกรมธรรม์ `ORA-DEMO-001`
3. ระดับน้ำ 65 ซม. ได้ยอดประเมิน 565,000 บาท
4. กลับหน้ารายการซึ่งอ่านจาก `PR_GET_CLAIM_PAGE`
5. อนุมัติเคลมและแสดงสถานะ `APPROVED`

### 8. Git, Tests และปิด Demo — 30 วินาที

> ผมแบ่ง Git Commit ตาม Feature และมี Unit Tests ครอบคลุม Business Rules
> โปรเจกต์ Build แบบ Release ผ่าน README อธิบายวิธีรันและข้อจำกัดชัดเจน
> สิ่งที่จะพัฒนาต่อคือ Authentication/Role, Payment Gateway Test Mode
> และแยก Oracle Application Schema จาก ADMIN ตามหลัก Least Privilege ครับ

## คำถามที่มีโอกาสถูกถาม

### ทำไมใช้ SQLite และ Oracle พร้อมกัน

SQLite ทำให้ Clone และ Demo ง่าย ส่วน Oracle ใช้สาธิต PL/SQL, Package,
Transaction, Concurrency, Audit และ Spatial ไม่ได้อ้างว่าเป็นการ Sync สองฐาน
แต่เป็น Local Demo Store กับ Enterprise Workflow Prototype

### ทำไมไม่มี Delete Claim

Claim เป็นประวัติธุรกรรมที่ต้อง Audit การยกเลิกควรเปลี่ยนสถานะและบันทึกผู้ทำรายการ
ไม่ควรลบแถวออกจากฐานข้อมูล

### Function ต่างจาก Procedure อย่างไร

Function คืนค่าหนึ่งค่าและใช้ใน Expression ได้ Procedure เหมาะกับ Workflow
ที่อ่าน/เขียนหลายตารางและคืนหลายค่าผ่าน OUT Parameter

### Package มีข้อดีอะไร

รวม Public API ของ Domain เดียวกัน ซ่อน Implementation และทำให้ระบบภายนอก
เรียก Contract ที่ชัดเจน

### ทำไมใช้ SELECT FOR UPDATE

ล็อก Claim ระหว่างตรวจสถานะและอัปเดต ป้องกัน Lost Update หรืออนุมัติซ้ำ
จากหลาย Session พร้อมกัน

### ใครควร COMMIT

Caller หรือ Service Layer เพราะรู้ขอบเขต Use Case ทั้งหมด Package จึงไม่ Commit เอง
เพื่อให้ Rollback งานหลายขั้นพร้อมกันได้

### ป้องกัน SQL Injection อย่างไร

ODP.NET เรียก Stored Procedure ด้วย Parameter และ `BindByName = true`
ไม่มีการต่อ Input เป็น SQL String

### ถ้า Oracle ล่ม

โมดูล SQLite ยังทำงาน หน้า Oracle แสดง Health ว่า Unavailable โดยไม่เผย Password
และ GIS มีข้อมูลจำลอง fallback

### ข้อจำกัดที่ต้องบอกตรง ๆ

- ข้อมูลและสูตรเป็น Simulation
- Demo ใช้ Schema ADMIN เพราะวัตถุปัจจุบันสร้างอยู่ใน Schema นี้
- Production ต้องใช้ Application User และ Least Privilege
- ยังไม่มี Authentication, Role และ Payment Gateway จริง
- ยังไม่มี Oracle Integration Test ใน CI

## แผนสำรอง

หาก Internet หรือ Oracle มีปัญหา ให้เปิดภาพ Package `VALID`, ผลจาก
`05_demo_calls.sql` และอธิบายเส้นทาง Controller → Service → ODP.NET → Package
จากนั้น Demo MVC, REST, SQLite และ GIS fallback ต่อได้
