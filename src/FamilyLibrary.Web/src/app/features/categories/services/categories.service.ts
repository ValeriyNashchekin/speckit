import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/api/api.service';
import {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../../../core/models';

@Injectable({
  providedIn: 'root',
})
export class CategoriesService {
  private readonly apiService = inject(ApiService);

  getCategories(): Observable<Category[]> {
    return this.apiService.get<Category[]>('/categories');
  }

  getCategory(id: string): Observable<Category> {
    return this.apiService.get<Category>(`/categories/${id}`);
  }

  createCategory(dto: CreateCategoryRequest): Observable<Category> {
    return this.apiService.post<Category>('/categories', dto);
  }

  updateCategory(id: string, dto: UpdateCategoryRequest): Observable<Category> {
    return this.apiService.put<Category>(`/categories/${id}`, dto);
  }

  deleteCategory(id: string): Observable<void> {
    return this.apiService.delete<void>(`/categories/${id}`);
  }
}
