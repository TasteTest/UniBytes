#!/usr/bin/env bash
set -euo pipefail

################################################################################
# Azure Container Apps Deployment Script
# Optimized for Azure for Students subscriptions
# 
# Features:
# - Multi-platform Docker builds (linux/amd64)
# - Managed Identity for secure access to Azure Storage
# - Scale-to-zero for cost optimization
# - External PostgreSQL database (Supabase, Neon, etc.)
#
# Usage: ./deploy_to_azure.sh [options]
# 
# Required Environment Variables:
#   POSTGRES_HOST     - PostgreSQL server hostname (e.g., aws-1-eu-west-1.pooler.supabase.com)
#   POSTGRES_PORT     - PostgreSQL port (default: 5432)
#   POSTGRES_DB       - Database name (default: postgres)
#   POSTGRES_USER     - Database user
#   POSTGRES_PASSWORD - Database password
#
# Optional Environment Variables:
#   RG                - Resource group name (default: unibytes)
#   LOC               - Azure region (default: germanywestcentral)
#   ACR               - Container registry name (default: unibytesacr)
#   STORAGE_ACCOUNT   - Storage account name (default: unibytesstorage)
#   ENV_NAME          - Container Apps environment (default: unibytes-env)
################################################################################

# Configuration defaults
readonly RG="${RG:-unibytes}"
readonly LOC="${LOC:-germanywestcentral}"
readonly ACR="${ACR:-unibytesacr}"
readonly STORAGE_ACCOUNT="${STORAGE_ACCOUNT:-unibytesstorage}"
readonly ENV_NAME="${ENV_NAME:-unibytes-env}"
readonly BLOB_CONTAINER="${BLOB_CONTAINER:-uploads}"
readonly APP_BACKEND="${APP_BACKEND:-unibytes-backend}"
readonly APP_FRONTEND="${APP_FRONTEND:-unibytes-frontend}"

# PostgreSQL connection string (REQUIRED - must be provided via CONNECTION_STRING env var)
# Format: "User Id=user;Password=pass;Server=host;Port=5432;Database=db"
readonly CONNECTION_STRING="${CONNECTION_STRING:-}"

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

# Frontend OAuth & Auth secrets (REQUIRED - must be provided via environment variables)
readonly GOOGLE_CLIENT_ID="${GOOGLE_CLIENT_ID:-}"
readonly GOOGLE_CLIENT_SECRET="${GOOGLE_CLIENT_SECRET:-}"
readonly NEXTAUTH_SECRET="${NEXTAUTH_SECRET:-$(openssl rand -base64 32)}"
readonly STRIPE_PUBLISHABLE_KEY="${STRIPE_PUBLISHABLE_KEY:-}"

# OpenRouter API Key for AI recommendations
readonly OPENROUTER_API_KEY="${OPENROUTER_API_KEY:-}"

# Resource sizing (Azure Container Apps requirements)
readonly BACKEND_CPU="0.25"
readonly BACKEND_MEM="0.5Gi"
readonly FRONTEND_CPU="0.25"
readonly FRONTEND_MEM="0.5Gi"
readonly MIN_REPLICAS="0"
readonly MAX_REPLICAS="1"

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m' # No Color

################################################################################
# Helper functions
################################################################################

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
    echo -e "${RED}✗${NC} $1" >&2
}

check_command() {
    if ! command -v "$1" >/dev/null 2>&1; then
        log_error "$1 is not installed. Please install it first."
        return 1
    fi
    return 0
}

register_provider() {
    local provider=$1
    local state
    state=$(az provider show --namespace "$provider" --query registrationState -o tsv 2>/dev/null || echo "NotRegistered")
    
    if [[ "$state" != "Registered" ]]; then
        log_info "Registering provider: $provider"
        if az provider register --namespace "$provider" --wait >/dev/null 2>&1; then
            log_success "Registered $provider"
        else
            log_warning "Could not register $provider (may require subscription permissions)"
        fi
    fi
}

################################################################################
# Pre-flight checks
################################################################################

