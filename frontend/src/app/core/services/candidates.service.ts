import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult } from '../models/pagination.models';
import {
  CandidateResponse,
  CreateCandidateRequest,
  UpdateCandidateRequest,
} from '../models/candidate.models';

@Injectable({ providedIn: 'root' })
export class CandidatesService {
  private readonly apiUrl = `${environment.apiBaseUrl}/Candidates`;

  constructor(private readonly http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10): Observable<PaginatedResult<CandidateResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginatedResult<CandidateResponse>>(this.apiUrl, { params });
  }

  getById(id: number): Observable<CandidateResponse> {
    return this.http.get<CandidateResponse>(`${this.apiUrl}/${id}`);
  }

  getMine(): Observable<CandidateResponse> {
    return this.http.get<CandidateResponse>(`${this.apiUrl}/mine`);
  }

  create(request: CreateCandidateRequest): Observable<CandidateResponse> {
    return this.http.post<CandidateResponse>(this.apiUrl, request);
  }

  update(id: number, request: UpdateCandidateRequest): Observable<CandidateResponse> {
    return this.http.put<CandidateResponse>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
