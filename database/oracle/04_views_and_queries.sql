CREATE OR REPLACE VIEW vw_district_claim_exposure AS
SELECT
    district,
    COUNT(*) AS claim_count,
    SUM(requested_amount) AS total_requested_amount,
    SUM(estimated_payout) AS total_estimated_payout,
    SUM(NVL(approved_amount, 0)) AS total_approved_amount,
    ROUND(AVG(water_depth_cm), 2) AS average_water_depth_cm,
    CASE MAX(CASE
        WHEN water_depth_cm >= 100 THEN 4
        WHEN water_depth_cm >= 50 THEN 3
        WHEN water_depth_cm >= 20 THEN 2
        ELSE 1
    END)
        WHEN 4 THEN 'SEVERE'
        WHEN 3 THEN 'HIGH'
        WHEN 2 THEN 'MEDIUM'
        ELSE 'LOW'
    END AS highest_severity
FROM motor_flood_claim
GROUP BY district;
/

-- Complex dashboard query using CTE, CASE, aggregate and analytic ranking.
WITH claim_metrics AS (
    SELECT
        claim_id,
        district,
        requested_amount,
        estimated_payout,
        water_depth_cm,
        CASE
            WHEN water_depth_cm >= 100 THEN 'SEVERE'
            WHEN water_depth_cm >= 50 THEN 'HIGH'
            WHEN water_depth_cm >= 20 THEN 'MEDIUM'
            ELSE 'LOW'
        END AS severity
    FROM motor_flood_claim
    WHERE claim_status NOT IN ('REJECTED')
),
district_totals AS (
    SELECT
        district,
        COUNT(*) AS claim_count,
        SUM(requested_amount) AS total_requested_amount,
        SUM(estimated_payout) AS total_estimated_payout,
        SUM(CASE WHEN severity IN ('HIGH', 'SEVERE') THEN 1 ELSE 0 END)
            AS high_risk_claim_count
    FROM claim_metrics
    GROUP BY district
)
SELECT
    district,
    claim_count,
    total_requested_amount,
    total_estimated_payout,
    high_risk_claim_count,
    ROUND(
        total_estimated_payout / NULLIF(total_requested_amount, 0) * 100,
        2
    ) AS estimated_loss_percentage,
    DENSE_RANK() OVER (
        ORDER BY total_estimated_payout DESC
    ) AS exposure_rank
FROM district_totals
ORDER BY exposure_rank, district;

-- Spatial matching: find claims located inside a flood polygon.
SELECT
    claim.claim_number,
    claim.district,
    claim.estimated_payout,
    area.area_name,
    area.severity AS flood_area_severity
FROM motor_flood_claim claim
JOIN flood_area area
    ON SDO_RELATE(
        area.area_geometry,
        claim.incident_location,
        'mask=CONTAINS querytype=WINDOW'
    ) = 'TRUE'
ORDER BY area.area_name, claim.estimated_payout DESC;
