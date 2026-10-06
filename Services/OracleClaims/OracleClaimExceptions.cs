namespace PolicyClaimHub.Services.OracleClaims;

public sealed class OraclePackageException : Exception
{
    public OraclePackageException(
        int errorNumber,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        ErrorNumber = errorNumber;
    }

    public int ErrorNumber { get; }
}

public sealed class OracleIntegrationException : Exception
{
    public OracleIntegrationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
