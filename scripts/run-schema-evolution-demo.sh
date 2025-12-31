#!/bin/bash

# Rapp Schema Evolution Safety Demo Script
# This script demonstrates the critical difference between MemoryPack crashes
# and Rapp's safe handling of schema evolution in .NET 10 applications.

set -e  # Exit on any error

# Configuration
BASE_URL="http://localhost:5000"
PROJECT_DIR="src/Rapp.Playground"
APP_PID=""
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_header() {
    echo -e "${MAGENTA}=======================================${NC}"
    echo -e "${MAGENTA}$1${NC}"
    echo -e "${MAGENTA}=======================================${NC}"
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."

    # Check if dotnet is installed
    if ! command -v dotnet &> /dev/null; then
        log_error "dotnet CLI is not installed or not in PATH"
        log_error "Please install .NET 10.0 SDK from: https://dotnet.microsoft.com/download"
        exit 1
    fi

    # Check if project directory exists
    if [ ! -d "$PROJECT_DIR" ]; then
        log_error "Project directory '$PROJECT_DIR' not found"
        log_error "Make sure you're running this script from the repository root"
        exit 1
    fi

    # Check if jq is available (optional, for pretty JSON output)
    if command -v jq &> /dev/null; then
        HAS_JQ=true
        log_info "jq found - JSON output will be formatted"
    else
        HAS_JQ=false
        log_warning "jq not found - JSON output will be raw"
    fi

    log_success "Prerequisites check passed"
}

# Test if application is responding
wait_for_app() {
    local max_wait=30
    local wait_count=0

    log_info "Waiting for application to start on $BASE_URL..."

    while [ $wait_count -lt $max_wait ]; do
        if curl -s --max-time 2 "$BASE_URL/weather" > /dev/null 2>&1; then
            log_success "Application is responding"
            return 0
        fi

        sleep 1
        wait_count=$((wait_count + 1))
        echo -n "."
    done

    echo "" # New line after dots
    log_error "Application failed to start within $max_wait seconds"
    return 1
}

# Start the demo application
start_demo_app() {
    log_info "Starting Rapp Playground application..."

    cd "$PROJECT_DIR"

    # Start in background
    dotnet run --urls="$BASE_URL" > /dev/null 2>&1 &
    APP_PID=$!

    # Wait for app to be ready
    if ! wait_for_app; then
        cleanup
        exit 1
    fi

    cd "$SCRIPT_DIR" # Go back to script directory
}

# Test a specific endpoint
test_endpoint() {
    local endpoint="$1"
    local description="$2"

    log_header "Testing: $description"
    log_info "Endpoint: $endpoint"

    if [ "$HAS_JQ" = true ]; then
        curl -s "$BASE_URL$endpoint" | jq '.' || {
            log_warning "Failed to parse JSON response, showing raw output:"
            curl -s "$BASE_URL$endpoint"
        }
    else
        curl -s "$BASE_URL$endpoint" || log_error "Failed to call endpoint"
    fi

    echo -e "\n"
}

# Cleanup function
cleanup() {
    if [ -n "$APP_PID" ]; then
        log_info "Stopping demo application (PID: $APP_PID)..."
        kill "$APP_PID" 2>/dev/null || true
        wait "$APP_PID" 2>/dev/null || true
        APP_PID=""
    fi
}

# Main demo function
run_demo() {
    log_header "Rapp Schema Evolution Safety Demo"
    echo ""
    log_info "This demo shows the critical difference between:"
    echo "  • MemoryPack: Fast but crashes on schema changes"
    echo "  • Rapp: Safe binary caching with enterprise reliability"
    echo "  • System.Text.Json: Safe but slow and verbose"
    echo ""

    # Test all endpoints
    test_endpoint "/demo/memorypack-crash" "MemoryPack Crash Scenario"
    test_endpoint "/demo/rapp-safety" "Rapp Safety Demonstration"
    test_endpoint "/demo/json-comparison" "System.Text.Json Comparison"

    log_success "Demo completed successfully!"
    echo ""
    log_header "Key Takeaways"
    echo -e "${CYAN}MemoryPack:${NC} Excellent raw performance but requires strict schema compatibility"
    echo -e "${CYAN}Rapp:${NC} Enterprise-grade safety with minimal overhead (~3%)"
    echo -e "${CYAN}System.Text.Json:${NC} Safe but 4.7x-9.3x slower than Rapp"
    echo ""
    echo -e "${GREEN}Rapp enables safe adoption of high-performance binary caching${NC}"
    echo -e "${GREEN}without the deployment risks that make raw binary serialization dangerous.${NC}"
}

# Script usage
show_usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Rapp Schema Evolution Safety Demo Script

This script starts the Rapp Playground application and demonstrates
the difference between MemoryPack crashes and Rapp's safe handling
of schema evolution scenarios.

OPTIONS:
    -h, --help          Show this help message
    -p, --port PORT     Port number for demo app (default: 5000)
    -e, --endpoint EP   Test specific endpoint only:
                        memorypack-crash, rapp-safety, json-comparison

EXAMPLES:
    $0                    # Run full demo on port 5000
    $0 -p 8080           # Run on port 8080
    $0 -e rapp-safety    # Test only Rapp safety endpoint

ENDPOINTS:
    /demo/memorypack-crash  - Shows MemoryPack crash scenarios
    /demo/rapp-safety       - Shows Rapp's safe handling
    /demo/json-comparison   - Shows System.Text.Json behavior

EOF
}

# Parse command line arguments
ENDPOINT="all"
PORT="5000"

while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_usage
            exit 0
            ;;
        -p|--port)
            PORT="$2"
            BASE_URL="http://localhost:$PORT"
            shift 2
            ;;
        -e|--endpoint)
            ENDPOINT="$2"
            shift 2
            ;;
        *)
            log_error "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

# Validate endpoint
if [ "$ENDPOINT" != "all" ] && [ "$ENDPOINT" != "memorypack-crash" ] && [ "$ENDPOINT" != "rapp-safety" ] && [ "$ENDPOINT" != "json-comparison" ]; then
    log_error "Invalid endpoint: $ENDPOINT"
    log_error "Valid endpoints: memorypack-crash, rapp-safety, json-comparison, all"
    exit 1
fi

# Trap to ensure cleanup on exit
trap cleanup EXIT

# Main execution
main() {
    check_prerequisites
    start_demo_app

    if [ "$ENDPOINT" = "all" ]; then
        run_demo
    else
        test_endpoint "/demo/$ENDPOINT" "Specific Endpoint Test: $ENDPOINT"
        log_success "Endpoint test completed"
    fi
}

main "$@"