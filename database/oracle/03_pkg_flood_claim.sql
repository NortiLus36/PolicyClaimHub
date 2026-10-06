CREATE OR REPLACE PACKAGE pkg_flood_claim AS
    FUNCTION fn_calculate_estimated_payout (
        p_sum_assured       IN NUMBER,
        p_water_depth_cm    IN NUMBER,
        p_requested_amount  IN NUMBER,
        p_deductible_amount IN NUMBER,
        p_outstanding_debt  IN NUMBER
    ) RETURN NUMBER DETERMINISTIC;

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
    );

    PROCEDURE pr_approve_claim (
        p_claim_id       IN NUMBER,
        p_approved_amount IN NUMBER,
        p_changed_by     IN VARCHAR2,
        p_change_note    IN VARCHAR2 DEFAULT NULL
    );

    PROCEDURE pr_get_claim_page (
        p_page_number IN PLS_INTEGER,
        p_page_size   IN PLS_INTEGER,
        p_district    IN VARCHAR2,
        p_total_rows  OUT NUMBER,
        p_result      OUT SYS_REFCURSOR
    );
END pkg_flood_claim;
/

CREATE OR REPLACE PACKAGE BODY pkg_flood_claim AS
    FUNCTION fn_calculate_estimated_payout (
        p_sum_assured       IN NUMBER,
        p_water_depth_cm    IN NUMBER,
        p_requested_amount  IN NUMBER,
        p_deductible_amount IN NUMBER,
        p_outstanding_debt  IN NUMBER
    ) RETURN NUMBER DETERMINISTIC
    IS
        -- Percentage of the sum assured used as the payout ceiling.
        v_damage_rate NUMBER(5, 4);
        -- Estimated loss before deductible and outstanding debt are deducted.
        v_gross_loss  NUMBER(18, 2);
        -- Final estimated payout after all deductions.
        v_net_payout  NUMBER(18, 2);
    BEGIN
        IF p_sum_assured <= 0 OR p_requested_amount <= 0 THEN
            RAISE_APPLICATION_ERROR(
                -20010,
                'Sum assured and requested amount must be greater than zero.'
            );
        END IF;

        IF p_water_depth_cm < 0 OR p_water_depth_cm > 500 THEN
            RAISE_APPLICATION_ERROR(
                -20011,
                'Water depth must be between 0 and 500 centimetres.'
            );
        END IF;

        v_damage_rate := CASE
            WHEN p_water_depth_cm < 20 THEN 0.00
            WHEN p_water_depth_cm < 40 THEN 0.15
            WHEN p_water_depth_cm < 60 THEN 0.35
            WHEN p_water_depth_cm < 100 THEN 0.60
            ELSE 0.85
        END;

        v_gross_loss := LEAST(
            p_requested_amount,
            p_sum_assured * v_damage_rate
        );

        v_net_payout := GREATEST(
            0,
            v_gross_loss
                - NVL(p_deductible_amount, 0)
                - NVL(p_outstanding_debt, 0)
        );

        RETURN ROUND(LEAST(v_net_payout, p_sum_assured), 2);
    END fn_calculate_estimated_payout;

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
        -- Primary key of the policy found from p_policy_number.
        v_policy_id           insurance_policy.policy_id%TYPE;
        -- Sum assured used as the base of the payout calculation.
        v_sum_assured         insurance_policy.sum_assured%TYPE;
        -- Current policy status; only ACTIVE policies can submit a claim.
        v_policy_status       insurance_policy.policy_status%TYPE;
        -- First date on which the policy provides coverage.
        v_coverage_start_date insurance_policy.coverage_start_date%TYPE;
        -- Last date on which the policy provides coverage.
        v_coverage_end_date   insurance_policy.coverage_end_date%TYPE;
    BEGIN
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
        WHERE policy_number = UPPER(TRIM(p_policy_number));

        IF v_policy_status <> 'ACTIVE' THEN
            RAISE_APPLICATION_ERROR(
                -20001,
                'Policy is not active.'
            );
        END IF;

        IF TRUNC(p_incident_date) NOT BETWEEN
            TRUNC(v_coverage_start_date) AND TRUNC(v_coverage_end_date)
        THEN
            RAISE_APPLICATION_ERROR(
                -20002,
                'Incident date is outside the coverage period.'
            );
        END IF;

        p_estimated_payout := fn_calculate_estimated_payout(
            v_sum_assured,
            p_water_depth_cm,
            p_requested_amount,
            p_deductible_amount,
            p_outstanding_debt
        );

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
            UPPER(TRIM(p_claim_number)),
            v_policy_id,
            UPPER(TRIM(p_vehicle_registration)),
            p_incident_date,
            TRIM(p_district),
            MDSYS.SDO_GEOMETRY(
                2001,
                4326,
                MDSYS.SDO_POINT_TYPE(p_longitude, p_latitude, NULL),
                NULL,
                NULL
            ),
            p_water_depth_cm,
            p_requested_amount,
            NVL(p_deductible_amount, 0),
            NVL(p_outstanding_debt, 0),
            p_estimated_payout,
            'SUBMITTED'
        )
        RETURNING claim_id INTO p_claim_id;

        INSERT INTO claim_status_history (
            claim_id,
            previous_status,
            new_status,
            changed_by,
            change_note
        )
        VALUES (
            p_claim_id,
            NULL,
            'SUBMITTED',
            p_changed_by,
            'Claim submitted through PKG_FLOOD_CLAIM'
        );
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20003, 'Policy number was not found.');
        WHEN DUP_VAL_ON_INDEX THEN
            RAISE_APPLICATION_ERROR(-20004, 'Claim number already exists.');
    END pr_submit_claim;

    PROCEDURE pr_approve_claim (
        p_claim_id        IN NUMBER,
        p_approved_amount IN NUMBER,
        p_changed_by      IN VARCHAR2,
        p_change_note     IN VARCHAR2 DEFAULT NULL
    )
    IS
        -- Claim status before approval, retained for validation and audit.
        v_previous_status  motor_flood_claim.claim_status%TYPE;
        -- Maximum amount calculated by the estimation function.
        v_estimated_payout motor_flood_claim.estimated_payout%TYPE;
    BEGIN
        SELECT claim_status, estimated_payout
        INTO v_previous_status, v_estimated_payout
        FROM motor_flood_claim
        WHERE claim_id = p_claim_id
        FOR UPDATE;

        IF v_previous_status NOT IN ('SUBMITTED', 'UNDER_REVIEW') THEN
            RAISE_APPLICATION_ERROR(
                -20005,
                'Claim status does not allow approval.'
            );
        END IF;

        IF p_approved_amount < 0 OR
           p_approved_amount > v_estimated_payout
        THEN
            RAISE_APPLICATION_ERROR(
                -20006,
                'Approved amount must be between zero and estimated payout.'
            );
        END IF;

        UPDATE motor_flood_claim
        SET approved_amount = p_approved_amount,
            claim_status = 'APPROVED',
            updated_at = SYSTIMESTAMP
        WHERE claim_id = p_claim_id;

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
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20007, 'Claim was not found.');
    END pr_approve_claim;

    PROCEDURE pr_get_claim_page (
        p_page_number IN PLS_INTEGER,
        p_page_size   IN PLS_INTEGER,
        p_district    IN VARCHAR2,
        p_total_rows  OUT NUMBER,
        p_result      OUT SYS_REFCURSOR
    )
    IS
        -- Number of rows to skip before reading the requested page.
        v_offset PLS_INTEGER;
    BEGIN
        IF p_page_number < 1 THEN
            RAISE_APPLICATION_ERROR(-20020, 'Page number must be at least 1.');
        END IF;

        IF p_page_size < 1 OR p_page_size > 100 THEN
            RAISE_APPLICATION_ERROR(-20021, 'Page size must be between 1 and 100.');
        END IF;

        v_offset := (p_page_number - 1) * p_page_size;

        SELECT COUNT(*)
        INTO p_total_rows
        FROM motor_flood_claim claim
        WHERE p_district IS NULL
           OR claim.district = TRIM(p_district);

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
            JOIN insurance_policy policy
                ON policy.policy_id = claim.policy_id
            WHERE p_district IS NULL
               OR claim.district = TRIM(p_district)
            ORDER BY claim.incident_date DESC, claim.claim_id DESC
            OFFSET v_offset ROWS
            FETCH NEXT p_page_size ROWS ONLY;
    END pr_get_claim_page;
END pkg_flood_claim;
/

-- Transaction control intentionally belongs to the caller.
-- Use COMMIT after a successful demo call or ROLLBACK when testing errors.
