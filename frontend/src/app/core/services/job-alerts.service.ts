import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { JobAlert, SaveJobAlertRequest } from '../models/job-alert.models';

@Injectable({ providedIn: 'root' })
export class JobAlertsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/job-alerts`;
  getMine(): Observable<JobAlert[]> { return this.http.get<JobAlert[]>(this.apiUrl); }
  create(request: SaveJobAlertRequest): Observable<JobAlert> { return this.http.post<JobAlert>(this.apiUrl, request); }
  update(id: number, request: SaveJobAlertRequest): Observable<JobAlert> { return this.http.put<JobAlert>(`${this.apiUrl}/${id}`, request); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.apiUrl}/${id}`); }
}
