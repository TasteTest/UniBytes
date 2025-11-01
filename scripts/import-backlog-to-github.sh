#!/bin/bash

# CampusEats - GitHub Project Backlog Automation Script
# This script creates items directly in GitHub Project from markdown backlog files
# Requires: gh CLI installed and authenticated

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
ORG="TasteTest"
PROJECT_NUMBER="1"
BACKLOGS_DIR="./backlogs"

# Find gh CLI path
GH_CMD="/opt/homebrew/bin/gh"
if [ ! -f "$GH_CMD" ]; then
    GH_CMD="/usr/local/bin/gh"
fi

# Check if gh CLI is installed
if [ ! -f "$GH_CMD" ]; then
    echo -e "${RED}Error: GitHub CLI (gh) is not installed${NC}"
    echo "Install it from: https://cli.github.com/"
    exit 1
fi

# Check if authenticated
if ! $GH_CMD auth status &> /dev/null; then
    echo -e "${RED}Error: Not authenticated with GitHub CLI${NC}"
    echo "Run: gh auth login"
    exit 1
fi

echo -e "${GREEN}Starting GitHub Project Backlog Import${NC}"
echo "Organization: $ORG"
echo "Project: $PROJECT_NUMBER"
echo ""

# Function to extract and create project items from a backlog file
process_backlog() {
    local file=$1
    local service_name=$2
    local service_label=$3
    
    echo -e "${GREEN}Processing: $file${NC}"
    echo "Service: $service_name"
    
    # Read the file and extract user stories
    local current_epic=""
    local current_story=""
    local story_content=""
    local acceptance_criteria=""
    local tasks=""
    local priority=""
    local story_points=""
    local in_story=false
    local in_acceptance=false
    local in_tasks=false
    
    while IFS= read -r line; do
        # Detect Epic
        if [[ $line =~ ^##[[:space:]]Epic ]]; then
            current_epic=$(echo "$line" | sed 's/^##[[:space:]]Epic[[:space:]][0-9]*:[[:space:]]*//')
            echo "  Epic: $current_epic"
            continue
        fi
        
        # Detect User Story
        if [[ $line =~ ^###[[:space:]]User[[:space:]]Story ]]; then
            # Create previous story if exists
            if [ "$in_story" = true ] && [ -n "$current_story" ]; then
                create_github_issue "$service_label" "$current_epic" "$current_story" "$story_content" "$acceptance_criteria" "$tasks" "$priority" "$story_points"
            fi
            
            # Reset for new story
            current_story=$(echo "$line" | sed 's/^###[[:space:]]User[[:space:]]Story[[:space:]][0-9.]*:[[:space:]]*//')
            story_content=""
            acceptance_criteria=""
            tasks=""
            priority=""
            story_points=""
            in_story=true
            in_acceptance=false
            in_tasks=false
            continue
        fi
        
        # Detect Technical Task (no user story format)
        if [[ $line =~ ^###[[:space:]]Task: ]]; then
            # Create previous story if exists
            if [ "$in_story" = true ] && [ -n "$current_story" ]; then
                create_github_issue "$service_label" "$current_epic" "$current_story" "$story_content" "$acceptance_criteria" "$tasks" "$priority" "$story_points"
            fi
            
            # Reset for new task
            current_story=$(echo "$line" | sed 's/^###[[:space:]]Task:[[:space:]]*//')
            story_content=""
            acceptance_criteria=""
            tasks=""
            priority="P0"
            story_points="3"
            in_story=true
            in_acceptance=false
            in_tasks=false
            continue
        fi
        
        # Extract content
        if [ "$in_story" = true ]; then
            # Story description (As a... I want to... So that...)
            if [[ $line =~ ^\*\*As[[:space:]]a\*\* ]]; then
                story_content+="$line"$'\n'
            elif [[ $line =~ ^\*\*I[[:space:]]want[[:space:]]to\*\* ]]; then
                story_content+="$line"$'\n'
            elif [[ $line =~ ^\*\*So[[:space:]]that\*\* ]]; then
                story_content+="$line"$'\n'
            fi
            
            # Acceptance Criteria
            if [[ $line =~ ^\*\*Acceptance[[:space:]]Criteria:\*\* ]]; then
                in_acceptance=true
                in_tasks=false
                continue
            fi
            
            if [ "$in_acceptance" = true ]; then
                if [[ $line =~ ^-[[:space:]] ]]; then
                    acceptance_criteria+="$line"$'\n'
                elif [[ $line =~ ^\*\*Tasks:\*\* ]]; then
                    in_acceptance=false
                    in_tasks=true
                    continue
                fi
            fi
            
            # Tasks
            if [ "$in_tasks" = true ]; then
                if [[ $line =~ ^-[[:space:]]\[\ \][[:space:]] ]]; then
                    tasks+="$line"$'\n'
                elif [[ $line =~ ^\*\*Priority:\*\* ]]; then
                    priority=$(echo "$line" | sed 's/^\*\*Priority:\*\*[[:space:]]*//' | sed 's/[[:space:]]*(.*)//')
                    in_tasks=false
                    continue
                fi
            fi
            
            # Priority
            if [[ $line =~ ^\*\*Priority:\*\* ]]; then
                priority=$(echo "$line" | sed 's/^\*\*Priority:\*\*[[:space:]]*//' | sed 's/[[:space:]]*(.*)//')
            fi
            
            # Story Points
            if [[ $line =~ ^\*\*Story[[:space:]]Points:\*\* ]]; then
                story_points=$(echo "$line" | sed 's/^\*\*Story[[:space:]]Points:\*\*[[:space:]]*//')
            fi
        fi
        
    done < "$file"
    
    # Create last story
    if [ "$in_story" = true ] && [ -n "$current_story" ]; then
        create_github_issue "$service_label" "$current_epic" "$current_story" "$story_content" "$acceptance_criteria" "$tasks" "$priority" "$story_points"
    fi
    
    echo ""
}

# Function to create a GitHub issue
create_github_issue() {
    local service_label=$1
    local epic=$2
    local title=$3
    local story=$4
    local acceptance=$5
    local tasks=$6
    local priority=$(echo "$7" | xargs)  # Trim whitespace
    local points=$(echo "$8" | xargs)    # Trim whitespace
    
    # Build issue body
    local body="## Epic: $epic"$'\n\n'
    body+="$story"$'\n\n'
    
    if [ -n "$acceptance" ]; then
        body+="## Acceptance Criteria"$'\n'
        body+="$acceptance"$'\n'
    fi
    
    if [ -n "$tasks" ]; then
        body+="## Tasks"$'\n'
        body+="$tasks"$'\n'
    fi
    
    # Add metadata fields
    body+=$'\n'"---"$'\n'
    body+="**Service:** $service_label"$'\n'
    body+="**Priority:** ${priority:-N/A}"$'\n'
    body+="**Story Points:** ${points:-N/A}"$'\n'
    
    # Create the project item (draft issue)
    echo "    Creating: $title"
    $GH_CMD project item-create $PROJECT_NUMBER \
        --owner "$ORG" \
        --title "$title" \
        --body "$body" \
        2>&1 | grep -v "^$" || true
}

# Process each backlog file
process_backlog "$BACKLOGS_DIR/users_service_backlog.md" "Users Service" "Users"
process_backlog "$BACKLOGS_DIR/menu_service_backlog.md" "Menu Service" "Menu"
process_backlog "$BACKLOGS_DIR/orders_service_backlog.md" "Orders Service" "Orders"
process_backlog "$BACKLOGS_DIR/kitchen_service_backlog.md" "Kitchen Service" "Kitchen"
process_backlog "$BACKLOGS_DIR/payments_service_backlog.md" "Payments Service" "Payments"
process_backlog "$BACKLOGS_DIR/loyalty_service_backlog.md" "Loyalty Service" "Loyalty"

echo -e "${GREEN}✓ Backlog import completed!${NC}"
echo ""
echo "View your project at: https://github.com/orgs/$ORG/projects/$PROJECT_NUMBER"
echo "To create a project board, run: gh project create --title 'CampusEats Development' --owner TasteTest"
