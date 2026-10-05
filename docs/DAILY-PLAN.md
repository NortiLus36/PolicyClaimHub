# แผนเร่งด่วน Policy Claim Hub

ช่วงดำเนินการ: 4–7 ตุลาคม 2026  
วันสัมภาษณ์: 7 ตุลาคม 2026

## เป้าหมายใหม่

เวลาที่เหลือไม่พอสำหรับสร้างทุกโมดูลแบบ Production ดังนั้น Portfolio รุ่นสัมภาษณ์จะเน้นเส้นทางที่สาธิตและอธิบายได้จริง:

1. ASP.NET Core MVC และ CRUD กรมธรรม์
2. RESTful API ที่ใช้ DTO, Validation และ HTTP Status Code ถูกต้อง
3. Business Logic สำหรับประเมินสินไหมน้ำท่วม
4. GIS Map และ Dashboard ขนาดเล็ก
5. Oracle SQL/PLSQL: Procedure, Function, Package และ Query ที่มี Business Logic
6. Git history และเอกสารที่อธิบายการตัดสินใจได้

Payment Gateway ยังคงอยู่ในแผน แต่เป็น Stretch Goal หลังแกนหลักข้างต้นทำงานครบ

## สถานะล่าสุด — 5 ตุลาคม 2026

- เสร็จ: MVC CRUD, SQLite Migrations และ Validation
- เสร็จ: REST API กรมธรรม์และสินไหมน้ำท่วม
- เสร็จ: Claim Estimation Service และ Unit Tests
- เสร็จ: GIS Dashboard และข้อมูลจุดเกิดเหตุจำลอง
- เสร็จ: Product Catalog, ลูกค้าจำลอง 50 คน และประวัติเคลมหลายประเภท
- เสร็จ: Risk Classification, Renewal Probability และเบี้ยแนะนำ
- เตรียมแล้ว: Oracle DDL, Package, Function, Procedures และ Spatial Query
- งานเร่งด่วนถัดไป: รัน Oracle scripts จริงใน SQL Developer และเก็บภาพหลักฐาน
- Stretch Goal: Payment Gateway Test Mode

## หลักการทำงานร่วมกัน

- ผู้เรียนพิมพ์โค้ดฟีเจอร์หลักเอง โดยทำทีละไฟล์และเข้าใจหน้าที่ก่อน
- หลังจบแต่ละขั้นต้อง Build, Run และ Test
- Commit งานเป็นช่วงเล็ก ๆ และอ่าน diff ก่อน Commit
- ใช้ข้อมูลกรมธรรม์ รถยนต์ บุคคล และพิกัดแบบจำลองเท่านั้น
- ห้าม Commit API Key, Password, Secret หรือข้อมูลส่วนบุคคล
- ถ้างานหลักไม่ผ่าน ห้ามเพิ่มฟีเจอร์ใหม่

## 4 ตุลาคม — Core Domain, Database และ Git

### ต้องเสร็จ

- เรียน `git status`, `git add`, `git diff --cached` และทำ First Commit
- อ่าน `Program.cs` และทบทวน MVC Request Flow
- สร้าง `InsurancePolicy` พร้อม DataAnnotations
- สร้าง `MotorFloodClaim` เฉพาะฟิลด์ที่จำเป็น
- สร้าง `ApplicationDbContext`
- ตั้งค่า SQLite และสร้าง Migration แรก
- Seed ข้อมูลจำลองขนาดเล็ก
- ทำหน้า List และ Create กรมธรรม์ก่อน

### Business Rules แรก

- เลขกรมธรรม์ห้ามว่างและห้ามซ้ำ
- ทุนประกันและเบี้ยต้องมากกว่า 0
- วันที่สิ้นสุดต้องอยู่หลังวันที่เริ่มต้น
- วันที่เกิดเหตุต้องอยู่ในช่วงที่กรมธรรม์มีผล

### ตรวจสอบ

- `dotnet build` ผ่าน
- Migration และ Database Update ผ่าน
- เพิ่มกรมธรรม์แล้วแสดงในรายการได้
- Validation ที่ Server ปฏิเสธข้อมูลผิดได้

### Commit ที่คาดหวัง

- `chore: initialize PolicyClaimHub project`
- `feat: add policy and flood claim domain models`
- `feat: add SQLite persistence and initial migration`

## 5 ตุลาคม — CRUD, RESTful API และ Business Service

### ต้องเสร็จ

- ทำ Edit และ Details ของกรมธรรม์
- สร้าง Service สำหรับ Business Logic แทนการคำนวณใน Controller
- สร้าง DTO แยกจาก Entity
- สร้าง RESTful API สำหรับกรมธรรม์
- สร้าง API สำหรับประเมินค่าสินไหมน้ำท่วม
- เพิ่มไฟล์ `.http` สำหรับทดสอบ API
- ทดสอบกรณีสำเร็จและข้อมูลผิด

### REST Endpoints ขั้นต่ำ

- `GET /api/policies`
- `GET /api/policies/{id}`
- `POST /api/policies`
- `PUT /api/policies/{id}`
- `DELETE /api/policies/{id}` ถ้ามีเวลา
- `POST /api/flood-claims/estimate`
- `GET /api/flood-claims/summary?district=...`

