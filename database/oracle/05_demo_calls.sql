SET SERVEROUTPUT ON;

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
        p_district             => 'บางเขน',
        p_latitude             => 13.8739,
        p_longitude            => 100.5964,
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
