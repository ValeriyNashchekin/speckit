import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService } from '../../../core/api/api.service';
import { Tag, CreateTagRequest, UpdateTagRequest } from '../../../core/models';

@Injectable({
  providedIn: 'root',
})
export class TagsService {
  private readonly apiService = inject(ApiService);

  getTags(): Observable<Tag[]> {
    return this.apiService.get<Tag[]>('/tags');
  }

  getTag(id: string): Observable<Tag> {
    return this.apiService.get<Tag>(`/tags/${id}`);
  }

  createTag(dto: CreateTagRequest): Observable<Tag> {
    return this.apiService.post<Tag>('/tags', dto);
  }

  updateTag(id: string, dto: UpdateTagRequest): Observable<Tag> {
    return this.apiService.put<Tag>(`/tags/${id}`, dto);
  }

  deleteTag(id: string): Observable<void> {
    return this.apiService.delete<void>(`/tags/${id}`);
  }
}