preflight_checks() {
    log_info "Running pre-flight checks..."
    
    # Check Azure CLI
    if ! check_command az; then
        log_error "Install from: https://aka.ms/azcli"
        exit 1
    fi
    
    # Check Docker (needed for local builds)
    if ! check_command docker; then
        log_error "Docker is required for building images"
        log_error "Install from: https://docs.docker.com/get-docker/"
        exit 1
    fi
    
    # Verify Azure login
    if ! az account show >/dev/null 2>&1; then
        log_error "Not logged in to Azure. Run: az login"
        exit 1
    fi
    
    # Install/upgrade Container Apps extension
    log_info "Ensuring Container Apps extension is installed..."
    az extension add --name containerapp --upgrade --allow-preview true 2>/dev/null || true
    
    # Register required providers
    log_info "Registering required resource providers..."
    for provider in "Microsoft.App" "Microsoft.Storage" "Microsoft.ContainerRegistry" "Microsoft.OperationalInsights"; do
        register_provider "$provider"
    done
    
    local subscription
    subscription=$(az account show --query name -o tsv)
    log_success "Using subscription: $subscription"
}

################################################################################
# Resource group setup
################################################################################

setup_resource_group() {
    log_info "Setting up resource group: $RG"
    
    if az group show --name "$RG" >/dev/null 2>&1; then
        local existing_loc
        existing_loc=$(az group show --name "$RG" --query location -o tsv)
        log_success "Resource group exists in $existing_loc"
    else
        log_info "Creating resource group in $LOC"
        az group create --name "$RG" --location "$LOC" --output none
        log_success "Resource group created"
    fi
}

################################################################################
# Container Registry setup
################################################################################

setup_container_registry() {
    log_info "Setting up Azure Container Registry: $ACR"
    
    if az acr show --name "$ACR" --resource-group "$RG" >/dev/null 2>&1; then
        log_success "ACR already exists"
    else
        log_info "Creating ACR (this may take a few minutes)..."
        if az acr create \
            --resource-group "$RG" \
            --name "$ACR" \
            --sku Standard \
            --location "$LOC" \
            --admin-enabled true \
            --output none 2>/dev/null; then
            log_success "ACR created successfully"
        else
            log_error "ACR creation failed (may be blocked by Azure for Students policy)"
            log_error "Consider using GitHub Container Registry (GHCR) instead"
            exit 1
        fi
    fi
    
    # Get ACR login server
    ACR_LOGIN_SERVER=$(az acr show --name "$ACR" --query loginServer -o tsv)
    log_success "ACR login server: $ACR_LOGIN_SERVER"
}

################################################################################
# Storage account setup
################################################################################

setup_storage() {
    log_info "Setting up Azure Storage: $STORAGE_ACCOUNT"
    
    if az storage account show --name "$STORAGE_ACCOUNT" --resource-group "$RG" >/dev/null 2>&1; then
        log_success "Storage account already exists"
    else
        log_info "Creating storage account..."
        if az storage account create \
            --name "$STORAGE_ACCOUNT" \
            --resource-group "$RG" \
            --location "$LOC" \
            --sku Standard_LRS \
            --kind StorageV2 \
            --output none 2>/dev/null; then
            log_success "Storage account created"
        else
            log_warning "Storage creation failed (checking for existing account)"
            STORAGE_ACCOUNT=$(az storage account list --resource-group "$RG" --query "[0].name" -o tsv 2>/dev/null || echo "")
            if [[ -z "$STORAGE_ACCOUNT" ]]; then
                log_error "No storage account available"
                exit 1
            fi
            log_success "Using existing storage: $STORAGE_ACCOUNT"
        fi
    fi
    
    # Create blob container
    log_info "Creating blob container: $BLOB_CONTAINER"
    local conn_string
    conn_string=$(az storage account show-connection-string --name "$STORAGE_ACCOUNT" --resource-group "$RG" -o tsv)
    az storage container create \
        --name "$BLOB_CONTAINER" \
        --account-name "$STORAGE_ACCOUNT" \
        --connection-string "$conn_string" \
        --output none 2>/dev/null || true
    log_success "Blob container ready"
}

################################################################################
# PostgreSQL Database setup
################################################################################

