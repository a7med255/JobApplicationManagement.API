import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult } from '../models/pagination.models';
import { SavedJobResponse, SavedJobStatus } from '../models/saved-job.models';

@Injectable({ providedIn: 'root' })
export class SavedJobsService {
  private readonly apiUrl = `${environment.apiBaseUrl}/jobs`;

  constructor(private readonly http: HttpClient) {}

  save(jobId: number): Observable<SavedJobResponse> {
    return this.http.post<SavedJobResponse>(`${this.apiUrl}/${jobId}/save`, {});
  }

  remove(jobId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${jobId}/save`);
  }

  getMySavedJobs(pageNumber = 1, pageSize = 10): Observable<PaginatedResult<SavedJobResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginatedResult<SavedJobResponse>>(`${this.apiUrl}/saved`, { params });
  }

  isSaved(jobId: number): Observable<SavedJobStatus> {
    return this.http.get<SavedJobStatus>(`${this.apiUrl}/${jobId}/saved-status`);
  }
}
