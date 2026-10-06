SET SERVEROUTPUT ON;

-- Verify that both the public specification and implementation compiled.
SELECT object_type, status
FROM user_objects
WHERE object_name = 'PKG_FLOOD_CLAIM'
ORDER BY object_type;

-- Boundary test for the current rate table.
SELECT water_depth_cm,
       pkg_flood_claim.fn_calculate_estimated_payout(
           1000000,
           water_depth_cm,
           1000000,
           0,
           0
       ) AS estimated_payout
FROM (
    SELECT 19 AS water_depth_cm FROM dual UNION ALL
    SELECT 20 FROM dual UNION ALL
    SELECT 40 FROM dual UNION ALL
    SELECT 60 FROM dual UNION ALL
    SELECT 100 FROM dual
)
ORDER BY water_depth_cm;

-- Make the demo repeatable without affecting non-demo data.
DELETE FROM claim_status_history
WHERE claim_id IN (
    SELECT claim_id
    FROM motor_flood_claim
    WHERE claim_number = 'ORA-CLM-001'
);

DELETE FROM motor_flood_claim
WHERE claim_number = 'ORA-CLM-001';

COMMIT;

SELECT pkg_flood_claim.fn_calculate_estimated_payout(
    p_sum_assured       => 1000000,
    p_water_depth_cm    => 65,
    p_requested_amount  => 700000,
    p_deductible_amount => 10000,
    p_outstanding_debt  => 25000
) AS estimated_payout
FROM dual;

DECLARE
    v_claim_id         NUMBER;
    v_estimated_payout NUMBER;
BEGIN
    pkg_flood_claim.pr_submit_claim(
        p_claim_number         => 'ORA-CLM-001',
        p_policy_number        => 'ORA-DEMO-001',
        p_vehicle_registration => 'TEST-1001',
        p_incident_date        => DATE '2026-10-05',
        p_district             => 'สาทร',
        p_latitude             => 13.720,
        p_longitude            => 100.533,
        p_water_depth_cm       => 65,
        p_requested_amount     => 700000,
        p_deductible_amount    => 10000,
        p_outstanding_debt     => 25000,
        p_changed_by           => 'PORTFOLIO_DEMO',
        p_claim_id             => v_claim_id,
        p_estimated_payout     => v_estimated_payout
    );

    DBMS_OUTPUT.PUT_LINE('Claim ID: ' || v_claim_id);
    DBMS_OUTPUT.PUT_LINE('Estimated payout: ' || v_estimated_payout);

    COMMIT;
END;
/

SELECT
    claim_number,
    district,
    water_depth_cm,
    requested_amount,
    estimated_payout,
    claim_status
FROM motor_flood_claim
WHERE claim_number = 'ORA-CLM-001';

DECLARE
    v_claim_id NUMBER;
BEGIN
    SELECT claim_id
    INTO v_claim_id
    FROM motor_flood_claim
    WHERE claim_number = 'ORA-CLM-001';

    pkg_flood_claim.pr_approve_claim(
        p_claim_id        => v_claim_id,
        p_approved_amount => 500000,
        p_changed_by      => 'PORTFOLIO_DEMO',
        p_change_note     => 'Approved during SQL Developer demonstration'
    );

    COMMIT;
END;
/

SELECT
    history_id,
    claim_id,
    previous_status,
    new_status,
    changed_by,
    changed_at
FROM claim_status_history
ORDER BY history_id;

-- Spatial proof: the simulated claim point is inside the Sathorn polygon.
SELECT
    claim.claim_number,
    area.area_name,
    SDO_RELATE(
        claim.incident_location,
        area.area_geometry,
        'mask=INSIDE'
    ) AS spatial_relation
FROM motor_flood_claim claim
CROSS JOIN flood_area area
WHERE claim.claim_number = 'ORA-CLM-001'
  AND area.area_name = 'พื้นที่น้ำท่วมจำลองเขตสาทร';

-- Pagination demo compatible with Database Actions.
DECLARE
    v_total_rows NUMBER;
    v_claim_page SYS_REFCURSOR;
BEGIN
    pkg_flood_claim.pr_get_claim_page(
        p_page_number => 1,
        p_page_size   => 10,
        p_district    => 'สาทร',
        p_total_rows  => v_total_rows,
        p_result      => v_claim_page
    );

    DBMS_OUTPUT.PUT_LINE('Total rows: ' || v_total_rows);
    DBMS_SQL.RETURN_RESULT(v_claim_page);
END;
/
