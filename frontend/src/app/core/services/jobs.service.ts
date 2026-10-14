import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult } from '../models/pagination.models';
import {
  CloseJobResponse,
  CreateJobRequest,
  JobResponse,
  JobSearchFilters,
  JobSummary,
  UpdateJobRequest,
} from '../models/job.models';

@Injectable({ providedIn: 'root' })
export class JobsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/Jobs`;

  getAll(
    pageNumber = 1,
    pageSize = 12,
    filters: JobSearchFilters = {},
  ): Observable<PaginatedResult<JobSummary>> {
    return this.http.get<PaginatedResult<JobSummary>>(this.apiUrl, {
      params: this.toParams(pageNumber, pageSize, filters),
    });
  }

  getMine(
    pageNumber = 1,
    pageSize = 12,
    filters: JobSearchFilters = {},
  ): Observable<PaginatedResult<JobSummary>> {
    return this.http.get<PaginatedResult<JobSummary>>(`${this.apiUrl}/mine`, {
      params: this.toParams(pageNumber, pageSize, filters),
    });
  }

  getById(id: number): Observable<JobResponse> {
    return this.http.get<JobResponse>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateJobRequest): Observable<JobResponse> {
    return this.http.post<JobResponse>(this.apiUrl, request);
  }

  update(id: number, request: UpdateJobRequest): Observable<JobResponse> {
    return this.http.put<JobResponse>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  publish(id: number): Observable<JobResponse> {
    return this.http.put<JobResponse>(`${this.apiUrl}/${id}/publish`, {});
  }

  close(id: number): Observable<CloseJobResponse> {
    return this.http.put<CloseJobResponse>(`${this.apiUrl}/${id}/close`, {});
  }

  reopen(id: number): Observable<JobResponse> {
    return this.http.put<JobResponse>(`${this.apiUrl}/${id}/reopen`, {});
  }

  archive(id: number): Observable<JobResponse> {
    return this.http.put<JobResponse>(`${this.apiUrl}/${id}/archive`, {});
  }

  private toParams(pageNumber: number, pageSize: number, filters: JobSearchFilters): HttpParams {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return params;
  }
}
