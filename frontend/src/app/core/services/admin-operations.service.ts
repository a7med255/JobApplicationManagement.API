import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminUser, OutboxFailure } from '../models/admin.models';
import { AppRole } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AdminOperationsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/admin`;
  getUsers(pageNumber = 1, pageSize = 50): Observable<AdminUser[]> { return this.http.get<AdminUser[]>(`${this.apiUrl}/users`, { params: new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize) }); }
  setLock(userId: string, locked: boolean): Observable<void> { return this.http.put<void>(`${this.apiUrl}/users/${encodeURIComponent(userId)}/${locked ? 'lock' : 'unlock'}`, {}); }
  setRoles(userId: string, roles: AppRole[]): Observable<void> { return this.http.put<void>(`${this.apiUrl}/users/${encodeURIComponent(userId)}/roles`, { roles }); }
  getOutboxFailures(): Observable<OutboxFailure[]> { return this.http.get<OutboxFailure[]>(`${this.apiUrl}/outbox-failures`); }
}
