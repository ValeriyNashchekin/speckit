// DTOs matching backend Category entity

export interface Category {
  id: string;
  name: string;
  description: string | null;
  sortOrder: number;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string | null;
  sortOrder?: number;
}

export interface UpdateCategoryRequest {
  name?: string;
  description?: string | null;
  sortOrder?: number;
}
