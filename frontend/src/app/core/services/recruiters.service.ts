import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult } from '../models/pagination.models';
import {
  RecruiterResponse,
  CreateRecruiterRequest,
  UpdateRecruiterRequest,
} from '../models/recruiter.models';

@Injectable({ providedIn: 'root' })
export class RecruitersService {
  private readonly apiUrl = `${environment.apiBaseUrl}/Recruiters`;

  constructor(private readonly http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10): Observable<PaginatedResult<RecruiterResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginatedResult<RecruiterResponse>>(this.apiUrl, { params });
  }

  getById(id: number): Observable<RecruiterResponse> {
    return this.http.get<RecruiterResponse>(`${this.apiUrl}/${id}`);
  }

  getMine(): Observable<RecruiterResponse> {
    return this.http.get<RecruiterResponse>(`${this.apiUrl}/mine`);
  }

  create(request: CreateRecruiterRequest): Observable<RecruiterResponse> {
    return this.http.post<RecruiterResponse>(this.apiUrl, request);
  }

  update(id: number, request: UpdateRecruiterRequest): Observable<RecruiterResponse> {
    return this.http.put<RecruiterResponse>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
