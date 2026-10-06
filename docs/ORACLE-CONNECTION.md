# เชื่อม PolicyClaimHub กับ Oracle Autonomous Database

เว็บใช้ SQLite เป็นฐานข้อมูลหลักสำหรับ CRUD เดิม และใช้ Oracle เป็นโมดูล
Enterprise Claim Workflow ผ่าน `PKG_FLOOD_CLAIM` โดยตรง การแยกสองส่วนนี้ทำให้
Demo บนเครื่องยังทำงานได้แม้ Oracle Cloud ไม่พร้อม

## สิ่งที่เตรียมในโค้ดแล้ว

- ODP.NET Core (`Oracle.ManagedDataAccess.Core`)
- หน้า `/OracleClaims`
- REST API `/api/oracle-flood-claims`
- Health Check ตรวจ Connection และสถานะ Package
- เรียก `PR_SUBMIT_CLAIM`, `PR_APPROVE_CLAIM` และ `PR_GET_CLAIM_PAGE`
- ใช้ Transaction จากฝั่ง ASP.NET และ `COMMIT` เฉพาะเมื่อ Procedure สำเร็จ
- แปลง `RAISE_APPLICATION_ERROR` เป็น HTTP 400, 404 หรือ 409
- ไม่เก็บ Password หรือ Wallet ใน Git

## 1. ดาวน์โหลด Wallet

ใน Oracle Cloud Console เปิด Autonomous Database `PolicyClaimHub` แล้วเลือก
**Database connection → Download wallet → Instance wallet** จากนั้นแตกไฟล์ไว้
นอก Repository ตัวอย่าง:

```text
D:\OracleWallet\Wallet_POLICYCLAIMHUB
```

โฟลเดอร์ควรมี `tnsnames.ora`, `sqlnet.ora`, `cwallet.sso` และไฟล์ Wallet อื่น
ห้ามวาง Wallet ใน Repository หรือ Commit ขึ้น Git

## 2. เก็บ Connection String ใน User Secrets

รันจากโฟลเดอร์โปรเจกต์:

```powershell
.\scripts\configure-oracle-secret.ps1
```

วิธีนี้ใช้ Wallet ที่ `C:\Users\admin\Downloads\wallet` และ Service
`policyclaimhub_tp` เป็นค่าเริ่มต้น รหัสผ่านจะแสดงเป็นตัวซ่อนและไม่ถูกเก็บใน Git
หาก Wallet อยู่ที่อื่นให้ระบุ path ด้วย `-WalletPath`

หรือกำหนดค่าด้วยคำสั่งโดยตรง:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:OracleConnection" "User Id=ADMIN;Password=ใส่รหัสผ่านจริงที่นี่;Data Source=policyclaimhub_tp;Tns_Admin=D:\OracleWallet\Wallet_POLICYCLAIMHUB"
```

ใช้ `ADMIN` เฉพาะ Portfolio Demo เพราะตารางและ Package ชุดปัจจุบันสร้างอยู่ใต้
Schema `ADMIN` สำหรับ Production ควรสร้าง Application Schema สิทธิ์จำกัด เช่น
`POLICY_APP` และ Grant เฉพาะ `EXECUTE`/`SELECT` ที่จำเป็น

ตรวจว่าตั้ง Secret แล้วโดยไม่แสดงค่ารหัสผ่าน:

```powershell
dotnet user-secrets list | Select-String "OracleConnection"
```

## 3. ทดสอบ

```powershell
dotnet run
```

เปิด `/OracleClaims` และ `/api/oracle-flood-claims/health`

Health ที่พร้อมใช้งานต้องแสดง `isConfigured: true`, `canConnect: true` และ
`packageStatus: VALID`

## REST API ที่เชื่อม Package

| Method | Endpoint | Package member |
|---|---|---|
| GET | `/api/oracle-flood-claims` | `PR_GET_CLAIM_PAGE` |
| POST | `/api/oracle-flood-claims` | `PR_SUBMIT_CLAIM` |
| PUT | `/api/oracle-flood-claims/{id}/approve` | `PR_APPROVE_CLAIM` |
| GET | `/api/oracle-flood-claims/health` | ตรวจ `USER_OBJECTS` |

ไม่มี Delete Endpoint โดยตั้งใจ เพราะ Claim และ Audit เป็นประวัติธุรกรรม
การยกเลิกควรทำด้วยสถานะและบันทึก Audit ไม่ใช่ลบแถวออกจากฐานข้อมูล

## Troubleshooting

- `NOT_CONFIGURED`: ยังไม่มี `ConnectionStrings:OracleConnection`
- `UNAVAILABLE`: ตรวจรหัสผ่าน, Wallet path, ชื่อ TNS และสถานะ Autonomous Database
- `ORA-50230`: ต้องกำหนด `WalletLocation` เป็น path จริง ไม่ใช้ `?` จาก `sqlnet.ora`
- `Package INVALID`: รัน `03_pkg_flood_claim.sql` และ Query `USER_ERRORS`
- `ORA-20003`: ไม่พบเลขกรมธรรม์ใน Oracle
- `ORA-20004`: เลขเคลมซ้ำ
- `ORA-20006`: ยอดอนุมัติเกินยอดประเมิน
