# Script chay tren MAY CUA BAN (khong phai EC2)
# Mo PowerShell, cd vao thu muc project, chay: .\deploy-local.ps1

$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishDir = "$projectDir\publish"
$zipPath    = "$projectDir\saigonride-deploy.zip"

Write-Host ""
Write-Host "=== BUOC 1: Build va publish app ===" -ForegroundColor Cyan
dotnet publish "$projectDir\SaigonRide.csproj" -c Release -r win-x64 --self-contained false -o $publishDir
if ($LASTEXITCODE -ne 0) { Write-Host "Loi khi publish!" -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "=== BUOC 2: Ghi de web.config de chay tren IIS ===" -ForegroundColor Cyan
$webConfig = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified"/>
    </handlers>
    <aspNetCore processPath="dotnet"
                arguments=".\SaigonRide.dll"
                stdoutLogEnabled="true"
                stdoutLogFile=".\logs\stdout"
                hostingModel="inprocess">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production"/>
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
'@
Set-Content -Path "$publishDir\web.config" -Value $webConfig -Encoding UTF8

Write-Host ""
Write-Host "=== BUOC 3: Nen thanh file zip ===" -ForegroundColor Cyan
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath
Write-Host "Da tao: $zipPath" -ForegroundColor Green

Write-Host ""
Write-Host "=============================================" -ForegroundColor Yellow
Write-Host " XONG! File deploy da san sang." -ForegroundColor Green
Write-Host " File: $zipPath" -ForegroundColor White
Write-Host ""
Write-Host " Tiep theo:" -ForegroundColor Yellow
Write-Host " 1. Mo Remote Desktop (mstsc) vao 54.255.200.246" -ForegroundColor White
Write-Host " 2. Trong hop thoai RDP, click 'Show Options'" -ForegroundColor White
Write-Host " 3. Tab 'Local Resources' > 'More' > tick o dia C:" -ForegroundColor White
Write-Host " 4. Ket noi vao EC2" -ForegroundColor White
Write-Host " 5. Trong EC2, mo File Explorer > go vao \\tsclient\C" -ForegroundColor White
Write-Host " 6. Copy file saigonride-deploy.zip vao Desktop cua EC2" -ForegroundColor White
Write-Host " 7. Chay ec2-setup.ps1 tren EC2" -ForegroundColor White
Write-Host "=============================================" -ForegroundColor Yellow
