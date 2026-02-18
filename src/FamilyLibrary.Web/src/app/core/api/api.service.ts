import { HttpClient, HttpContext, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface RequestOptions {
  headers?: HttpHeaders | Record<string, string | string[]>;
  context?: HttpContext;
  observe?: 'body';
  params?: HttpParams | Record<string, string | number | boolean | ReadonlyArray<string | number | boolean>>;
  reportProgress?: boolean;
  responseType?: 'json';
  withCredentials?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly baseUrl = '/api';

  get<T>(endpoint: string, options?: RequestOptions): Observable<T> {
    return this.httpClient.get<T>(`${this.baseUrl}${endpoint}`, options);
  }

  post<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T> {
    return this.httpClient.post<T>(`${this.baseUrl}${endpoint}`, body, options);
  }

  put<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T> {
    return this.httpClient.put<T>(`${this.baseUrl}${endpoint}`, body, options);
  }

  patch<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T> {
    return this.httpClient.patch<T>(`${this.baseUrl}${endpoint}`, body, options);
  }

  delete<T>(endpoint: string, options?: RequestOptions): Observable<T> {
    return this.httpClient.delete<T>(`${this.baseUrl}${endpoint}`, options);
  }
}
