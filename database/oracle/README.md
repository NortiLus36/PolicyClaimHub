# Oracle / PL/SQL Demo

โฟลเดอร์นี้เป็น Oracle implementation สำหรับสาธิตทักษะ SQL, PL/SQL,
Business Logic และ Spatial Query ของ PolicyClaimHub โดย SQLite ยังคงเป็น
ฐานข้อมูลเริ่มต้นสำหรับการรันเว็บบนเครื่องทั่วไป

## ลำดับการรันใน SQL Developer

เชื่อมด้วย Application Schema เช่น `POLICY_APP` แล้วใช้ **Run Script (F5)**
ตามลำดับ:

1. `01_schema.sql`
2. `02_seed_data.sql`
3. `03_pkg_flood_claim.sql`
4. `04_views_and_queries.sql`
5. `05_demo_calls.sql`

ตรวจ Compilation Error หลังสร้าง Package:

```sql
SELECT line, position, text
FROM user_errors
WHERE name = 'PKG_FLOOD_CLAIM'
ORDER BY sequence;
```

ถ้า Query ไม่คืนแถว แปลว่า Package Compile ผ่าน

## Business Rules ใน Package

- กรมธรรม์ต้องมีสถานะ `ACTIVE`
- วันที่เกิดเหตุต้องอยู่ในช่วงความคุ้มครอง
- อัตราความเสียหายเปลี่ยนตามระดับน้ำ 15%, 35%, 60% และ 85%
- ยอดประเมินไม่เกินยอดเรียกร้องและทุนประกัน
- หัก Deductible และยอดหนี้คงเหลือ
- ห้ามอนุมัติยอดเกินยอดประเมิน
- ใช้ `SELECT ... FOR UPDATE` ป้องกันการอนุมัติชนกัน
- บันทึกประวัติทุกครั้งที่ยื่นหรืออนุมัติเคลม
- Package ไม่ `COMMIT` เอง เพื่อให้ Caller ควบคุม Transaction

## Spatial Design

- ใช้ SRID `4326` (WGS 84)
- จุดเกิดเหตุเก็บเป็น `SDO_GEOMETRY` ประเภท Point
- พื้นที่น้ำท่วมเก็บเป็น Polygon
- ใช้ Spatial Index และ `SDO_RELATE` ตรวจว่าจุดอยู่ภายใน Polygon

ข้อมูลทั้งหมดเป็นข้อมูลจำลองเพื่อการศึกษา ไม่ใช่ข้อมูลลูกค้าหรือเหตุการณ์จริง
