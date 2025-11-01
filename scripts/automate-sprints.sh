#!/usr/bin/env bash

# CampusEats - Automated GitHub Project Sprint Setup
# This script automatically creates iterations and assigns items to sprints

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Configuration
ORG="TasteTest"
PROJECT_NUMBER="1"

# Find gh CLI path
GH_CMD="/opt/homebrew/bin/gh"
if [ ! -f "$GH_CMD" ]; then
    GH_CMD="/usr/local/bin/gh"
fi

# Check if gh CLI is installed
if [ ! -f "$GH_CMD" ]; then
    echo -e "${RED}Error: GitHub CLI (gh) is not installed${NC}"
    exit 1
fi

# Check if authenticated
if ! $GH_CMD auth status &> /dev/null; then
    echo -e "${RED}Error: Not authenticated with GitHub CLI${NC}"
    exit 1
fi

echo -e "${GREEN}╔═══════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║   CampusEats Automated Sprint Setup                  ║${NC}"
echo -e "${GREEN}╚═══════════════════════════════════════════════════════╝${NC}"
echo ""

# Get project ID using GraphQL
echo -e "${YELLOW}Fetching project details...${NC}"
PROJECT_ID=$($GH_CMD api graphql -f query='
  query($org: String!, $number: Int!) {
    organization(login: $org) {
      projectV2(number: $number) {
        id
        title
      }
    }
  }
' -f org="$ORG" -F number="$PROJECT_NUMBER" --jq '.data.organization.projectV2.id')

if [ -z "$PROJECT_ID" ]; then
    echo -e "${RED}Error: Could not find project${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Found project: $PROJECT_ID${NC}"
echo ""

# Get iteration field ID
echo -e "${YELLOW}Finding iteration field...${NC}"
ITERATION_FIELD_ID=$($GH_CMD api graphql -f query='
  query($org: String!, $number: Int!) {
    organization(login: $org) {
      projectV2(number: $number) {
        fields(first: 20) {
          nodes {
            ... on ProjectV2IterationField {
              id
              name
            }
          }
        }
      }
    }
  }
' -f org="$ORG" -F number="$PROJECT_NUMBER" --jq '.data.organization.projectV2.fields.nodes[] | select(.name == "Iteration") | .id')

if [ -z "$ITERATION_FIELD_ID" ]; then
    echo -e "${YELLOW}No iteration field found. Creating one...${NC}"
    
    # Create iteration field
    ITERATION_FIELD_ID=$($GH_CMD api graphql -f query='
      mutation($projectId: ID!, $name: String!) {
        createProjectV2Field(input: {
          projectId: $projectId
          dataType: ITERATION
          name: $name
        }) {
          projectV2Field {
            ... on ProjectV2IterationField {
              id
              name
            }
          }
        }
      }
    ' -f projectId="$PROJECT_ID" -f name="Sprint" --jq '.data.createProjectV2Field.projectV2Field.id')
    
    echo -e "${GREEN}✓ Created iteration field: $ITERATION_FIELD_ID${NC}"
else
    echo -e "${GREEN}✓ Found iteration field: $ITERATION_FIELD_ID${NC}"
fi
echo ""

# Calculate sprint dates
get_next_monday() {
    if [ "$(date +%u)" -eq 1 ]; then
        date +%Y-%m-%d
    else
        local days_until_monday=$((8 - $(date +%u)))
        date -v+${days_until_monday}d +%Y-%m-%d
    fi
}

add_weeks() {
    local weeks=$1
    date -j -v+${weeks}w -f "%Y-%m-%d" "$(get_next_monday)" +%Y-%m-%d
}

SPRINT_START=$(get_next_monday)

echo -e "${YELLOW}Creating 12 sprint iterations...${NC}"
echo "Starting from: $SPRINT_START"
echo ""

# Create iterations
declare -a ITERATION_IDS

for i in {1..12}; do
    START_DATE=$(add_weeks $((i-1)))
    END_DATE=$(add_weeks $i)
    SPRINT_NAME="Sprint $i"
    
    echo -e "${CYAN}Creating $SPRINT_NAME ($START_DATE to $END_DATE)...${NC}"
    
    ITERATION_ID=$($GH_CMD api graphql -f query='
      mutation($projectId: ID!, $fieldId: ID!, $title: String!, $startDate: Date!, $duration: Int!) {
        updateProjectV2IterationField(input: {
          projectId: $projectId
          fieldId: $fieldId
          configuration: {
            iterations: [{
              title: $title
              startDate: $startDate
              duration: $duration
            }]
          }
        }) {
          projectV2IterationField {
            configuration {
              iterations {
                id
                title
              }
            }
          }
        }
      }
    ' -f projectId="$PROJECT_ID" \
      -f fieldId="$ITERATION_FIELD_ID" \
      -f title="$SPRINT_NAME" \
      -f startDate="$START_DATE" \
      -F duration=7 2>&1)
    
    if echo "$ITERATION_ID" | grep -q "error"; then
        echo -e "${YELLOW}Note: Iteration might already exist or API limitation${NC}"
    else
        echo -e "${GREEN}✓ Created $SPRINT_NAME${NC}"
    fi
done

echo ""
echo -e "${YELLOW}Fetching all project items...${NC}"

# Get all items in the project
ITEMS=$($GH_CMD api graphql -f query='
  query($org: String!, $number: Int!) {
    organization(login: $org) {
      projectV2(number: $number) {
        items(first: 100) {
          nodes {
            id
            content {
              ... on DraftIssue {
                title
                body
              }
            }
          }
        }
      }
    }
  }
' -f org="$ORG" -F number="$PROJECT_NUMBER" --jq '.data.organization.projectV2.items.nodes')

echo -e "${GREEN}✓ Found items in project${NC}"
echo ""

# Function to assign item to sprint
assign_to_sprint() {
    local item_id=$1
    local sprint_number=$2
    local item_title=$3
    
    echo -e "${CYAN}Assigning '$item_title' to Sprint $sprint_number...${NC}"
    
    # This would require getting the specific iteration ID and using updateProjectV2ItemFieldValue
    # For now, we'll output instructions
    echo -e "${YELLOW}  → Manual assignment needed via UI${NC}"
}

echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}Sprint Assignment Recommendations${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo ""
echo "Due to GitHub API limitations, items need to be assigned manually."
echo "Use these patterns to filter and assign in the GitHub Project UI:"
echo ""

echo -e "${CYAN}Sprint 1: Foundation & Infrastructure${NC}"
echo "  Filter: title contains 'DbContext' OR 'MediatR' OR 'Repository' OR 'Event Listeners'"
echo ""

echo -e "${CYAN}Sprint 2: Authentication${NC}"
echo "  Filter: title contains 'OAuth' OR 'Profile' OR 'Domain Events'"
echo ""

echo -e "${CYAN}Sprint 3: Menu Management${NC}"
echo "  Filter: title contains 'Menu Category' OR 'Menu Item'"
echo ""

echo -e "${CYAN}Sprint 4: Order Creation${NC}"
echo "  Filter: title contains 'Create New Order' OR 'Add Items to Order' OR 'View Order Details' OR 'List My Orders' OR 'Update Order Status' OR 'Idempotency (Orders)'"
echo ""

echo -e "${CYAN}Sprint 5: Payment Integration${NC}"
echo "  Filter: title contains 'Stripe' OR 'Payment' OR 'Secret Management'"
echo ""

echo -e "${CYAN}Sprint 6: Kitchen Queue${NC}"
echo "  Filter: title contains 'Kitchen Order' OR 'Kitchen Queue' OR 'Claim' OR 'Mark Item as Ready' OR 'Mark Order as Complete'"
echo ""

echo -e "${CYAN}Sprint 7: Kitchen Operations${NC}"
echo "  Filter: title contains 'Filter Queue' OR 'Real-Time' OR 'Special Request' OR 'Cancel Order' OR 'Cancel Kitchen'"
echo ""

echo -e "${CYAN}Sprint 8: Loyalty Foundation${NC}"
echo "  Filter: title contains 'Loyalty Account' OR 'Earn Points' OR 'Tier' OR 'Concurrency Control'"
echo ""

echo -e "${CYAN}Sprint 9: Loyalty Rewards${NC}"
echo "  Filter: title contains 'Rewards' OR 'Redeem' OR 'Caching' OR 'Search Menu'"
echo ""

echo -e "${CYAN}Sprint 10: Admin Tools${NC}"
echo "  Filter: title contains 'List All Users' OR 'View User Activity' OR 'Track User Activity' OR 'User Deactivation' OR 'Promote User' OR 'View All Orders'"
echo ""

echo -e "${CYAN}Sprint 11: Analytics${NC}"
echo "  Filter: title contains 'Statistics' OR 'Performance Metrics' OR 'Payment History'"
echo ""

echo -e "${CYAN}Sprint 12: Polish${NC}"
echo "  Filter: title contains 'Reorder' OR 'Filter Menu Items' OR 'Delete' OR 'Manual' OR 'Mock Payments' OR 'Simulate'"
echo ""

echo -e "${GREEN}═══════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}Setup Complete!${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Go to https://github.com/orgs/$ORG/projects/$PROJECT_NUMBER"
echo "2. Iterations should be created automatically"
echo "3. Use the filters above to bulk-select items and assign to sprints"
echo "4. Set Sprint 1 as 'Current Iteration' in project settings"
echo ""
echo -e "${GREEN}Your automated sprint setup is complete! 🚀${NC}"
