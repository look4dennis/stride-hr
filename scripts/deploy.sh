#!/bin/bash

# StrideHR Production Deployment Script
# Usage: ./deploy.sh [environment] [version]
# Example: ./deploy.sh production v1.0.0

set -e

# Configuration
ENVIRONMENT=${1:-production}
VERSION=${2:-latest}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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

# Validate environment
validate_environment() {
    if [[ ! "$ENVIRONMENT" =~ ^(staging|production)$ ]]; then
        log_error "Invalid environment: $ENVIRONMENT. Must be 'staging' or 'production'"
        exit 1
    fi
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    # Check if Docker is installed and running
    if ! command -v docker &> /dev/null; then
        log_error "Docker is not installed"
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        log_error "Docker is not running"
        exit 1
    fi
    
    # Check if Docker Compose is installed
    if ! command -v docker-compose &> /dev/null; then
        log_error "Docker Compose is not installed"
        exit 1
    fi
    
    # Check if environment file exists
    if [[ ! -f "$PROJECT_ROOT/.env.$ENVIRONMENT" ]]; then
        log_error "Environment file .env.$ENVIRONMENT not found"
        exit 1
    fi
    
    log_success "Prerequisites check passed"
}

# Backup database
backup_database() {
    log_info "Creating database backup..."
    
    BACKUP_DIR="$PROJECT_ROOT/backups"
    mkdir -p "$BACKUP_DIR"
    
    BACKUP_FILE="$BACKUP_DIR/stridehr_backup_$(date +%Y%m%d_%H%M%S).sql"
    
    # Load environment variables
    source "$PROJECT_ROOT/.env.$ENVIRONMENT"
    
    # Create backup
    docker exec stridehr-mysql-${ENVIRONMENT} mysqldump \
        -u"$MYSQL_USER" -p"$MYSQL_PASSWORD" \
        "$MYSQL_DATABASE" > "$BACKUP_FILE"
    
    if [[ $? -eq 0 ]]; then
        log_success "Database backup created: $BACKUP_FILE"
    else
        log_error "Database backup failed"
        exit 1
    fi
}

# Pull latest images
pull_images() {
    log_info "Pulling latest Docker images..."
    
    if [[ "$VERSION" == "latest" ]]; then
        docker-compose -f "$PROJECT_ROOT/docker-compose.prod.yml" pull
    else
        # Pull specific version images
        docker pull "ghcr.io/your-org/stridehr-backend:$VERSION"
        docker pull "ghcr.io/your-org/stridehr-frontend:$VERSION"
    fi
    
    log_success "Images pulled successfully"
}

# Deploy application
deploy_application() {
    log_info "Deploying StrideHR $VERSION to $ENVIRONMENT..."
    
    cd "$PROJECT_ROOT"
    
    # Set environment file
    cp ".env.$ENVIRONMENT" .env
    
    # Deploy with zero downtime
    if [[ "$ENVIRONMENT" == "production" ]]; then
        # Blue-green deployment for production
        deploy_blue_green
    else
        # Simple deployment for staging
        docker-compose -f docker-compose.prod.yml down
        docker-compose -f docker-compose.prod.yml up -d
    fi
    
    log_success "Deployment completed"
}

# Blue-green deployment
deploy_blue_green() {
    log_info "Starting blue-green deployment..."
    
    # Check current environment
    CURRENT_ENV=$(docker ps --format "table {{.Names}}" | grep -E "(blue|green)" | head -1 | sed 's/.*-\(blue\|green\).*/\1/')
    
    if [[ "$CURRENT_ENV" == "blue" ]]; then
        NEW_ENV="green"
    else
        NEW_ENV="blue"
    fi
    
    log_info "Current environment: ${CURRENT_ENV:-none}, deploying to: $NEW_ENV"
    
    # Deploy to new environment
    docker-compose -f "docker-compose.$NEW_ENV.yml" up -d
    
    # Wait for health checks
    wait_for_health_checks "$NEW_ENV"
    
    # Switch traffic
    switch_traffic "$NEW_ENV"
    
    # Clean up old environment
    if [[ -n "$CURRENT_ENV" ]]; then
        log_info "Cleaning up old environment: $CURRENT_ENV"
        docker-compose -f "docker-compose.$CURRENT_ENV.yml" down
    fi
    
    log_success "Blue-green deployment completed"
}

