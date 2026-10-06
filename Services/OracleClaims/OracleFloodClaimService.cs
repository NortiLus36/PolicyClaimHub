using System.Data;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace PolicyClaimHub.Services.OracleClaims;

public sealed class OracleFloodClaimService : IOracleFloodClaimService
{
    private const string PackageName = "PKG_FLOOD_CLAIM";
    private readonly string? _connectionString;
    private readonly ILogger<OracleFloodClaimService> _logger;

    public OracleFloodClaimService(
        IConfiguration configuration,
        ILogger<OracleFloodClaimService> logger)
    {
        _connectionString = configuration
            .GetConnectionString("OracleConnection");
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

    public async Task<OracleDatabaseStatus> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return new OracleDatabaseStatus(
                false,
                false,
                "NOT_CONFIGURED",
                "ยังไม่ได้ตั้งค่า OracleConnection ใน User Secrets");
        }

        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.BindByName = true;
            command.CommandText = """
                SELECT COUNT(*)
                FROM user_objects
                WHERE object_name = :package_name
                  AND object_type IN ('PACKAGE', 'PACKAGE BODY')
                  AND status = 'VALID'
                """;
            command.Parameters.Add(
                "package_name",
                OracleDbType.Varchar2,
                PackageName,
                ParameterDirection.Input);

            var validObjectCount = Convert.ToInt32(
                await command.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture);
            var packageStatus = validObjectCount == 2 ? "VALID" : "INVALID";