setup_postgresql() {
    log_info "Validating PostgreSQL configuration..."
    
    # PostgreSQL values are extracted from CONNECTION_STRING or can be set via individual env vars
    if [[ -z "${POSTGRES_HOST:-}" ]]; then
        log_error "PostgreSQL configuration is missing"
        log_info "Please provide PostgreSQL connection details in one of these ways:"
        log_info "  1. Set CONNECTION_STRING environment variable:"
        log_info "     export CONNECTION_STRING=\"User Id=user;Password=pass;Server=host;Port=5432;Database=db\""
        log_info "  2. Set individual environment variables:"
        log_info "     POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD"
        log_info "Example: export POSTGRES_HOST=your-db.supabase.com"
        exit 1
    fi
    
    log_success "Using external PostgreSQL: $POSTGRES_HOST"
    log_info "Database: $POSTGRES_DB"
    log_info "User: $POSTGRES_USER"
    log_info "Port: $POSTGRES_PORT"
}

validate_secrets() {
    log_info "Validating required secrets..."
    
    local missing_secrets=()
    
    if [[ -z "$GOOGLE_CLIENT_ID" ]]; then
        missing_secrets+=("GOOGLE_CLIENT_ID")
    fi
    
    if [[ -z "$GOOGLE_CLIENT_SECRET" ]]; then
        missing_secrets+=("GOOGLE_CLIENT_SECRET")
    fi
    
    if [[ -z "$STRIPE_PUBLISHABLE_KEY" ]]; then
        missing_secrets+=("STRIPE_PUBLISHABLE_KEY")
    fi
    
    if [[ ${#missing_secrets[@]} -gt 0 ]]; then
        log_error "Missing required secrets:"
        for secret in "${missing_secrets[@]}"; do
            log_error "  - $secret"
        done
        log_info "Please set these environment variables before running the script"
        exit 1
    fi
    
    log_success "All required secrets are configured"
}

################################################################################
# Build and push Docker images
################################################################################

build_and_push_image() {
    local service=$1
    local dockerfile_path=$2
    local image_tag="${service}:latest"
    local full_image="$ACR_LOGIN_SERVER/$image_tag"
    
    log_info "Building $service image..."
    
    # Login to ACR
    az acr login --name "$ACR" >/dev/null 2>&1
    
    # Setup buildx builder for multi-platform support
    if ! docker buildx inspect unibytes-builder >/dev/null 2>&1; then
        docker buildx create --name unibytes-builder --use >/dev/null 2>&1
    else
        docker buildx use unibytes-builder >/dev/null 2>&1
    fi
    
    # Build arguments (for frontend)
    local build_args=""
    if [[ "$service" == "frontend" ]]; then
        # Get backend URL for frontend build
        local backend_url
        backend_url=$(az containerapp show --name "$APP_BACKEND" --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "")
        
        # Fallback to environment variable if backend doesn't exist yet (first deployment)
        if [[ -z "$backend_url" && -n "${BACKEND_URL:-}" ]]; then
            backend_url="${BACKEND_URL#https://}"  # Remove https:// if present
            backend_url="${backend_url%/}"  # Remove trailing slash
            log_info "Using BACKEND_URL environment variable: $backend_url"
        fi
        
        if [[ -n "$backend_url" ]]; then
            build_args="--build-arg NEXT_PUBLIC_API_URL=https://$backend_url/api --build-arg API_URL=https://$backend_url/api --build-arg NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=$STRIPE_PUBLISHABLE_KEY"
            log_info "Frontend will use backend URL: https://$backend_url/api"
        else
            log_warning "Backend URL not available. Frontend will be built without API URL."
            log_warning "You can set BACKEND_URL environment variable or rebuild frontend after backend is deployed."
        fi
    fi
    
    # Build and push with platform specification
    if docker buildx build \
        --platform linux/amd64 \
        --push \
        --tag "$full_image" \
        $build_args \
        "$dockerfile_path" \
        >/dev/null 2>&1; then
        log_success "$service image built and pushed"
    else
        log_error "Failed to build $service image"
        exit 1
    fi
}

build_images() {
    log_info "Building and pushing Docker images..."
    
    if [[ -d "./backend" ]]; then
        build_and_push_image "backend" "./backend"
    else
        log_warning "Backend directory not found, skipping"
    fi
    
    if [[ -d "./frontend" ]]; then
        build_and_push_image "frontend" "./frontend"
    else
        log_warning "Frontend directory not found, skipping"
    fi
}

################################################################################
# Container Apps Environment setup
################################################################################

setup_container_apps_environment() {
    log_info "Setting up Container Apps environment: $ENV_NAME"
    
    if az containerapp env show --name "$ENV_NAME" --resource-group "$RG" >/dev/null 2>&1; then
        log_success "Container Apps environment already exists"
    else
        log_info "Creating Container Apps environment (this may take several minutes)..."
        if az containerapp env create \
            --name "$ENV_NAME" \
            --resource-group "$RG" \
            --location "$LOC" \
            --output none 2>/dev/null; then
            log_success "Container Apps environment created"
        else
            log_error "Failed to create Container Apps environment"
            exit 1
        fi
    fi
}

################################################################################
# Deploy Container Apps
################################################################################

deploy_backend() {
    log_info "Deploying backend Container App..."
    
    local backend_image="$ACR_LOGIN_SERVER/backend:latest"
    local acr_user acr_password
    acr_user=$(az acr credential show --name "$ACR" --query username -o tsv)
    acr_password=$(az acr credential show --name "$ACR" --query "passwords[0].value" -o tsv)
    
    # Create or update backend app
    if az containerapp show --name "$APP_BACKEND" --resource-group "$RG" >/dev/null 2>&1; then
        log_info "Updating existing backend app with new configuration..."
        
        # Update secrets first
        az containerapp secret set \
            --name "$APP_BACKEND" \
            --resource-group "$RG" \
            --secrets \
                "postgres-password=$POSTGRES_PASSWORD" \
                "openrouter-api-key=$OPENROUTER_API_KEY" \
            --output none 2>/dev/null || true
        
        # Update the app with new image and env vars
        az containerapp update \
            --name "$APP_BACKEND" \
            --resource-group "$RG" \
            --image "$backend_image" \
            --set-env-vars \
                "STORAGE_ACCOUNT_NAME=$STORAGE_ACCOUNT" \
                "BLOB_CONTAINER_NAME=$BLOB_CONTAINER" \
                "ASPNETCORE_ENVIRONMENT=Production" \
                "POSTGRES_HOST=$POSTGRES_HOST" \
                "POSTGRES_PORT=5432" \
                "POSTGRES_DB=$POSTGRES_DB" \
                "POSTGRES_USER=$POSTGRES_USER" \
                "POSTGRES_PASSWORD=secretref:postgres-password" \
                "OPENROUTER_API_KEY=secretref:openrouter-api-key" \
            --output none 2>/dev/null || {
                log_warning "Backend update had issues, but may still work"
            }
    else
        log_info "Creating backend app..."
        az containerapp create \
            --name "$APP_BACKEND" \
            --resource-group "$RG" \
            --environment "$ENV_NAME" \
            --image "$backend_image" \
            --cpu "$BACKEND_CPU" \
            --memory "$BACKEND_MEM" \
            --min-replicas "$MIN_REPLICAS" \
            --max-replicas "$MAX_REPLICAS" \
            --ingress external \
            --target-port 8080 \
            --registry-server "$ACR_LOGIN_SERVER" \
            --registry-username "$acr_user" \
            --registry-password "$acr_password" \
            --secrets \
                "postgres-password=$POSTGRES_PASSWORD" \
                "openrouter-api-key=$OPENROUTER_API_KEY" \
            --env-vars \
                "STORAGE_ACCOUNT_NAME=$STORAGE_ACCOUNT" \
                "BLOB_CONTAINER_NAME=$BLOB_CONTAINER" \
                "ASPNETCORE_ENVIRONMENT=Production" \
                "POSTGRES_HOST=$POSTGRES_HOST" \
                "POSTGRES_PORT=5432" \
                "POSTGRES_DB=$POSTGRES_DB" \
                "POSTGRES_USER=$POSTGRES_USER" \
                "POSTGRES_PASSWORD=secretref:postgres-password" \
                "OPENROUTER_API_KEY=secretref:openrouter-api-key" \
            --output none 2>/dev/null || {
                log_error "Backend deployment failed"
                return 1
            }
    fi
    
    # Enable managed identity
    log_info "Configuring managed identity..."
    az containerapp identity assign \
        --name "$APP_BACKEND" \
        --resource-group "$RG" \
        --system-assigned \
        --output none 2>/dev/null || true
    
    # Grant Storage Blob Data Contributor role
    local principal_id storage_id
    principal_id=$(az containerapp show --name "$APP_BACKEND" --resource-group "$RG" --query identity.principalId -o tsv 2>/dev/null || echo "")
    storage_id=$(az storage account show --name "$STORAGE_ACCOUNT" --resource-group "$RG" --query id -o tsv 2>/dev/null || echo "")
    
    if [[ -n "$principal_id" && -n "$storage_id" ]]; then
        log_info "Granting Storage Blob Data Contributor role..."
        az role assignment create \
            --assignee "$principal_id" \
            --role "Storage Blob Data Contributor" \
            --scope "$storage_id" \
            --output none 2>/dev/null || true
        log_success "Managed identity configured"
    fi
    
    log_success "Backend deployed successfully"
}

deploy_frontend() {
    log_info "Deploying frontend Container App..."
    
    local frontend_image="$ACR_LOGIN_SERVER/frontend:latest"
    local acr_user acr_password backend_url
    acr_user=$(az acr credential show --name "$ACR" --query username -o tsv)
    acr_password=$(az acr credential show --name "$ACR" --query "passwords[0].value" -o tsv)
    backend_url=$(az containerapp show --name "$APP_BACKEND" --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "")
    
    if [[ -z "$backend_url" ]]; then
        log_warning "Could not retrieve backend URL, frontend will need manual configuration"
        backend_url="localhost:8080"
    fi
    
    # Get the frontend URL for NEXTAUTH_URL (needed before we can set it)
    local frontend_url
    frontend_url=$(az containerapp show --name "$APP_FRONTEND" --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "")
    
    # Create or update frontend app
    if az containerapp show --name "$APP_FRONTEND" --resource-group "$RG" >/dev/null 2>&1; then
        log_info "Updating existing frontend app with backend URL..."
        
        # Update secrets first
        az containerapp secret set \
            --name "$APP_FRONTEND" \
            --resource-group "$RG" \
            --secrets \
                "google-client-secret=$GOOGLE_CLIENT_SECRET" \
                "nextauth-secret=$NEXTAUTH_SECRET" \
            --output none 2>/dev/null || true
        
        az containerapp update \
            --name "$APP_FRONTEND" \
            --resource-group "$RG" \
            --image "$frontend_image" \
            --set-env-vars \
                "NEXT_PUBLIC_API_URL=https://$backend_url/api" \
                "API_URL=https://$backend_url/api" \
                "NEXTAUTH_URL=https://$frontend_url" \
                "GOOGLE_CLIENT_ID=$GOOGLE_CLIENT_ID" \
                "GOOGLE_CLIENT_SECRET=secretref:google-client-secret" \
                "NEXTAUTH_SECRET=secretref:nextauth-secret" \
                "NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=$STRIPE_PUBLISHABLE_KEY" \
            --output none 2>/dev/null || {
                log_warning "Frontend update had issues, but may still work"
            }
    else
        log_info "Creating frontend app with backend URL: https://$backend_url"
        az containerapp create \
            --name "$APP_FRONTEND" \
            --resource-group "$RG" \
            --environment "$ENV_NAME" \
            --image "$frontend_image" \
            --cpu "$FRONTEND_CPU" \
            --memory "$FRONTEND_MEM" \
            --min-replicas "$MIN_REPLICAS" \
            --max-replicas "$MAX_REPLICAS" \
            --ingress external \
            --target-port 3000 \
            --registry-server "$ACR_LOGIN_SERVER" \
            --registry-username "$acr_user" \
            --registry-password "$acr_password" \
            --secrets \
                "google-client-secret=$GOOGLE_CLIENT_SECRET" \
                "nextauth-secret=$NEXTAUTH_SECRET" \
            --env-vars \
                "NEXT_PUBLIC_API_URL=https://$backend_url/api" \
                "API_URL=https://$backend_url/api" \
                "GOOGLE_CLIENT_ID=$GOOGLE_CLIENT_ID" \
                "GOOGLE_CLIENT_SECRET=secretref:google-client-secret" \
                "NEXTAUTH_SECRET=secretref:nextauth-secret" \
                "NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=$STRIPE_PUBLISHABLE_KEY" \
            --output none 2>/dev/null || {
                log_error "Frontend deployment failed"
                return 1
            }
        
        # Get the frontend URL after creation and update NEXTAUTH_URL
        frontend_url=$(az containerapp show --name "$APP_FRONTEND" --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "")
        if [[ -n "$frontend_url" ]]; then
            log_info "Updating NEXTAUTH_URL to https://$frontend_url"
            az containerapp update \
                --name "$APP_FRONTEND" \
                --resource-group "$RG" \
                --set-env-vars "NEXTAUTH_URL=https://$frontend_url" \
                --output none 2>/dev/null || true
        fi
    fi
    
    log_success "Frontend deployed successfully"
}

################################################################################
# Display summary
################################################################################

display_summary() {
    local backend_url frontend_url
    backend_url=$(az containerapp show --name "$APP_BACKEND" --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "N/A")
    frontend_url=$(az containerapp show --name "$APP_FRONTEND" --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "N/A")
    
    echo ""
    echo "═══════════════════════════════════════════════════════════════"
    echo "  Deployment Summary"
    echo "═══════════════════════════════════════════════════════════════"
    echo ""
    echo "  Resource Group:    $RG"
    echo "  Location:          $LOC"
    echo "  Environment:       $ENV_NAME"
    echo ""
    echo "  Container Registry:"
    echo "    Name:            $ACR"
    echo "    Login Server:    $ACR_LOGIN_SERVER"
    echo ""
    echo "  Storage Account:"
    echo "    Name:            $STORAGE_ACCOUNT"
    echo "    Container:       $BLOB_CONTAINER"
    echo ""
    echo "  PostgreSQL Database:"
    echo "    Host:            $POSTGRES_HOST"
    echo "    Port:            $POSTGRES_PORT"
    echo "    Database:        $POSTGRES_DB"
    echo "    User:            $POSTGRES_USER"
    echo "    Password:        [REDACTED]"
    echo ""
    echo "  Backend App:"
    echo "    Name:            $APP_BACKEND"
    echo "    URL:             https://$backend_url"
    echo "    CPU/Memory:      $BACKEND_CPU / $BACKEND_MEM"
    echo "    Scale:           $MIN_REPLICAS-$MAX_REPLICAS replicas"
    echo ""
    echo "  Frontend App:"
    echo "    Name:            $APP_FRONTEND"
    echo "    URL:             https://$frontend_url"
    echo "    CPU/Memory:      $FRONTEND_CPU / $FRONTEND_MEM"
    echo "    Scale:           $MIN_REPLICAS-$MAX_REPLICAS replicas"
    echo ""
    echo "═══════════════════════════════════════════════════════════════"
    echo ""
    log_success "Deployment completed successfully!"
    echo ""
    echo "Database Connection String (save securely):"
    echo "  Host=${POSTGRES_HOST};Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
    echo ""
    echo "Next steps:"
    echo "  • Visit your frontend: https://$frontend_url"
    echo "  • Monitor logs: az containerapp logs show --name $APP_BACKEND -g $RG --follow"
    echo "  • Connect to DB: psql -h $POSTGRES_HOST -U $POSTGRES_USER -d $POSTGRES_DB"
    echo "  • View metrics in Azure Portal"
    echo ""
}

################################################################################
# Main execution
################################################################################

main() {
    echo ""
    echo "╔═══════════════════════════════════════════════════════════════╗"
    echo "║  Azure Container Apps Deployment                             ║"
    echo "║  Optimized for Azure for Students                            ║"
    echo "╚═══════════════════════════════════════════════════════════════╝"
    echo ""
    
    preflight_checks
    setup_resource_group
    setup_container_registry
    setup_storage
    setup_postgresql
    validate_secrets
    build_images
    setup_container_apps_environment
    deploy_backend
    deploy_frontend
    display_summary
}

# Run main function
main "$@"