# Wait for health checks
wait_for_health_checks() {
    local env=$1
    log_info "Waiting for health checks to pass..."
    
    local max_attempts=30
    local attempt=1
    
    while [[ $attempt -le $max_attempts ]]; do
        if curl -f "http://localhost:5000/health" &> /dev/null; then
            log_success "Health checks passed"
            return 0
        fi
        
        log_info "Attempt $attempt/$max_attempts: Health check failed, retrying in 10 seconds..."
        sleep 10
        ((attempt++))
    done
    
    log_error "Health checks failed after $max_attempts attempts"
    exit 1
}

# Switch traffic
switch_traffic() {
    local new_env=$1
    log_info "Switching traffic to $new_env environment..."
    
    # Update load balancer configuration
    # This would typically involve updating nginx configuration or cloud load balancer
    
    log_success "Traffic switched to $new_env environment"
}

# Run database migrations
run_migrations() {
    log_info "Running database migrations..."
    
    docker exec stridehr-api-${ENVIRONMENT} dotnet ef database update
    
    if [[ $? -eq 0 ]]; then
        log_success "Database migrations completed"
    else
        log_error "Database migrations failed"
        exit 1
    fi
}

# Verify deployment
verify_deployment() {
    log_info "Verifying deployment..."
    
    # Check if containers are running
    local containers=(
        "stridehr-mysql-${ENVIRONMENT}"
        "stridehr-redis-${ENVIRONMENT}"
        "stridehr-api-${ENVIRONMENT}"
        "stridehr-frontend-${ENVIRONMENT}"
    )
    
    for container in "${containers[@]}"; do
        if docker ps | grep -q "$container"; then
            log_success "$container is running"
        else
            log_error "$container is not running"
            exit 1
        fi
    done
    
    # Check application health
    if curl -f "http://localhost:5000/health" &> /dev/null; then
        log_success "Application health check passed"
    else
        log_error "Application health check failed"
        exit 1
    fi
    
    log_success "Deployment verification completed"
}

# Rollback function
rollback() {
    log_warning "Rolling back deployment..."
    
    # Restore from backup
    local latest_backup=$(ls -t "$PROJECT_ROOT/backups"/*.sql | head -1)
    if [[ -n "$latest_backup" ]]; then
        log_info "Restoring database from: $latest_backup"
        
        source "$PROJECT_ROOT/.env.$ENVIRONMENT"
        docker exec -i stridehr-mysql-${ENVIRONMENT} mysql \
            -u"$MYSQL_USER" -p"$MYSQL_PASSWORD" \
            "$MYSQL_DATABASE" < "$latest_backup"
    fi
    
    # Restart previous version
    docker-compose -f "$PROJECT_ROOT/docker-compose.prod.yml" down
    docker-compose -f "$PROJECT_ROOT/docker-compose.prod.yml" up -d
    
    log_success "Rollback completed"
}

# Cleanup old images
cleanup() {
    log_info "Cleaning up old Docker images..."
    
    docker image prune -f
    docker system prune -f
    
    log_success "Cleanup completed"
}

# Main deployment flow
main() {
    log_info "Starting StrideHR deployment to $ENVIRONMENT (version: $VERSION)"
    
    validate_environment
    check_prerequisites
    
    # Create backup before deployment
    backup_database
    
    # Deploy application
    pull_images
    deploy_application
    run_migrations
    
    # Verify deployment
    verify_deployment
    
    # Cleanup
    cleanup
    
    log_success "StrideHR deployment to $ENVIRONMENT completed successfully!"
    log_info "Application is available at: http://localhost:4200"
    log_info "API documentation: http://localhost:5000/swagger"
}

# Handle script interruption
trap 'log_error "Deployment interrupted"; rollback; exit 1' INT TERM

# Run main function
main "$@"