### สิ่งที่ต้องอธิบายได้

- REST ต่างจาก MVC Controller อย่างไร
- Entity ต่างจาก DTO อย่างไร
- `GET`, `POST`, `PUT`, `DELETE` ใช้เมื่อใด
- ความหมายของ `200`, `201`, `204`, `400`, `404` และ `409`
- เหตุผลที่ Business Logic อยู่ใน Service ไม่อยู่ใน Controller

### ตรวจสอบ

- API คืน Status Code ถูกต้อง
- `POST` สำเร็จคืน `201 Created`
- Id ที่ไม่มีอยู่คืน `404 Not Found`
- เลขกรมธรรม์ซ้ำคืน `409 Conflict`
- Validation ผิดคืน `400 Bad Request`

### Commit ที่คาดหวัง

- `feat: complete policy management pages`
- `feat: add policy REST API`
- `feat: add flood claim estimation service`

## 6 ตุลาคม — GIS Dashboard, Oracle/PLSQL และเอกสาร

### ต้องเสร็จ

- แสดงจุดเคลมจำลองบน Leaflet
- แสดง Dashboard: จำนวนเคลม ยอดเรียกร้อง และยอดประมาณการแยกตามเขต
- สร้าง Oracle DDL สำหรับตารางหลัก
- สร้าง PL/SQL Package ที่รวม Function และ Procedure
- สร้าง Query สรุปข้อมูลที่มี CTE, CASE, Aggregate และ Business Rules
- เพิ่มตัวอย่าง Oracle Spatial ตรวจ Point อยู่ใน Flood Polygon
- ปรับ README และเตรียมบทพูด Demo 3–5 นาที
- ถ่ายภาพหน้าจอหรือวิดีโอสำรอง

### PL/SQL ที่ต้องมี

- Function `FN_CALCULATE_ESTIMATED_PAYOUT`
- Procedure `PR_SUBMIT_CLAIM`
- Procedure `PR_APPROVE_CLAIM`
- Package `PKG_FLOOD_CLAIM` รวม Public API ของ Logic
- View หรือ Query `VW_DISTRICT_CLAIM_EXPOSURE`
- Query ใช้ `SDO_CONTAINS` หรือ `SDO_RELATE` สำหรับ Spatial Matching

### Business Logic ตัวอย่าง

- ตรวจว่ากรมธรรม์มีผลในวันเกิดเหตุ
- คำนวณความเสียหายจากระดับน้ำตามช่วง
- หัก Deductible และหนี้ค้างชำระ
- จำกัดยอดไม่ให้เกินทุนประกัน
- ห้ามอนุมัติเคลมที่อยู่ผิดสถานะ
- บันทึกประวัติการเปลี่ยนสถานะ
- สรุปยอด Exposure แยกตามเขตและระดับความรุนแรง

### Stretch Goal: Payment Gateway

ทำเมื่อแกนหลักทั้งหมดผ่านแล้วเท่านั้น:

- สร้าง `PremiumInvoice` และ `PaymentTransaction`
- เชื่อม Stripe Checkout Test Mode
- รับ Webhook และป้องกันการประมวลผลซ้ำ

ถ้าเวลาไม่พอ ให้เก็บ Payment Flow ไว้ใน Specification และอธิบายการออกแบบ ไม่สร้างโค้ดครึ่งเสร็จ

### Commit ที่คาดหวัง

- `feat: add flood claim map and dashboard`
- `feat: add Oracle PL/SQL claim business rules`
- `docs: prepare portfolio demo and API examples`

## 7 ตุลาคม — วันสัมภาษณ์

### ก่อนสัมภาษณ์

- ไม่เพิ่มฟีเจอร์ใหม่
- Build และ Run รอบสุดท้าย
- ทดสอบ MVC, API, Dashboard และ Script ที่จะเปิดให้ดู
- เปิด Repository และเตรียม Commit History
- เตรียม README, API requests, SQL scripts และภาพสำรอง

### ลำดับ Demo

1. อธิบายปัญหาน้ำท่วมและโจทย์ทางธุรกิจ
2. สร้างกรมธรรม์จาก MVC
3. เรียก REST API และอธิบาย DTO/Status Code
4. ประเมินสินไหมผ่าน Business Service
5. แสดงจุดเคลมและยอด Exposure บน Dashboard
6. เปิด Oracle Package และอธิบาย Function/Procedure/Query
7. เปิด Git history แสดงพัฒนาการทีละขั้น

## เกณฑ์ตัดฟีเจอร์

ห้ามตัด:

1. Build/Run ได้
2. CRUD ขั้นต่ำทำงาน
3. REST API มี Endpoint ที่ทดสอบได้
4. Business Logic มีผลลัพธ์และกรณีผิดพลาด
5. Oracle Package/Query อ่านและอธิบายได้
6. README มีวิธีทดลอง

ตัดก่อนได้:

1. Payment Gateway
2. Login/Role
3. Live Flood API
4. กราฟรองและ Filter หลายแบบ
5. เชื่อม Oracle เป็นฐานข้อมูลหลัก

