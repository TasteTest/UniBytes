#!/usr/bin/env bash
set -euo pipefail

################################################################################
# Backend-only Redeploy Script
# Builds and redeploys only the backend Container App
################################################################################

# Configuration (matches deploy_to_azure.sh defaults)
readonly RG="${RG:-unibytes}"
readonly ACR="${ACR:-unibytesacr}"
readonly STORAGE_ACCOUNT="${STORAGE_ACCOUNT:-unibytesstorage}"
readonly BLOB_CONTAINER="${BLOB_CONTAINER:-uploads}"
readonly APP_BACKEND="${APP_BACKEND:-unibytes-backend}"

# PostgreSQL connection string (REQUIRED - must be provided via CONNECTION_STRING env var)
# Format: "User Id=user;Password=pass;Server=host;Port=5432;Database=db"
readonly CONNECTION_STRING="${CONNECTION_STRING:-}"

# Stripe configuration (REQUIRED for payments)
readonly STRIPE_SECRET_KEY="${STRIPE_SECRET_KEY:-}"
readonly STRIPE_WEBHOOK_SECRET="${STRIPE_WEBHOOK_SECRET:-}"

# Function to extract value from connection string
extract_from_connection_string() {
    local key=$1
    local conn_str=$2
    # Split by semicolon and find the key-value pair
    IFS=';' read -ra PAIRS <<< "$conn_str"
    for pair in "${PAIRS[@]}"; do
        if [[ "$pair" == "${key}="* ]]; then
            echo "${pair#${key}=}"
            return
        fi
    done
}

# Extract PostgreSQL values from connection string (if provided)
if [[ -n "$CONNECTION_STRING" ]]; then
    _extracted_user=$(extract_from_connection_string "User Id" "$CONNECTION_STRING")
    _extracted_password=$(extract_from_connection_string "Password" "$CONNECTION_STRING")
    _extracted_server=$(extract_from_connection_string "Server" "$CONNECTION_STRING")
    _extracted_port=$(extract_from_connection_string "Port" "$CONNECTION_STRING")
    _extracted_db=$(extract_from_connection_string "Database" "$CONNECTION_STRING")
fi

# PostgreSQL configuration (extracted from connection string, can be overridden by env vars)
readonly POSTGRES_USER="${POSTGRES_USER:-${_extracted_user:-}}"
readonly POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-${_extracted_password:-}}"
readonly POSTGRES_HOST="${POSTGRES_HOST:-${_extracted_server:-}}"
readonly POSTGRES_PORT="${POSTGRES_PORT:-${_extracted_port:-5432}}"
readonly POSTGRES_DB="${POSTGRES_DB:-${_extracted_db:-}}"

# Colors
readonly GREEN='\033[0;32m'
readonly BLUE='\033[0;34m'
readonly YELLOW='\033[1;33m'
readonly NC='\033[0m'

log_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

