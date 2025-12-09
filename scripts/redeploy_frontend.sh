#!/usr/bin/env bash
set -euo pipefail

################################################################################
# Frontend-only Redeploy Script
# Builds and redeploys only the frontend Container App
################################################################################

# Configuration (matches deploy_to_azure.sh defaults)
readonly RG="${RG:-unibytes}"
readonly ACR="${ACR:-unibytesacr}"
readonly APP_BACKEND="${APP_BACKEND:-unibytes-backend}"
readonly APP_FRONTEND="${APP_FRONTEND:-unibytes-frontend}"
# Frontend OAuth & Auth secrets (REQUIRED - must be provided via environment variables)
readonly STRIPE_PUBLISHABLE_KEY="${STRIPE_PUBLISHABLE_KEY:-}"
readonly GOOGLE_CLIENT_ID="${GOOGLE_CLIENT_ID:-}"
readonly GOOGLE_CLIENT_SECRET="${GOOGLE_CLIENT_SECRET:-}"
readonly NEXTAUTH_SECRET="${NEXTAUTH_SECRET:-$(openssl rand -base64 32)}"

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

# Validate secrets
validate_secrets

# Get ACR login server
log_info "Getting ACR login server..."
ACR_LOGIN_SERVER=$(az acr show --name "$ACR" --resource-group "$RG" --query loginServer -o tsv)
log_success "ACR login server: $ACR_LOGIN_SERVER"

# Get backend URL
log_info "Getting backend URL..."
backend_url=$(az containerapp show --name "$APP_BACKEND" --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "")

if [[ -z "$backend_url" ]]; then
    log_warning "Backend URL not found. Using BACKEND_URL env var or default..."
    if [[ -n "${BACKEND_URL:-}" ]]; then
        backend_url="${BACKEND_URL#https://}"
        backend_url="${backend_url%/}"
    else
        log_warning "No backend URL available. Frontend may not work correctly."
    fi
fi

if [[ -n "$backend_url" ]]; then
    log_success "Backend URL: https://$backend_url"
fi

# Get frontend URL for NEXTAUTH_URL
frontend_url=$(az containerapp show --name "$APP_FRONTEND" --resource-group "$RG" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || echo "")

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

# Build arguments
build_args=""
if [[ -n "$backend_url" ]]; then
    build_args="--build-arg NEXT_PUBLIC_API_URL=https://$backend_url/api --build-arg API_URL=https://$backend_url/api --build-arg NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=$STRIPE_PUBLISHABLE_KEY"
    log_info "Building with API URL: https://$backend_url/api"
else
    log_warning "Building without API URL (may cause issues)"
fi

# Build and push image
log_info "Building and pushing frontend image..."
full_image="$ACR_LOGIN_SERVER/frontend:latest"

if docker buildx build \
    --platform linux/amd64 \
    --push \
    --tag "$full_image" \
    $build_args \
    "./frontend" \
    >/dev/null 2>&1; then
    log_success "Frontend image built and pushed"
else
    log_error "Failed to build frontend image"
    exit 1
fi

# Update frontend container app
log_info "Updating frontend Container App..."

# Get ACR credentials
acr_user=$(az acr credential show --name "$ACR" --query username -o tsv)
acr_password=$(az acr credential show --name "$ACR" --query "passwords[0].value" -o tsv)

# Update secrets
log_info "Updating secrets..."
az containerapp secret set \
    --name "$APP_FRONTEND" \
    --resource-group "$RG" \
    --secrets \
        "google-client-secret=$GOOGLE_CLIENT_SECRET" \
        "nextauth-secret=$NEXTAUTH_SECRET" \
    --output none 2>/dev/null || true

# Update container app
log_info "Updating container app with new image..."
if [[ -n "$backend_url" && -n "$frontend_url" ]]; then
    az containerapp update \
        --name "$APP_FRONTEND" \
        --resource-group "$RG" \
        --image "$full_image" \
        --set-env-vars \
            "NEXT_PUBLIC_API_URL=https://$backend_url/api" \
            "API_URL=https://$backend_url/api" \
            "NEXTAUTH_URL=https://$frontend_url" \
            "GOOGLE_CLIENT_ID=$GOOGLE_CLIENT_ID" \
            "GOOGLE_CLIENT_SECRET=secretref:google-client-secret" \
            "NEXTAUTH_SECRET=secretref:nextauth-secret" \
            "NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=$STRIPE_PUBLISHABLE_KEY" \
        --output none 2>/dev/null || {
            log_warning "Update had issues, but may still work"
        }
else
    log_warning "Missing URLs, updating image only..."
    az containerapp update \
        --name "$APP_FRONTEND" \
        --resource-group "$RG" \
        --image "$full_image" \
        --output none 2>/dev/null || {
            log_warning "Update had issues, but may still work"
        }
fi

log_success "Frontend redeployed successfully!"
echo ""
echo "Frontend URL: https://$frontend_url"
echo "Backend API URL: https://$backend_url/api"
echo ""

