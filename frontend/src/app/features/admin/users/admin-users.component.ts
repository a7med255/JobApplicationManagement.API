import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { AdminUser, OutboxFailure } from '../../../core/models/admin.models';
import { AppRole } from '../../../core/models/auth.models';
import { AdminOperationsService } from '../../../core/services/admin-operations.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [DatePipe, RouterLink, EmptyStateComponent],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminUsersComponent implements OnInit {
  private readonly service = inject(AdminOperationsService);
  private readonly apiError = inject(ApiErrorService);

  protected readonly users = signal<AdminUser[]>([]);
  protected readonly failures = signal<OutboxFailure[]>([]);
  protected readonly loading = signal(true);
  protected readonly pendingUserId = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly activeTab = signal<'users' | 'failures'>('users');
  protected readonly roles: AppRole[] = ['Candidate', 'Recruiter', 'Admin'];
  protected readonly pageNumber = signal(1);
  protected readonly pageSize = 25;
  protected readonly hasNextPage = computed(() => this.users().length === this.pageSize);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    forkJoin({
      users: this.service.getUsers(this.pageNumber(), this.pageSize),
      failures: this.service.getOutboxFailures(),
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: ({ users, failures }) => {
          this.users.set(users);
          this.failures.set(failures);
        },
        error: (e: unknown) => {
          this.error.set(this.apiError.getMessage(e, 'Administration data could not be loaded.'));
        },
      });
  }

  previousPage(): void {
    if (this.pageNumber() <= 1) return;
    this.pageNumber.update((page) => page - 1);
    this.load();
  }

  nextPage(): void {
    if (!this.hasNextPage()) return;
    this.pageNumber.update((page) => page + 1);
    this.load();
  }

  toggleLock(user: AdminUser): void {
    this.pendingUserId.set(user.userId);
    this.service
      .setLock(user.userId, !user.isLocked)
      .pipe(finalize(() => this.pendingUserId.set(null)))
      .subscribe({
        next: () => this.load(),
        error: (e: unknown) => {
          this.error.set(this.apiError.getMessage(e, 'Account status could not be changed.'));
        },
      });
  }

  changeRole(user: AdminUser, event: Event): void {
    const role = (event.target as HTMLSelectElement).value as AppRole;
    if (!this.roles.includes(role)) return;

    this.pendingUserId.set(user.userId);
    this.service
      .setRoles(user.userId, [role])
      .pipe(finalize(() => this.pendingUserId.set(null)))
      .subscribe({
        next: () => this.load(),
        error: (e: unknown) => {
          this.error.set(this.apiError.getMessage(e, 'Roles could not be changed.'));
          this.load();
        },
      });
  }
}
