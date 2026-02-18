import { computed, inject, Injectable, signal } from '@angular/core';
import { ApiService } from '../../core/api/api.service';
import {
  CreateFamilyRoleRequest,
  FamilyRole,
  FamilyRoleListRequest,
  UpdateFamilyRoleRequest,
} from '../../core/models';

@Injectable({
  providedIn: 'root',
})
export class RolesStore {
  private readonly api = inject(ApiService);

  // State signals
  readonly roles = signal<FamilyRole[]>([]);
  readonly selectedRole = signal<FamilyRole | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly totalCount = signal(0);

  // Computed values
  readonly hasRoles = computed(() => this.roles().length > 0);
  readonly isLoading = computed(() => this.loading());

  // API endpoints
  private readonly endpoint = '/roles';

  async loadRoles(params?: FamilyRoleListRequest): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const queryParams = params ? this.toQueryParams(params) : undefined;
      const response = await this.api
        .get<{ data: FamilyRole[]; totalCount: number }>(this.endpoint, { params: queryParams })
        .toPromise();

      if (response) {
        this.roles.set(response.data);
        this.totalCount.set(response.totalCount);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load roles';
      this.error.set(errorMessage);
    } finally {
      this.loading.set(false);
    }
  }

  async createRole(dto: CreateFamilyRoleRequest): Promise<FamilyRole | null> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const role = await this.api.post<FamilyRole>(this.endpoint, dto).toPromise();

      if (role) {
        this.roles.update(roles => [...roles, role]);
        return role;
      }
      return null;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to create role';
      this.error.set(errorMessage);
      return null;
    } finally {
      this.loading.set(false);
    }
  }

  async updateRole(id: string, dto: UpdateFamilyRoleRequest): Promise<FamilyRole | null> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const role = await this.api
        .put<FamilyRole>(`${this.endpoint}/${id}`, dto)
        .toPromise();

      if (role) {
        this.roles.update(roles => roles.map(r => (r.id === id ? role : r)));
        if (this.selectedRole()?.id === id) {
          this.selectedRole.set(role);
        }
        return role;
      }
      return null;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update role';
      this.error.set(errorMessage);
      return null;
    } finally {
      this.loading.set(false);
    }
  }

  async deleteRole(id: string): Promise<boolean> {
    this.loading.set(true);
    this.error.set(null);

    try {
      await this.api.delete<void>(`${this.endpoint}/${id}`).toPromise();
      this.roles.update(roles => roles.filter(r => r.id !== id));
      if (this.selectedRole()?.id === id) {
        this.selectedRole.set(null);
      }
      return true;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete role';
      this.error.set(errorMessage);
      return false;
    } finally {
      this.loading.set(false);
    }
  }

  selectRole(role: FamilyRole | null): void {
    this.selectedRole.set(role);
  }

  clearError(): void {
    this.error.set(null);
  }

  reset(): void {
    this.roles.set([]);
    this.selectedRole.set(null);
    this.loading.set(false);
    this.error.set(null);
    this.totalCount.set(0);
  }

  private toQueryParams(params: FamilyRoleListRequest): Record<string, string | number | boolean> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (params.page !== undefined) queryParams['page'] = params.page;
    if (params.pageSize !== undefined) queryParams['pageSize'] = params.pageSize;
    if (params.searchTerm) queryParams['searchTerm'] = params.searchTerm;
    if (params.categoryId) queryParams['categoryId'] = params.categoryId;
    if (params.type) queryParams['type'] = params.type;
    return queryParams;
  }
}
