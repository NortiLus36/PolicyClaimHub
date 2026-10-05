-- Idempotent policy seed data for SQL Developer demonstrations.

MERGE INTO insurance_policy target
USING (
    SELECT
        'ORA-DEMO-001' AS policy_number,
        'กิตติพงษ์ ตัวอย่าง' AS insured_name,
        1000000 AS sum_assured,
        14500 AS premium_amount
    FROM dual
) source
ON (target.policy_number = source.policy_number)
WHEN NOT MATCHED THEN
    INSERT (
        policy_number,
        insured_name,
        sum_assured,
        premium_amount,
        coverage_start_date,
        coverage_end_date,
        policy_status
    )
    VALUES (
        source.policy_number,
        source.insured_name,
        source.sum_assured,
        source.premium_amount,
        DATE '2026-01-01',
        DATE '2027-12-31',
        'ACTIVE'
    );

MERGE INTO insurance_policy target
USING (
    SELECT
        'ORA-DEMO-002' AS policy_number,
        'วราภรณ์ ตัวอย่าง' AS insured_name,
        800000 AS sum_assured,
        12000 AS premium_amount
    FROM dual
) source
ON (target.policy_number = source.policy_number)
WHEN NOT MATCHED THEN
    INSERT (
        policy_number,
        insured_name,
        sum_assured,
        premium_amount,
        coverage_start_date,
        coverage_end_date,
        policy_status
    )
    VALUES (
        source.policy_number,
        source.insured_name,
        source.sum_assured,
        source.premium_amount,
        DATE '2026-01-01',
        DATE '2027-12-31',
        'ACTIVE'
    );

INSERT INTO flood_area (
    area_name,
    severity,
    observed_at,
    area_geometry
)
SELECT
    'พื้นที่น้ำท่วมจำลองตอนเหนือของกรุงเทพฯ',
    'HIGH',
    SYSTIMESTAMP,
    MDSYS.SDO_GEOMETRY(
        2003,
        4326,
        NULL,
        MDSYS.SDO_ELEM_INFO_ARRAY(1, 1003, 1),
        MDSYS.SDO_ORDINATE_ARRAY(
            100.555, 13.835,
            100.565, 13.925,
            100.640, 13.935,
            100.655, 13.845,
            100.555, 13.835
        )
    )
FROM dual
WHERE NOT EXISTS (
    SELECT 1
    FROM flood_area
    WHERE area_name = 'พื้นที่น้ำท่วมจำลองตอนเหนือของกรุงเทพฯ'
);

COMMIT;