log_success() {
    echo -e "${GREEN}✓${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

log_error() {
    echo -e "\033[0;31m✗\033[0m $1" >&2
}

# Validate PostgreSQL configuration
if [[ -z "$POSTGRES_HOST" ]]; then
    log_error "PostgreSQL configuration is missing"
    log_info "Please provide PostgreSQL connection details in one of these ways:"
    log_info "  1. Set CONNECTION_STRING environment variable:"
    log_info "     export CONNECTION_STRING=\"User Id=user;Password=pass;Server=host;Port=5432;Database=db\""
    log_info "  2. Set individual environment variables:"
    log_info "     POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD"
    exit 1
fi

log_info "PostgreSQL configuration:"
log_info "  Host: $POSTGRES_HOST"
log_info "  Port: $POSTGRES_PORT"
log_info "  Database: $POSTGRES_DB"
log_info "  User: $POSTGRES_USER"

# Get ACR login server
log_info "Getting ACR login server..."
ACR_LOGIN_SERVER=$(az acr show --name "$ACR" --resource-group "$RG" --query loginServer -o tsv)
log_success "ACR login server: $ACR_LOGIN_SERVER"

# Login to ACR
log_info "Logging in to ACR..."
az acr login --name "$ACR" >/dev/null 2>&1
log_success "Logged in to ACR"

# Setup buildx builder
log_info "Setting up Docker buildx..."
if ! docker buildx inspect unibytes-builder >/dev/null 2>&1; then
    docker buildx create --name unibytes-builder --use >/dev/null 2>&1
else
    docker buildx use unibytes-builder >/dev/null 2>&1
fi
log_success "Buildx ready"

# Build and push image
log_info "Building and pushing backend image..."
full_image="$ACR_LOGIN_SERVER/backend:latest"

if docker buildx build \
    --platform linux/amd64 \
    --push \
    --tag "$full_image" \
    "./backend" \
    >/dev/null 2>&1; then
    log_success "Backend image built and pushed"
else
    log_error "Failed to build backend image"
    exit 1
fi

# Get ACR credentials
log_info "Getting ACR credentials..."
acr_user=$(az acr credential show --name "$ACR" --query username -o tsv)
acr_password=$(az acr credential show --name "$ACR" --query "passwords[0].value" -o tsv)

# Update backend container app
log_info "Updating backend Container App..."

# Get current frontend URL first
log_info "Getting frontend URL for CORS configuration..."
FRONTEND_URL=$(az containerapp show --name unibytes-frontend --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "unibytes-frontend.placeholder.azurecontainerapps.io")
log_info "Frontend URL: $FRONTEND_URL"

# Update secrets
log_info "Updating secrets..."
az containerapp secret set \
    --name "$APP_BACKEND" \
    --resource-group "$RG" \
    --secrets \
        "postgres-password=$POSTGRES_PASSWORD" \
        "stripe-secret-key=$STRIPE_SECRET_KEY" \
        "stripe-webhook-secret=$STRIPE_WEBHOOK_SECRET" \
    --output none 2>/dev/null || true

# Update container app
log_info "Updating container app with new image..."
az containerapp update \
    --name "$APP_BACKEND" \
    --resource-group "$RG" \
    --image "$full_image" \
    --set-env-vars \
        "STORAGE_ACCOUNT_NAME=$STORAGE_ACCOUNT" \
        "BLOB_CONTAINER_NAME=$BLOB_CONTAINER" \
        "ASPNETCORE_ENVIRONMENT=Production" \
        "POSTGRES_HOST=$POSTGRES_HOST" \
        "POSTGRES_PORT=$POSTGRES_PORT" \
        "POSTGRES_DB=$POSTGRES_DB" \
        "POSTGRES_USER=$POSTGRES_USER" \
        "POSTGRES_PASSWORD=secretref:postgres-password" \
        "FRONTEND_URL=https://$FRONTEND_URL" \
        "Stripe__SecretKey=secretref:stripe-secret-key" \
        "Stripe__WebhookSecret=secretref:stripe-webhook-secret" \
    --output none 2>/dev/null || {
        log_warning "Update had issues, but may still work"
    }

# Configure managed identity
log_info "Configuring managed identity..."
az containerapp identity assign \
    --name "$APP_BACKEND" \
    --resource-group "$RG" \
    --system-assigned \
    --output none 2>/dev/null || true

# Grant Storage Blob Data Contributor role
log_info "Granting Storage Blob Data Contributor role..."
principal_id=$(az containerapp show --name "$APP_BACKEND" --resource-group "$RG" --query identity.principalId -o tsv 2>/dev/null || echo "")
storage_id=$(az storage account show --name "$STORAGE_ACCOUNT" --resource-group "$RG" --query id -o tsv 2>/dev/null || echo "")

if [[ -n "$principal_id" && -n "$storage_id" ]]; then
    az role assignment create \
        --assignee "$principal_id" \
        --role "Storage Blob Data Contributor" \
        --scope "$storage_id" \
        --output none 2>/dev/null || true
    log_success "Managed identity configured"
else
    log_warning "Could not configure managed identity (may already be configured)"
fi

# Get backend URL
backend_url=$(az containerapp show --name "$APP_BACKEND" --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "")

log_success "Backend redeployed successfully!"
echo ""
if [[ -n "$backend_url" ]]; then
    echo "Backend URL: https://$backend_url"
    echo "API Base URL: https://$backend_url/api"
else
    echo "Backend URL: (not available)"
fi
echo ""

