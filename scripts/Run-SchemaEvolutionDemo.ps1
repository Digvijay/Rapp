#Requires -Version 7.0
<#
.SYNOPSIS
    Runs the Rapp Schema Evolution Safety Demo

.DESCRIPTION
    This script starts the Rapp Playground application and demonstrates
    the difference between MemoryPack crashes and Rapp's safe handling
    of schema evolution scenarios.

.PARAMETER Endpoint
    Specific endpoint to test (memorypack-crash, rapp-safety, json-comparison)

.PARAMETER Port
    Port number for the demo application (default: 5000)

.EXAMPLE
    .\Run-SchemaEvolutionDemo.ps1

.EXAMPLE
    .\Run-SchemaEvolutionDemo.ps1 -Endpoint rapp-safety

.EXAMPLE
    .\Run-SchemaEvolutionDemo.ps1 -Port 8080
#>
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("memorypack-crash", "rapp-safety", "json-comparison", "all")]
    [string]$Endpoint = "all",

    [Parameter(Mandatory = $false)]
    [int]$Port = 5000
)

$baseUrl = "http://localhost:$Port"
$projectDir = "src/Rapp.Playground"
$appStarted = $false

function Test-Endpoint {
    param(
        [string]$endpoint,
        [string]$description
    )

    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "Testing: $description" -ForegroundColor Cyan
    Write-Host "Endpoint: $endpoint" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl$endpoint" -Method Get -TimeoutSec 10
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor Green
    }
    catch {
        Write-Host "Error calling $endpoint`: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response.StatusCode -eq 404) {
            Write-Host "Make sure the Rapp Playground application is running on port $Port" -ForegroundColor Yellow
        }
    }
    Write-Host "`n"
}

function Start-DemoApp {
    Write-Host "Starting Rapp Playground application..." -ForegroundColor Yellow

    # Check if dotnet is available
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Error "dotnet CLI is not installed or not in PATH"
        exit 1
    }

    # Check if project directory exists
    if (-not (Test-Path $projectDir)) {
        Write-Error "Project directory '$projectDir' not found. Make sure you're running from the repository root."
        exit 1
    }

    # Start the application in background
    $job = Start-Job -ScriptBlock {
        param($projectDir)
        Set-Location $projectDir
        dotnet run --urls="http://localhost:$using:Port"
    } -ArgumentList $projectDir -Name "RappDemo"

    # Wait for app to start (check if port is listening)
    $maxWait = 30
    $waitCount = 0
    while ($waitCount -lt $maxWait) {
        try {
            $response = Invoke-WebRequest -Uri "$baseUrl/weather" -Method Get -TimeoutSec 2 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Host "Application started successfully on port $Port" -ForegroundColor Green
                return $job
            }
        }
        catch {
            # App not ready yet
        }
        Start-Sleep -Seconds 1
        $waitCount++
        Write-Host "Waiting for application to start... ($waitCount/$maxWait)" -ForegroundColor Gray
    }

    Write-Error "Application failed to start within $maxWait seconds"
    Stop-Job -Name "RappDemo" -ErrorAction SilentlyContinue
    Remove-Job -Name "RappDemo" -ErrorAction SilentlyContinue
    exit 1
}

function Stop-DemoApp {
    param([System.Management.Automation.Job]$job)

    if ($job) {
        Write-Host "Stopping demo application..." -ForegroundColor Yellow
        Stop-Job -Name "RappDemo" -ErrorAction SilentlyContinue
        Remove-Job -Name "RappDemo" -ErrorAction SilentlyContinue
    }
}

# Main execution
try {
    Write-Host "Rapp Schema Evolution Safety Demo" -ForegroundColor Magenta
    Write-Host "==================================" -ForegroundColor Magenta
    Write-Host ""

    # Start the demo application
    $job = Start-DemoApp
    $appStarted = $true

    # Test endpoints based on parameter
    if ($Endpoint -eq "all") {
        Test-Endpoint "/demo/memorypack-crash" "MemoryPack Crash Scenario - Shows how binary serialization fails on schema changes"
        Test-Endpoint "/demo/rapp-safety" "Rapp Safety Demonstration - Shows graceful handling of schema evolution"
        Test-Endpoint "/demo/json-comparison" "System.Text.Json Comparison - Shows JSON's flexible but slow approach"
    }
    else {
        $endpointMap = @{
            "memorypack-crash" = "MemoryPack Crash Scenario"
            "rapp-safety" = "Rapp Safety Demonstration"
            "json-comparison" = "System.Text.Json Comparison"
        }
        Test-Endpoint "/demo/$Endpoint" $endpointMap[$Endpoint]
    }

    Write-Host "Demo completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Key Takeaways:" -ForegroundColor Cyan
    Write-Host "- MemoryPack: Fast but crashes on schema changes" -ForegroundColor White
    Write-Host "- Rapp: Safe binary caching with minimal overhead" -ForegroundColor White
    Write-Host "- JSON: Safe but slow and verbose" -ForegroundColor White

}
catch {
    Write-Error "Demo failed: $($_.Exception.Message)"
}
finally {
    if ($appStarted) {
        Stop-DemoApp -job $job
    }
}