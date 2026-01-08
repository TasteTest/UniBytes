import { Result } from '../types/common.types';

const API_URL = process.env.NEXT_PUBLIC_API_URL || '';

export interface AIRequest {
    dietaryGoal: string;
    dietaryRestrictions: string[];
    allergies?: string;
    dislikes?: string;
    preferredMealType: string;
    caloriePreference: string;
    menuType: string;
}

export interface MenuItemDto {
    id: string;
    categoryId: string;
    name: string;
    description: string | null;
    price: number;
    currency: string;
    available: boolean;
    imageUrl: string | null;
}

export interface AIResponse {
    response: string;
    recommendedProducts: MenuItemDto[];
}

export class AIService {
    /**
     * Get personalized menu recommendations based on user preferences.
     */
    async getMenuRecommendations(request: AIRequest): Promise<AIResponse> {
        try {
            if (!API_URL) {
                throw new Error('API base URL not configured');
            }

            const response = await fetch(`${API_URL}/ai/chat`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(request),
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Failed to get menu recommendations');
            }

            const data: AIResponse = await response.json();
            return data;
        } catch (error) {
            throw error instanceof Error ? error : new Error('Failed to get menu recommendations');
        }
    }
}
