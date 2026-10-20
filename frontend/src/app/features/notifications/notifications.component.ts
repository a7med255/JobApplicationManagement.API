import { ChangeDetectionStrategy, Component, OnInit, inject, signal, computed } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AppNotification, NotificationPreferences, NotificationType } from '../../core/models/notification.models';
import { NotificationsService } from '../../core/services/notifications.service';
import { ApiErrorService } from '../../core/services/api-error.service';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [DatePipe, NgClass, EmptyStateComponent, PaginationComponent],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationsComponent implements OnInit {
  private readonly service = inject(NotificationsService);
  private readonly router = inject(Router);
  private readonly apiError = inject(ApiErrorService);

  protected readonly notifications = signal<AppNotification[]>([]);
  protected readonly preferences = signal<NotificationPreferences | null>(null);
  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly filterMode = signal<'all' | 'unread'>('all');
  protected readonly showSettings = signal(false);
  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = 20;

  protected readonly NotificationType = NotificationType;

  protected readonly unreadCount = computed(() =>
    this.notifications().filter((n) => !n.isRead).length
  );

  protected readonly filteredNotifications = computed(() => {
    const mode = this.filterMode();
    const all = this.notifications();
    if (mode === 'unread') {
      return all.filter((n) => !n.isRead);
    }
    return all;
  });

  ngOnInit(): void {
    this.load();
    this.service.getPreferences().subscribe({
      next: (value) => this.preferences.set(value),
    });
  }

  load(): void {
    this.loading.set(true);
    this.service
      .getMine(this.pageNumber(), this.pageSize)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          this.notifications.set(result.items);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
          this.service.refreshUnreadCount();
        },
        error: (value: unknown) =>
          this.error.set(this.apiError.getMessage(value, 'Notifications could not be loaded.')),
      });
  }

  open(notification: AppNotification): void {
    const finish = () => {
      this.notifications.update((items) =>
        items.map((item) => (item.id === notification.id ? { ...item, isRead: true } : item))
      );
      this.service.refreshUnreadCount();
      const route = this.routeFor(notification);
      if (route) void this.router.navigate(route);
    };

    if (notification.isRead) {
      finish();
    } else {
      this.service.markRead(notification.id).subscribe({ next: finish, error: finish });
    }
  }

  markAllRead(): void {
    this.busy.set(true);
    this.service
      .markAllRead()
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: () => {
          this.notifications.update((items) => items.map((item) => ({ ...item, isRead: true })));
          this.service.refreshUnreadCount();
        },
        error: (value: unknown) =>
          this.error.set(this.apiError.getMessage(value, 'Notifications could not be updated.')),
      });
  }

  togglePreference(key: keyof NotificationPreferences, checked: boolean): void {
    const current = this.preferences();
    if (!current) return;
    const next = { ...current, [key]: checked };
    this.preferences.set(next);
    this.service.updatePreferences(next).subscribe({
      error: (value: unknown) => {
        this.preferences.set(current);
        this.error.set(this.apiError.getMessage(value, 'Preferences could not be saved.'));
      },
    });
  }

  toggleSettings(): void {
    this.showSettings.update((v) => !v);
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private routeFor(notification: AppNotification): unknown[] | null {
    if (notification.relatedEntityType !== 'Application' || !notification.relatedEntityId) return null;
    const isRecruiterNotice = notification.type === NotificationType.NewApplication;
    return isRecruiterNotice
      ? ['/recruiter/applications', notification.relatedEntityId]
      : ['/candidate/applications', notification.relatedEntityId];
  }
}
