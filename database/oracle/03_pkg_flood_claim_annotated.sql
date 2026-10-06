-- ============================================================================
-- PolicyClaimHub: PKG_FLOOD_CLAIM (ฉบับอธิบายภาษาไทย)
-- ใช้ไฟล์นี้สำหรับเรียนและพรีเซนต์ ส่วนไฟล์ 03_pkg_flood_claim.sql ใช้ Deploy
-- ============================================================================

-- PACKAGE SPECIFICATION คือ Public Contract ที่ระบบภายนอกมองเห็นและเรียกใช้ได้
CREATE OR REPLACE PACKAGE pkg_flood_claim AS

    -- Function รับข้อมูลความเสียหายและคืนยอดสินไหมประมาณการหนึ่งค่า
    FUNCTION fn_calculate_estimated_payout (
        -- ทุนประกันของกรมธรรม์
        p_sum_assured       IN NUMBER,
        -- ระดับน้ำ ณ จุดเกิดเหตุ หน่วยเซนติเมตร
        p_water_depth_cm    IN NUMBER,
        -- ยอดเงินที่ผู้เอาประกันร้องขอ
        p_requested_amount  IN NUMBER,
        -- ค่าเสียหายส่วนแรกที่ต้องหัก
        p_deductible_amount IN NUMBER,
        -- ยอดหนี้คงเหลือที่ต้องหัก
        p_outstanding_debt  IN NUMBER
    -- Function นี้คืนค่าเป็นตัวเลขว่าได้เท่าไร
    ) RETURN NUMBER DETERMINISTIC;

    -- Procedure รับคำขอยื่นเคลม บันทึก Claim/Audit และคืน Claim ID กับยอดประเมิน
    PROCEDURE pr_submit_claim (
        -- เลขเคลมจากระบบต้นทาง
        p_claim_number         IN VARCHAR2,
        -- เลขกรมธรรม์ที่ต้องการใช้สิทธิ์
        p_policy_number        IN VARCHAR2,
        -- ทะเบียนรถที่เกิดเหตุ
        p_vehicle_registration IN VARCHAR2,
        -- วันที่เกิดเหตุ ใช้ตรวจช่วงคุ้มครอง
        p_incident_date        IN DATE,
        -- เขตที่เกิดเหตุ ใช้ค้นหาและทำรายงาน
        p_district             IN VARCHAR2,
        -- พิกัดแกน Y
        p_latitude             IN NUMBER,
        -- พิกัดแกน X
        p_longitude            IN NUMBER,
        -- ระดับน้ำ หน่วยเซนติเมตร
        p_water_depth_cm       IN NUMBER,
        -- ยอดที่ผู้เอาประกันเรียกร้อง
        p_requested_amount     IN NUMBER,
        -- ถ้าไม่ส่ง Deductible ให้ใช้ศูนย์
        p_deductible_amount    IN NUMBER DEFAULT 0,
        -- ถ้าไม่ส่งยอดหนี้ให้ใช้ศูนย์
        p_outstanding_debt     IN NUMBER DEFAULT 0,
        -- ระบุผู้หรือระบบที่ทำรายการ Audit
        p_changed_by           IN VARCHAR2 DEFAULT 'SQL_DEVELOPER_DEMO',
        -- คืน Primary Key ของ Claim ที่เพิ่งสร้าง
        p_claim_id             OUT NUMBER,
        -- คืนยอดสินไหมที่ Function คำนวณได้
        p_estimated_payout     OUT NUMBER
    );

    -- Procedure อนุมัติเคลมและบันทึกประวัติการเปลี่ยนสถานะ
    PROCEDURE pr_approve_claim (
        -- Claim ที่ต้องการอนุมัติ
        p_claim_id        IN NUMBER,
        -- ยอดที่เจ้าหน้าที่อนุมัติ
        p_approved_amount IN NUMBER,
        -- ผู้อนุมัติหรือระบบที่เรียก
        p_changed_by      IN VARCHAR2,
        -- หมายเหตุเพิ่มเติม ไม่ส่งก็ได้
        p_change_note     IN VARCHAR2 DEFAULT NULL
    );

    -- Procedure คืน Claim แบบแบ่งหน้า พร้อมจำนวนแถวทั้งหมด
    PROCEDURE pr_get_claim_page (
        -- เลขหน้า เริ่มจากหนึ่ง
        p_page_number IN PLS_INTEGER,
        -- จำนวนรายการต่อหน้า
        p_page_size   IN PLS_INTEGER,
        -- เขตที่ใช้กรอง ส่ง NULL เพื่อดูทุกเขต
        p_district    IN VARCHAR2,
        -- คืนจำนวนรายการทั้งหมดก่อนแบ่งหน้า
        p_total_rows  OUT NUMBER,
        -- คืน Result Set ให้ Caller อ่านทีละแถว
        p_result      OUT SYS_REFCURSOR
    );

