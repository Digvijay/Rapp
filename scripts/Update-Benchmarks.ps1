# Update benchmarks folder with latest BenchmarkDotNet results
# Usage: .\update-benchmarks.ps1

param()

Write-Host "🔄 Updating benchmarks folder with latest results..." -ForegroundColor Cyan

# Check if BenchmarkDotNet.Artifacts/results exists
if (-not (Test-Path "BenchmarkDotNet.Artifacts/results")) {
    Write-Host "❌ BenchmarkDotNet.Artifacts/results not found. Run benchmarks first." -ForegroundColor Red
    exit 1
}

# Create benchmarks directory if it doesn't exist
if (-not (Test-Path "benchmarks")) {
    New-Item -ItemType Directory -Path "benchmarks" | Out-Null
}

# Copy latest results
Copy-Item "BenchmarkDotNet.Artifacts/results/*" "benchmarks/" -Force

Write-Host "✅ Benchmarks updated successfully!" -ForegroundColor Green
Write-Host "📁 Latest results available in: benchmarks/" -ForegroundColor Yellow
Write-Host "   - Benchmarks-report.html (web view)" -ForegroundColor Yellow
Write-Host "   - Benchmarks-report.csv (data)" -ForegroundColor Yellow
Write-Host "   - Benchmarks-report-github.md (GitHub markdown)" -ForegroundColor Yellow