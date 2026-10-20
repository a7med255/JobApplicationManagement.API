import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppNotification, NotificationPreferences } from '../models/notification.models';
import { PaginatedResult } from '../models/pagination.models';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/notifications`;
  private readonly unreadCountSignal = signal(0);

  readonly unreadCount = this.unreadCountSignal.asReadonly();

  getMine(pageNumber = 1, pageSize = 20): Observable<PaginatedResult<AppNotification>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PaginatedResult<AppNotification>>(this.apiUrl, { params });
  }

  refreshUnreadCount(): void {
    this.http.get<{ count: number }>(`${this.apiUrl}/unread-count`).subscribe({
      next: ({ count }) => this.unreadCountSignal.set(count),
      error: () => this.unreadCountSignal.set(0),
    });
  }

  markRead(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => this.unreadCountSignal.update((value) => Math.max(0, value - 1))),
    );
  }

  markAllRead(): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => this.unreadCountSignal.set(0)),
    );
  }

  getPreferences(): Observable<NotificationPreferences> {
    return this.http.get<NotificationPreferences>(`${this.apiUrl}/preferences`);
  }

  updatePreferences(preferences: NotificationPreferences): Observable<NotificationPreferences> {
    return this.http.put<NotificationPreferences>(`${this.apiUrl}/preferences`, preferences);
  }
}
