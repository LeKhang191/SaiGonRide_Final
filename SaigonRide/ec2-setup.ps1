# Script chay tren EC2 (Windows Server)
# Copy script nay vao EC2 va chay bang PowerShell (Run as Administrator)

$appDir   = "C:\inetpub\SaigonRide"
$setupDir = "C:\setup"
$zipPath  = "$env:USERPROFILE\Desktop\saigonride-deploy.zip"

New-Item -ItemType Directory -Force -Path $setupDir | Out-Null
New-Item -ItemType Directory -Force -Path $appDir   | Out-Null

# ─────────────────────────────────────────────
# BUOC 1: Cai IIS
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "=== BUOC 1: Cai IIS ===" -ForegroundColor Cyan
Install-WindowsFeature -Name Web-Server, Web-Mgmt-Tools, Web-Mgmt-Console -IncludeManagementTools | Out-Null
Write-Host "IIS da cai xong." -ForegroundColor Green

# ─────────────────────────────────────────────
# BUOC 2: Cai .NET 10 ASP.NET Core Hosting Bundle
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "=== BUOC 2: Tai .NET 10 Hosting Bundle ===" -ForegroundColor Cyan
$dotnetUrl = "https://aka.ms/dotnet/10.0/dotnet-hosting-win.exe"
$dotnetExe = "$setupDir\dotnet-hosting.exe"
Write-Host "Dang tai... (co the mat 1-2 phut)" -ForegroundColor Gray
Invoke-WebRequest -Uri $dotnetUrl -OutFile $dotnetExe -UseBasicParsing
Write-Host "Dang cai .NET 10..." -ForegroundColor Gray
Start-Process -FilePath $dotnetExe -ArgumentList "/quiet /norestart" -Wait
Write-Host ".NET 10 Hosting Bundle da cai xong." -ForegroundColor Green

# ─────────────────────────────────────────────
# BUOC 3: Cai SQL Server 2022 Express
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "=== BUOC 3: Tai SQL Server 2022 Express ===" -ForegroundColor Cyan
$sqlBootstrap = "$setupDir\SQL2022-SSEI-Expr.exe"
$sqlMedia     = "$setupDir\SQLMedia"
$sqlUrl = "https://download.microsoft.com/download/5/1/4/5145fe04-4d30-4b85-b0d1-39533663a2f1/SQL2022-SSEI-Expr.exe"

Write-Host "Dang tai SQL Server installer... (co the mat vai phut)" -ForegroundColor Gray
Invoke-WebRequest -Uri $sqlUrl -OutFile $sqlBootstrap -UseBasicParsing

Write-Host "Dang tai SQL Server media..." -ForegroundColor Gray
Start-Process -FilePath $sqlBootstrap -ArgumentList "/ACTION=Download /MEDIAPATH=$sqlMedia /MEDIATYPE=Local /QUIET" -Wait

Write-Host "Dang cai SQL Server Express..." -ForegroundColor Gray
$sqlSetup = Get-ChildItem -Path $sqlMedia -Filter "SETUP.EXE" -Recurse | Select-Object -First 1
if ($sqlSetup) {
    Start-Process -FilePath $sqlSetup.FullName -ArgumentList @(
        "/Q",
        "/ACTION=Install",
        "/FEATURES=SQLEngine",
        "/INSTANCENAME=SQLEXPRESS",
        "/TCPENABLED=1",
        "/SQLSYSADMINACCOUNTS=BUILTIN\Administrators",
        "/IACCEPTSQLSERVERLICENSETERMS"
    ) -Wait
    Write-Host "SQL Server Express da cai xong." -ForegroundColor Green
} else {
    Write-Host "Khong tim thay SETUP.EXE. Thu cach khac..." -ForegroundColor Yellow
    # Fallback: dung LocalDB
    $localDbUrl = "https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4dd2-b7f7-d2c1e80f6bcc/SqlLocalDB.msi"
    Invoke-WebRequest -Uri $localDbUrl -OutFile "$setupDir\SqlLocalDB.msi" -UseBasicParsing
    Start-Process msiexec -ArgumentList "/i $setupDir\SqlLocalDB.msi /quiet" -Wait
    Write-Host "SQL LocalDB da cai xong." -ForegroundColor Green
}

# ─────────────────────────────────────────────
# BUOC 4: Giai nen app vao thu muc
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "=== BUOC 4: Giai nen app ===" -ForegroundColor Cyan
if (Test-Path $zipPath) {
    Expand-Archive -Path $zipPath -DestinationPath $appDir -Force
    New-Item -ItemType Directory -Force -Path "$appDir\logs" | Out-Null
    Write-Host "App da giai nen vao $appDir" -ForegroundColor Green
} else {
    Write-Host "CANH BAO: Khong tim thay file $zipPath" -ForegroundColor Yellow
    Write-Host "Hay copy file saigonride-deploy.zip vao Desktop roi chay lai script nay." -ForegroundColor Yellow
}

# ─────────────────────────────────────────────
# BUOC 5: Cau hinh IIS
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "=== BUOC 5: Cau hinh IIS ===" -ForegroundColor Cyan
Import-Module WebAdministration

# Xoa Default Web Site neu co
if (Get-Website -Name "Default Web Site" -ErrorAction SilentlyContinue) {
    Remove-Website -Name "Default Web Site"
}

# Xoa site cu neu co
if (Get-Website -Name "SaigonRide" -ErrorAction SilentlyContinue) {
    Remove-Website -Name "SaigonRide"
}

# Tao App Pool moi
if (Get-WebConfiguration "system.applicationHost/applicationPools/add[@name='SaigonRide']") {
    Remove-WebAppPool -Name "SaigonRide"
}
New-WebAppPool -Name "SaigonRide"
Set-ItemProperty "IIS:\AppPools\SaigonRide" managedRuntimeVersion ""
Set-ItemProperty "IIS:\AppPools\SaigonRide" processModel.identityType LocalSystem

# Tao Website
New-Website -Name "SaigonRide" -Port 80 -PhysicalPath $appDir -ApplicationPool "SaigonRide" -Force
Write-Host "IIS da cau hinh xong." -ForegroundColor Green

# ─────────────────────────────────────────────
# BUOC 6: Reset IIS
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "=== BUOC 6: Khoi dong lai IIS ===" -ForegroundColor Cyan
iisreset /restart
Write-Host "IIS da khoi dong lai." -ForegroundColor Green

# ─────────────────────────────────────────────
# HOAN THANH
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host " CAI DAT HOAN THANH!" -ForegroundColor Green
Write-Host " Mo trinh duyet va vao: http://54.255.200.246" -ForegroundColor White
Write-Host ""
Write-Host " Neu app loi, xem log tai:" -ForegroundColor Yellow
Write-Host " $appDir\logs\" -ForegroundColor White
Write-Host "=============================================" -ForegroundColor Green
