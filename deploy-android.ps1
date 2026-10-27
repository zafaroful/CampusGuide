#!/usr/bin/env pwsh
param([string]$emulatorName = "", [switch]$rebuild = $false, [switch]$uninstall = $false, [switch]$logonly = $false)

$projectRoot = "$PSScriptRoot"
$projectPath = "$projectRoot\CampusGuide\CampusGuide.csproj"
$packageId = "com.companyname.campusguide"
$mainActivity = "com.companyname.campusguide.MainActivity"

function Write-Status { param([string]$msg, [string]$type = "Info")
    $c = @{Success="Green"; Error="Red"; Warning="Yellow"; Info="Cyan"}
    Write-Host "[$(Get-Date -Format HH:mm:ss)] " -NoNewline -ForegroundColor Gray
    Write-Host $msg -ForegroundColor $c[$type]
}

Write-Host ""; Write-Host ("=" * 70) -ForegroundColor Magenta
Write-Host "CampusGuide Android Deployment" -ForegroundColor Magenta
Write-Host ("=" * 70) -ForegroundColor Magenta; Write-Host ""

Write-Status "SDK: $env:ANDROID_SDK_ROOT" Info

if (-not $logonly) {
    Write-Host "" ; Write-Host ("=" * 70) -ForegroundColor Magenta
    Write-Host "Building APK" -ForegroundColor Magenta
    Write-Host ("=" * 70) -ForegroundColor Magenta; Write-Host ""
    
    if ($rebuild) {
        Write-Status "Cleaning..." Info
        $binFolder = "$projectRoot\CampusGuide\bin"
        $objFolder = "$projectRoot\CampusGuide\obj"
        if (Test-Path $binFolder) { Remove-Item -Recurse -Force $binFolder }
        if (Test-Path $objFolder) { Remove-Item -Recurse -Force $objFolder }
    }
    
    Write-Status "Building..." Info
    & dotnet build -c Debug -f net10.0-android "$projectPath"
    
    if ($LASTEXITCODE -ne 0) { 
        Write-Status "Build failed!" Error
        exit 1 
    }
    Write-Status "Build successful!" Success
    
    Write-Host ""; Write-Host ("=" * 70) -ForegroundColor Magenta
    Write-Host "Finding APK" -ForegroundColor Magenta
    Write-Host ("=" * 70) -ForegroundColor Magenta; Write-Host ""
    
    $binFolder = "$projectRoot\CampusGuide\bin\Debug\net10.0-android"
    $apk = Get-ChildItem -Path $binFolder -Filter "*debug.apk" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if (-not $apk) {
        Write-Status "APK not found!" Error
        exit 1
    }
    Write-Status "Found: $($apk.Name)" Success
    
    Write-Host ""; Write-Host ("=" * 70) -ForegroundColor Magenta
    Write-Host "Checking Emulator" -ForegroundColor Magenta
    Write-Host ("=" * 70) -ForegroundColor Magenta; Write-Host ""
    
    if ($emulatorName -eq "") {
        $running = @(adb devices 2>$null | Select-Object -Skip 1 | Where-Object { $_ -match "emulator" })
        if ($running.Count -eq 0) {
            Write-Status "No emulator running" Warning
            $avds = @(emulator -list-avds 2>$null)
            if ($avds.Count -gt 0) {
                $emulatorName = $avds[0]
                Write-Status "Starting: $emulatorName" Info
                Start-Process emulator -ArgumentList "-avd", $emulatorName -WindowStyle Minimized
                Start-Sleep -Seconds 15
            } else {
                Write-Status "No emulators found!" Error
                exit 1
            }
        } else {
            $emulatorName = ($running[0] -split "\s+")[0]
            Write-Status "Using: $emulatorName" Success
        }
    }
    
    Write-Host ""; Write-Host ("=" * 70) -ForegroundColor Magenta
    Write-Host "Installing APK" -ForegroundColor Magenta
    Write-Host ("=" * 70) -ForegroundColor Magenta; Write-Host ""
    
    if ($uninstall) {
        adb -s $emulatorName uninstall $packageId 2>$null
    }
    
    Write-Status "Installing..." Info
    adb -s $emulatorName install -r "$($apk.FullName)"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Status "Install failed!" Error
        exit 1
    }
    Write-Status "Install successful!" Success
    
    Write-Host ""; Write-Host ("=" * 70) -ForegroundColor Magenta
    Write-Host "Launching App" -ForegroundColor Magenta
    Write-Host ("=" * 70) -ForegroundColor Magenta; Write-Host ""
    
    adb -s $emulatorName shell am start -n "$packageId/$mainActivity"
    Start-Sleep -Seconds 3
    Write-Status "App launched!" Success
}

Write-Host ""; Write-Host ("=" * 70) -ForegroundColor Magenta
Write-Host "Logs (Ctrl+C to stop)" -ForegroundColor Magenta
Write-Host ("=" * 70) -ForegroundColor Magenta; Write-Host ""
adb logcat
