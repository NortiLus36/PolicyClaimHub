param(
    [string]$WalletPath = "C:\Users\admin\Downloads\wallet",
    [string]$ServiceName = "policyclaimhub_tp"
)

$ErrorActionPreference = "Stop"
$projectPath = [IO.Path]::GetFullPath(
    (Join-Path -Path $PSScriptRoot -ChildPath "..\PolicyClaimHub.csproj"))

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "Project file was not found at $projectPath"
}

$requiredFiles = @("cwallet.sso", "sqlnet.ora", "tnsnames.ora")
foreach ($fileName in $requiredFiles) {
    $filePath = Join-Path -Path $WalletPath -ChildPath $fileName
    if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
        throw "Required wallet file $fileName was not found in $WalletPath"
    }
}

$securePassword = Read-Host "Enter the Oracle database password for ADMIN" -AsSecureString
$passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)

try {
    $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)
    $connectionString = "User Id=ADMIN;Password=$plainPassword;Data Source=$ServiceName;Tns_Admin=$WalletPath"

    & dotnet user-secrets set `
        "ConnectionStrings:OracleConnection" `
        $connectionString `
        --project $projectPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet user-secrets failed (exit code $LASTEXITCODE)"
    }

    Write-Host "OracleConnection was saved in User Secrets successfully." -ForegroundColor Green
    Write-Host "Wallet: $WalletPath"
    Write-Host "Service: $ServiceName"
}
finally {
    if ($null -ne $passwordPointer) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
    }

    $plainPassword = $null
    $connectionString = $null
    $securePassword = $null
}
