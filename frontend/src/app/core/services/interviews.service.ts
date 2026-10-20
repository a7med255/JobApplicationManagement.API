import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Interview, RescheduleInterviewRequest, ScheduleInterviewRequest } from '../models/application.models';

@Injectable({ providedIn: 'root' })
export class InterviewsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/interviews`;
  getForApplication(applicationId: number): Observable<Interview[]> { return this.http.get<Interview[]>(`${this.apiUrl}/application/${applicationId}`); }
  schedule(request: ScheduleInterviewRequest): Observable<Interview> { return this.http.post<Interview>(this.apiUrl, request); }
  reschedule(id: number, request: RescheduleInterviewRequest): Observable<Interview> { return this.http.put<Interview>(`${this.apiUrl}/${id}`, request); }
  cancel(id: number): Observable<void> { return this.http.put<void>(`${this.apiUrl}/${id}/cancel`, {}); }
}
