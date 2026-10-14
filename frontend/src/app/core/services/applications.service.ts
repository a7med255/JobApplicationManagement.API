import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult } from '../models/pagination.models';
import {
  JobApplicationResponse,
  CreateJobApplicationRequest,
  UpdateJobApplicationRequest,
} from '../models/application.models';

@Injectable({ providedIn: 'root' })
export class ApplicationsService {
  private readonly apiUrl = `${environment.apiBaseUrl}/Applications`;

  constructor(private readonly http: HttpClient) {}

  /**
   * Returns applications scoped to the current user's role:
   * - Admin: all applications
   * - Recruiter: applications for their jobs
   * - Candidate: their own applications
   */
  getAll(pageNumber = 1, pageSize = 10): Observable<PaginatedResult<JobApplicationResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginatedResult<JobApplicationResponse>>(this.apiUrl, { params });
  }

  getById(id: number): Observable<JobApplicationResponse> {
    return this.http.get<JobApplicationResponse>(`${this.apiUrl}/${id}`);
  }

  apply(request: CreateJobApplicationRequest): Observable<JobApplicationResponse> {
    return this.http.post<JobApplicationResponse>(this.apiUrl, request);
  }

  updateStatus(id: number, request: UpdateJobApplicationRequest): Observable<JobApplicationResponse> {
    return this.http.put<JobApplicationResponse>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  cancel(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/cancel`, {});
  }

  downloadSubmittedCv(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/submitted-cv`, { responseType: 'blob' });
  }
}
