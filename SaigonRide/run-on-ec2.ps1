Write-Host "===== BAT DAU CAI DAT SAIGONRIDE =====" -ForegroundColor Cyan

# 1. Cai Chocolatey
Write-Host "[1/6] Cai Chocolatey..." -ForegroundColor Yellow
if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
    Set-ExecutionPolicy Bypass -Scope Process -Force
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
    Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH","User")
    Write-Host "    Chocolatey xong!" -ForegroundColor Green
} else {
    Write-Host "    Chocolatey da co san." -ForegroundColor Green
}

# 2. Cai SQL Server Express qua Chocolatey
Write-Host "[2/6] Cai SQL Server 2022 Express (10-15 phut)..." -ForegroundColor Yellow
choco install sql-server-2022 --params="'/InstanceName:SQLEXPRESS'" -y --no-progress
if ($LASTEXITCODE -ne 0) {
    Write-Host "    Thu cach khac..." -ForegroundColor Yellow
    choco install sql-server-express -y --no-progress
}
$env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH","User")
Write-Host "    SQL Server xong!" -ForegroundColor Green

# 3. Bat SQL Server
Write-Host "[3/6] Khoi dong SQL Server..." -ForegroundColor Yellow
$sqlSvc = Get-Service -Name "MSSQL*" | Select-Object -First 1
if ($sqlSvc) {
    Set-Service $sqlSvc.Name -StartupType Automatic
    Start-Service $sqlSvc.Name -ErrorAction SilentlyContinue
    Write-Host "    $($sqlSvc.Name): $((Get-Service $sqlSvc.Name).Status)" -ForegroundColor Green
} else {
    Write-Host "    CANH BAO: Khong tim thay SQL Server service!" -ForegroundColor Red
}

# 4. Giai nen app
Write-Host "[4/6] Giai nen app..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path "C:\inetpub\SaigonRide" | Out-Null
New-Item -ItemType Directory -Force -Path "C:\inetpub\SaigonRide\logs" | Out-Null
if (Test-Path "C:\saigonride-deploy.zip") {
    Expand-Archive "C:\saigonride-deploy.zip" -DestinationPath "C:\inetpub\SaigonRide" -Force
    Write-Host "    Giai nen xong!" -ForegroundColor Green
} else {
    Write-Host "    THAT BAI: Khong tim thay C:\saigonride-deploy.zip" -ForegroundColor Red
    Write-Host "    Hay copy file saigonride-deploy.zip vao o C: roi chay lai." -ForegroundColor Yellow
    exit 1
}

# 5. Sua web.config va appsettings
Write-Host "[5/6] Cau hinh web.config..." -ForegroundColor Yellow
$sqlSvcName = (Get-Service -Name "MSSQL*" -ErrorAction SilentlyContinue | Select-Object -First 1).Name
$instanceName = if ($sqlSvcName) { $sqlSvcName.Replace("MSSQL`$","") } else { "SQLEXPRESS" }
$connStr = "Server=.\\$instanceName;Database=SaigonRideDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
Write-Host "    Connection string: $connStr" -ForegroundColor Gray

$appsettingsPath = "C:\inetpub\SaigonRide\appsettings.Production.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath -Raw
    $appsettings = $appsettings -replace '"DefaultConnection":\s*"[^"]*"', "`"DefaultConnection`": `"$connStr`""
    Set-Content $appsettingsPath -Value $appsettings -Encoding UTF8
}

$wc = '<?xml version="1.0" encoding="utf-8"?><configuration><system.webServer><handlers><add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified"/></handlers><aspNetCore processPath="dotnet" arguments=".\SaigonRide.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="inprocess"><environmentVariables><environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production"/></environmentVariables></aspNetCore></system.webServer></configuration>'
Set-Content "C:\inetpub\SaigonRide\web.config" -Value $wc -Encoding UTF8
Write-Host "    Cau hinh xong!" -ForegroundColor Green

# 6. Cau hinh IIS
Write-Host "[6/6] Cau hinh IIS..." -ForegroundColor Yellow
Import-Module WebAdministration
Remove-Website -Name "Default Web Site" -ErrorAction SilentlyContinue
if (Get-WebAppPool -Name "SaigonRide" -ErrorAction SilentlyContinue) { Remove-WebAppPool -Name "SaigonRide" }
New-WebAppPool -Name "SaigonRide"
Set-ItemProperty "IIS:\AppPools\SaigonRide" managedRuntimeVersion ""
Set-ItemProperty "IIS:\AppPools\SaigonRide" processModel.identityType LocalSystem
if (Get-Website -Name "SaigonRide" -ErrorAction SilentlyContinue) { Remove-Website -Name "SaigonRide" }
New-Website -Name "SaigonRide" -Port 80 -PhysicalPath "C:\inetpub\SaigonRide" -ApplicationPool "SaigonRide"
iisreset /restart | Out-Null
Write-Host "    IIS xong!" -ForegroundColor Green

Write-Host ""
Write-Host "===== HOAN THANH! =====" -ForegroundColor Green
Write-Host "Vao trinh duyet: http://54.255.200.246" -ForegroundColor Cyan
Write-Host "Neu loi, xem log tai: C:\inetpub\SaigonRide\logs\" -ForegroundColor Yellow