            return new OracleDatabaseStatus(
                true,
                true,
                packageStatus,
                packageStatus == "VALID"
                    ? "เชื่อมต่อ Oracle และตรวจพบ Package ที่พร้อมใช้งาน"
                    : "เชื่อมต่อได้ แต่ Package Specification หรือ Body ยังไม่ VALID");
        }
        catch (OracleException exception)
        {
            _logger.LogError(
                exception,
                "Unable to check Oracle package status. Oracle error {ErrorNumber}",
                exception.Number);

            return new OracleDatabaseStatus(
                true,
                false,
                "UNAVAILABLE",
                "เชื่อมต่อ Oracle ไม่สำเร็จ กรุณาตรวจ Wallet, Connection String และสถานะฐานข้อมูล");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to initialize the Oracle connection");

            return new OracleDatabaseStatus(
                true,
                false,
                "UNAVAILABLE",
                "ตั้งค่า Oracle ยังไม่สมบูรณ์ กรุณาตรวจ Wallet และ Connection String");
        }
    }

    public async Task<OracleClaimPage> GetPageAsync(
        int pageNumber,
        int pageSize,
        string? district,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = CreatePackageCommand(
                connection,
                $"{PackageName}.PR_GET_CLAIM_PAGE");
            command.Parameters.Add(
                "p_page_number",
                OracleDbType.Int32,
                pageNumber,
                ParameterDirection.Input);
            command.Parameters.Add(
                "p_page_size",
                OracleDbType.Int32,
                pageSize,
                ParameterDirection.Input);
            command.Parameters.Add(
                "p_district",
                OracleDbType.Varchar2,
                string.IsNullOrWhiteSpace(district)
                    ? DBNull.Value
                    : district.Trim(),
                ParameterDirection.Input);
            var totalRowsParameter = command.Parameters.Add(
                "p_total_rows",
                OracleDbType.Int32,
                ParameterDirection.Output);
            command.Parameters.Add(
                "p_result",
                OracleDbType.RefCursor,
                ParameterDirection.Output);

            var items = new List<OracleClaimPageItem>();
            await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    items.Add(new OracleClaimPageItem(
                        Convert.ToInt64(reader.GetDecimal(0)),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetDateTime(4),
                        reader.GetString(5),
                        reader.GetDecimal(6),
                        reader.GetDecimal(7),
                        reader.GetDecimal(8),
                        reader.GetString(9)));
                }
            }

            return new OracleClaimPage(
                items,
                pageNumber,
                pageSize,
                ToInt32(totalRowsParameter.Value));
        }
        catch (OracleException exception)
        {
            throw TranslateOracleException(
                exception,
                "ไม่สามารถอ่านรายการเคลมจาก Oracle ได้");
        }
    }

    public async Task<OracleSubmitClaimResult> SubmitAsync(
        OracleSubmitClaimCommand request,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            await using var transaction = connection.BeginTransaction();

            await using var command = CreatePackageCommand(
                connection,
                $"{PackageName}.PR_SUBMIT_CLAIM");
            command.Transaction = transaction;
            AddSubmitParameters(command, request);
            var claimIdParameter = command.Parameters.Add(
                "p_claim_id",
                OracleDbType.Int64,
                ParameterDirection.Output);
            var estimatedPayoutParameter = command.Parameters.Add(
                "p_estimated_payout",
                OracleDbType.Decimal,
                ParameterDirection.Output);

            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new OracleSubmitClaimResult(
                ToInt64(claimIdParameter.Value),
                ToDecimal(estimatedPayoutParameter.Value));
        }
        catch (OracleException exception)
        {
            throw TranslateOracleException(
                exception,
                "ไม่สามารถยื่นเคลมผ่าน Oracle Package ได้");
        }
    }

    public async Task ApproveAsync(
        OracleApproveClaimCommand request,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            await using var transaction = connection.BeginTransaction();

            await using var command = CreatePackageCommand(
                connection,
                $"{PackageName}.PR_APPROVE_CLAIM");
            command.Transaction = transaction;
            command.Parameters.Add(
                "p_claim_id",
                OracleDbType.Int64,
                request.ClaimId,
                ParameterDirection.Input);
            command.Parameters.Add(
                "p_approved_amount",
                OracleDbType.Decimal,
                request.ApprovedAmount,
                ParameterDirection.Input);
            command.Parameters.Add(
                "p_changed_by",
                OracleDbType.Varchar2,
                request.ChangedBy.Trim(),
                ParameterDirection.Input);
            command.Parameters.Add(
                "p_change_note",
                OracleDbType.Varchar2,
                string.IsNullOrWhiteSpace(request.ChangeNote)
                    ? DBNull.Value
                    : request.ChangeNote.Trim(),
                ParameterDirection.Input);

            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (OracleException exception)
        {
            throw TranslateOracleException(
                exception,
                "ไม่สามารถอนุมัติเคลมผ่าน Oracle Package ได้");
        }
    }

    private OracleConnection CreateConnection()
    {
        return new OracleConnection(_connectionString);
    }

    private static OracleCommand CreatePackageCommand(
        OracleConnection connection,
        string procedureName)
    {
        return new OracleCommand(procedureName, connection)
        {
            BindByName = true,
            CommandType = CommandType.StoredProcedure
        };
    }

    private static void AddSubmitParameters(
        OracleCommand command,
        OracleSubmitClaimCommand request)
    {
        command.Parameters.Add("p_claim_number", OracleDbType.Varchar2, request.ClaimNumber.Trim().ToUpperInvariant(), ParameterDirection.Input);
        command.Parameters.Add("p_policy_number", OracleDbType.Varchar2, request.PolicyNumber.Trim().ToUpperInvariant(), ParameterDirection.Input);
        command.Parameters.Add("p_vehicle_registration", OracleDbType.Varchar2, request.VehicleRegistration.Trim().ToUpperInvariant(), ParameterDirection.Input);
        command.Parameters.Add("p_incident_date", OracleDbType.Date, request.IncidentDate, ParameterDirection.Input);
        command.Parameters.Add("p_district", OracleDbType.Varchar2, request.District.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_latitude", OracleDbType.Decimal, request.Latitude, ParameterDirection.Input);
        command.Parameters.Add("p_longitude", OracleDbType.Decimal, request.Longitude, ParameterDirection.Input);
        command.Parameters.Add("p_water_depth_cm", OracleDbType.Decimal, request.WaterDepthCm, ParameterDirection.Input);
        command.Parameters.Add("p_requested_amount", OracleDbType.Decimal, request.RequestedAmount, ParameterDirection.Input);
        command.Parameters.Add("p_deductible_amount", OracleDbType.Decimal, request.DeductibleAmount, ParameterDirection.Input);
        command.Parameters.Add("p_outstanding_debt", OracleDbType.Decimal, request.OutstandingDebt, ParameterDirection.Input);
        command.Parameters.Add("p_changed_by", OracleDbType.Varchar2, request.ChangedBy.Trim(), ParameterDirection.Input);
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "ยังไม่ได้ตั้งค่า ConnectionStrings:OracleConnection ใน User Secrets");
        }
    }

    private Exception TranslateOracleException(
        OracleException exception,
        string integrationMessage)
    {
        if (exception.Number is >= 20000 and <= 20999)
        {
            return new OraclePackageException(
                exception.Number,
                ExtractBusinessMessage(exception.Message),
                exception);
        }

        _logger.LogError(
            exception,
            "Oracle integration failed with error {ErrorNumber}",
            exception.Number);
        return new OracleIntegrationException(integrationMessage, exception);
    }

    private static string ExtractBusinessMessage(string oracleMessage)
    {
        var firstLine = oracleMessage
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(firstLine))
        {
            return "Oracle Package ปฏิเสธรายการตามกฎธุรกิจ";
        }

        var separatorIndex = firstLine.IndexOf(':');
        return separatorIndex >= 0
            ? firstLine[(separatorIndex + 1)..].Trim()
            : firstLine.Trim();
    }

    private static int ToInt32(object value)
    {
        return checked((int)ToDecimal(value));
    }

    private static long ToInt64(object value)
    {
        return checked((long)ToDecimal(value));
    }

    private static decimal ToDecimal(object value)
    {
        return value switch
        {
            OracleDecimal oracleDecimal => oracleDecimal.Value,
            decimal decimalValue => decimalValue,
            _ => Convert.ToDecimal(value, CultureInfo.InvariantCulture)
        };
    }
}