-- จบ Public Contract ของ Package
END pkg_flood_claim;
-- เครื่องหมาย / สั่ง SQL Worksheet ให้ส่ง PL/SQL Unit ไป Compile
/

-- PACKAGE BODY คือ Implementation ของ Function/Procedure ที่ประกาศไว้ด้านบน
CREATE OR REPLACE PACKAGE BODY pkg_flood_claim AS

    -- เริ่ม Implementation ของ Function คำนวณยอดประมาณการ
    FUNCTION fn_calculate_estimated_payout (
        p_sum_assured       IN NUMBER,
        p_water_depth_cm    IN NUMBER,
        p_requested_amount  IN NUMBER,
        p_deductible_amount IN NUMBER,
        p_outstanding_debt  IN NUMBER
    ) RETURN NUMBER DETERMINISTIC
    -- IS เริ่มส่วนประกาศตัวแปรภายใน Function
    IS
        -- อัตราความเสียหาย เช่น 0.1500 หรือ 0.8500
        v_damage_rate NUMBER(5, 4);
        -- ความเสียหายก่อนหัก Deductible และหนี้
        v_gross_loss  NUMBER(18, 2);
        -- ยอดสุทธิหลังหักรายการต่าง ๆ
        v_net_payout  NUMBER(18, 2);
    -- BEGIN เริ่มส่วนประมวลผล
    BEGIN
        -- ทุนประกันและยอดเรียกร้องต้องเป็นจำนวนบวก
        IF p_sum_assured <= 0 OR p_requested_amount <= 0 THEN
            -- สร้าง Business Error ด้วยรหัสในช่วง -20000 ถึง -20999
            RAISE_APPLICATION_ERROR(
                -20010,
                'Sum assured and requested amount must be greater than zero.'
            );
        -- จบเงื่อนไขตรวจจำนวนเงิน
        END IF;

        -- จำกัดระดับน้ำให้อยู่ในขอบเขตข้อมูลจำลอง
        IF p_water_depth_cm < 0 OR p_water_depth_cm > 500 THEN
            RAISE_APPLICATION_ERROR(
                -20011,
                'Water depth must be between 0 and 500 centimetres.'
            );
        -- จบเงื่อนไขตรวจระดับน้ำ
        END IF;

        -- CASE เลือกอัตราความเสียหายจากระดับน้ำ โดยตรวจจากบนลงล่าง
        -- ใช้ขอบเขตแบบ "ตั้งแต่ค่าหนึ่ง ถึงน้อยกว่าค่าถัดไป" จึงไม่มีค่าซ้ำกัน
        v_damage_rate := CASE
            -- ต่ำกว่า 20 ซม. ยังไม่เข้าเกณฑ์จ่ายตามแบบจำลอง จึงเป็น 0%
            WHEN p_water_depth_cm < 20 THEN 0.00
            -- ตั้งแต่ 20 แต่ต่ำกว่า 40 ซม. ประเมิน 15%
            WHEN p_water_depth_cm < 40 THEN 0.15
            -- ตั้งแต่ 40 แต่ต่ำกว่า 60 ซม. ประเมิน 35%
            WHEN p_water_depth_cm < 60 THEN 0.35
            -- ตั้งแต่ 60 แต่ต่ำกว่า 100 ซม. ประเมิน 60%
            WHEN p_water_depth_cm < 100 THEN 0.60
            -- ตั้งแต่ 100 ซม. ขึ้นไป ประเมิน 85%
            ELSE 0.85
        -- จบ CASE และกำหนดค่าให้ตัวแปร
        END;

        -- LEAST ป้องกันความเสียหายขั้นต้นเกินยอดเรียกร้องหรือเพดานตามสูตร
        v_gross_loss := LEAST(
            p_requested_amount,
            p_sum_assured * v_damage_rate
        );

        -- GREATEST ป้องกันยอดสุทธิติดลบ
        v_net_payout := GREATEST(
            0,
            -- NVL เปลี่ยน NULL เป็นศูนย์ก่อนนำไปลบ
            v_gross_loss
                - NVL(p_deductible_amount, 0)
                - NVL(p_outstanding_debt, 0)
        );

        -- จำกัดยอดไม่เกินทุนประกัน ปัดทศนิยมสองตำแหน่ง แล้วคืนค่า
        RETURN ROUND(LEAST(v_net_payout, p_sum_assured), 2);
    -- จบ Function โดยระบุชื่อเพื่อให้อ่านง่าย
    END fn_calculate_estimated_payout;

    -- เริ่ม Procedure สำหรับยื่นเคลม
    PROCEDURE pr_submit_claim (
        p_claim_number         IN VARCHAR2,
        p_policy_number        IN VARCHAR2,
        p_vehicle_registration IN VARCHAR2,
        p_incident_date        IN DATE,
        p_district             IN VARCHAR2,
        p_latitude             IN NUMBER,
        p_longitude            IN NUMBER,
        p_water_depth_cm       IN NUMBER,
        p_requested_amount     IN NUMBER,
        p_deductible_amount    IN NUMBER DEFAULT 0,
        p_outstanding_debt     IN NUMBER DEFAULT 0,
        p_changed_by           IN VARCHAR2 DEFAULT 'SQL_DEVELOPER_DEMO',
        p_claim_id             OUT NUMBER,
        p_estimated_payout     OUT NUMBER
    )
    IS
        -- %TYPE ทำให้ตัวแปรใช้ชนิดเดียวกับคอลัมน์จริง
        v_policy_id           insurance_policy.policy_id%TYPE;
        v_sum_assured         insurance_policy.sum_assured%TYPE;
        v_policy_status       insurance_policy.policy_status%TYPE;
        v_coverage_start_date insurance_policy.coverage_start_date%TYPE;
        v_coverage_end_date   insurance_policy.coverage_end_date%TYPE;
    BEGIN
        -- อ่านกรมธรรม์หนึ่งแถวและนำค่ามาใส่ตัวแปรด้วย SELECT INTO
        SELECT
            policy_id,
            sum_assured,
            policy_status,
            coverage_start_date,
            coverage_end_date
        INTO
            v_policy_id,
            v_sum_assured,
            v_policy_status,
            v_coverage_start_date,
            v_coverage_end_date
        FROM insurance_policy
        -- TRIM ตัดช่องว่าง และ UPPER ปรับรูปแบบเลขกรมธรรม์
        WHERE policy_number = UPPER(TRIM(p_policy_number));

        -- อนุญาตให้ยื่นเคลมเฉพาะกรมธรรม์ ACTIVE
        IF v_policy_status <> 'ACTIVE' THEN
            RAISE_APPLICATION_ERROR(
                -20001,
                'Policy is not active.'
            );
        END IF;

        -- TRUNC ตัดเวลาออกก่อนเทียบวันที่กับช่วงคุ้มครอง
        IF TRUNC(p_incident_date) NOT BETWEEN
            TRUNC(v_coverage_start_date) AND TRUNC(v_coverage_end_date)
        THEN
            RAISE_APPLICATION_ERROR(
                -20002,
                'Incident date is outside the coverage period.'
            );
        END IF;

        -- เรียก Function ภายใน Package และใส่ผลลง OUT Parameter
        p_estimated_payout := fn_calculate_estimated_payout(
            v_sum_assured,
            p_water_depth_cm,
            p_requested_amount,
            p_deductible_amount,
            p_outstanding_debt
        );

        -- บันทึกข้อมูล Claim หลัง Validation และการคำนวณผ่านแล้ว
        INSERT INTO motor_flood_claim (
            claim_number,
            policy_id,
            vehicle_registration,
            incident_date,
            district,
            incident_location,
            water_depth_cm,
            requested_amount,
            deductible_amount,
            outstanding_debt,
            estimated_payout,
            claim_status
        )
        VALUES (
            -- Normalize เลขเคลม
            UPPER(TRIM(p_claim_number)),
            -- ใช้ Policy ID ที่อ่านจากตาราง
            v_policy_id,
            -- Normalize ทะเบียนรถ
            UPPER(TRIM(p_vehicle_registration)),
            p_incident_date,
            TRIM(p_district),
            -- สร้าง Geometry สองมิติชนิด Point ใน WGS 84
            MDSYS.SDO_GEOMETRY(
                2001,
                4326,
                -- Oracle ใช้ X,Y จึงส่ง Longitude ก่อน Latitude
                MDSYS.SDO_POINT_TYPE(p_longitude, p_latitude, NULL),
                NULL,
                NULL
            ),
            p_water_depth_cm,
            p_requested_amount,
            NVL(p_deductible_amount, 0),
            NVL(p_outstanding_debt, 0),
            p_estimated_payout,
            -- สถานะแรกของ Claim
            'SUBMITTED'
        )
        -- รับ Identity ที่เพิ่งสร้างโดยไม่ต้อง SELECT MAX
        RETURNING claim_id INTO p_claim_id;

        -- เพิ่ม Audit Trail ใน Transaction เดียวกับ Claim
        INSERT INTO claim_status_history (
            claim_id,
            previous_status,
            new_status,
            changed_by,
            change_note
        )
        VALUES (
            p_claim_id,
            -- Claim ใหม่ยังไม่มีสถานะก่อนหน้า
            NULL,
            'SUBMITTED',
            p_changed_by,
            'Claim submitted through PKG_FLOOD_CLAIM'
        );
    -- เริ่มจัดการ Exception ที่คาดไว้
    EXCEPTION
        -- SELECT INTO ไม่พบเลขกรมธรรม์
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20003, 'Policy number was not found.');
        -- Unique Constraint ตรวจพบเลขเคลมซ้ำ
        WHEN DUP_VAL_ON_INDEX THEN
            RAISE_APPLICATION_ERROR(-20004, 'Claim number already exists.');
    -- จบ Procedure ยื่นเคลม
    END pr_submit_claim;

    -- เริ่ม Procedure อนุมัติเคลม
    PROCEDURE pr_approve_claim (
        p_claim_id        IN NUMBER,
        p_approved_amount IN NUMBER,
        p_changed_by      IN VARCHAR2,
        p_change_note     IN VARCHAR2 DEFAULT NULL
    )
    IS
        -- เก็บสถานะเดิมเพื่อใช้ตรวจและบันทึก Audit
        v_previous_status  motor_flood_claim.claim_status%TYPE;
        -- เก็บเพดานยอดที่ Function ประเมินไว้
        v_estimated_payout motor_flood_claim.estimated_payout%TYPE;
    BEGIN
        -- อ่าน Claim และล็อกแถวไว้จนกว่า Caller จะ COMMIT หรือ ROLLBACK
        SELECT claim_status, estimated_payout
        INTO v_previous_status, v_estimated_payout
        FROM motor_flood_claim
        WHERE claim_id = p_claim_id
        FOR UPDATE;

        -- อนุมัติได้เฉพาะ Claim ที่ยังอยู่ในขั้นรับเรื่องหรือตรวจสอบ
        IF v_previous_status NOT IN ('SUBMITTED', 'UNDER_REVIEW') THEN
            RAISE_APPLICATION_ERROR(
                -20005,
                'Claim status does not allow approval.'
            );
        END IF;

        -- ยอดอนุมัติต้องไม่ติดลบและต้องไม่เกินยอดประเมิน
        IF p_approved_amount < 0 OR
           p_approved_amount > v_estimated_payout
        THEN
            RAISE_APPLICATION_ERROR(
                -20006,
                'Approved amount must be between zero and estimated payout.'
            );
        END IF;

        -- อัปเดตยอดอนุมัติ สถานะ และเวลาแก้ไขล่าสุด
        UPDATE motor_flood_claim
        SET approved_amount = p_approved_amount,
            claim_status = 'APPROVED',
            updated_at = SYSTIMESTAMP
        WHERE claim_id = p_claim_id;

        -- บันทึกการเปลี่ยนสถานะจากค่าเดิมเป็น APPROVED
        INSERT INTO claim_status_history (
            claim_id,
            previous_status,
            new_status,
            changed_by,
            change_note
        )
        VALUES (
            p_claim_id,
            v_previous_status,
            'APPROVED',
            p_changed_by,
            p_change_note
        );
    EXCEPTION
        -- SELECT INTO ไม่พบ Claim ID
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20007, 'Claim was not found.');
    -- จบ Procedure อนุมัติเคลม
    END pr_approve_claim;

    -- เริ่ม Procedure แบ่งหน้ารายการ Claim
    PROCEDURE pr_get_claim_page (
        p_page_number IN PLS_INTEGER,
        p_page_size   IN PLS_INTEGER,
        p_district    IN VARCHAR2,
        p_total_rows  OUT NUMBER,
        p_result      OUT SYS_REFCURSOR
    )
    IS
        -- จำนวนแถวที่ต้องข้ามก่อนเริ่มหน้าปัจจุบัน
        v_offset PLS_INTEGER;
    BEGIN
        -- เลขหน้าต้องเริ่มจากหนึ่ง
        IF p_page_number < 1 THEN
            RAISE_APPLICATION_ERROR(-20020, 'Page number must be at least 1.');
        END IF;

        -- จำกัด Page Size ป้องกัน Query คืนข้อมูลมากเกินไป
        IF p_page_size < 1 OR p_page_size > 100 THEN
            RAISE_APPLICATION_ERROR(-20021, 'Page size must be between 1 and 100.');
        END IF;

        -- หน้า 1 ข้าม 0 แถว หน้า 2 ข้ามหนึ่ง Page Size
        v_offset := (p_page_number - 1) * p_page_size;

        -- นับจำนวนทั้งหมดเพื่อให้ UI คำนวณ Total Pages
        SELECT COUNT(*)
        INTO p_total_rows
        FROM motor_flood_claim claim
        -- ถ้าเขตเป็น NULL ไม่กรอง มิฉะนั้นเลือกเฉพาะเขตที่ตรงกัน
        WHERE p_district IS NULL
           OR claim.district = TRIM(p_district);

        -- เปิด Ref Cursor เพื่อคืน Result Set ให้ Caller
        OPEN p_result FOR
            SELECT
                claim.claim_id,
                claim.claim_number,
                policy.policy_number,
                claim.vehicle_registration,
                claim.incident_date,
                claim.district,
                claim.water_depth_cm,
                claim.requested_amount,
                claim.estimated_payout,
                claim.claim_status
            FROM motor_flood_claim claim
            -- Join เพื่อคืนเลขกรมธรรม์แทนการคืนเฉพาะ Policy ID
            JOIN insurance_policy policy
                ON policy.policy_id = claim.policy_id
            WHERE p_district IS NULL
               OR claim.district = TRIM(p_district)
            -- ต้องกำหนดลำดับก่อนแบ่งหน้าเพื่อให้ผลลัพธ์คงที่
            ORDER BY claim.incident_date DESC, claim.claim_id DESC
            -- ข้ามแถวของหน้าก่อนหน้า
            OFFSET v_offset ROWS
            -- ดึงเฉพาะจำนวนรายการต่อหน้า
            FETCH NEXT p_page_size ROWS ONLY;
    -- จบ Procedure Pagination
    END pr_get_claim_page;

-- จบ Package Body
END pkg_flood_claim;
/

-- Package ไม่ COMMIT เอง เพื่อให้ Caller ควบคุม Transaction Boundary
-- หลังเรียกสำเร็จให้ Caller เลือก COMMIT และตอนทดสอบ Error สามารถ ROLLBACK ได้
