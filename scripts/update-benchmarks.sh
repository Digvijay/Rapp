#!/bin/bash
# Update benchmarks folder with latest BenchmarkDotNet results
# Usage: ./update-benchmarks.sh

set -e

echo "🔄 Updating benchmarks folder with latest results..."

# Check if BenchmarkDotNet.Artifacts/results exists
if [ ! -d "BenchmarkDotNet.Artifacts/results" ]; then
    echo "❌ BenchmarkDotNet.Artifacts/results not found. Run benchmarks first."
    exit 1
fi

# Create benchmarks directory if it doesn't exist
mkdir -p benchmarks

# Copy latest results
cp BenchmarkDotNet.Artifacts/results/* benchmarks/

echo "✅ Benchmarks updated successfully!"
echo "📁 Latest results available in: benchmarks/"
echo "   - Benchmarks-report.html (web view)"
echo "   - Benchmarks-report.csv (data)"
echo "   - Benchmarks-report-github.md (GitHub markdown)"