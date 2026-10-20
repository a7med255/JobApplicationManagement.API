import { ChangeDetectionStrategy, Component, computed, effect, HostListener, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { APP_CONFIG } from '../../core/config/app.constants';
import { NotificationsService } from '../../core/services/notifications.service';
import { AppNotification } from '../../core/models/notification.models';

import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, DatePipe],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent {
  protected readonly auth = inject(AuthService);
  protected readonly notificationsService = inject(NotificationsService);
  private readonly router = inject(Router);

  protected readonly appConfig = APP_CONFIG;
  protected readonly menuOpen = signal(false);
  protected readonly notificationsOpen = signal(false);
  protected readonly userMenuOpen = signal(false);
  protected readonly recentNotifications = signal<AppNotification[]>([]);

  protected readonly userInitials = computed(() => {
    const email = this.auth.userEmail();
    if (!email) return 'U';
    const namePart = email.split('@')[0];
    return namePart.slice(0, 2).toUpperCase();
  });

  protected readonly primaryRole = computed(() => {
    const roles = this.auth.userRoles();
    return roles.length > 0 ? roles[0] : 'User';
  });

  constructor() {
    effect(() => {
      if (this.auth.isAuthenticated()) {
        this.refreshNotifications();
      } else {
        this.recentNotifications.set([]);
      }
    });
  }

  toggleMenu(): void {
    this.menuOpen.update((open) => !open);
    if (this.menuOpen()) {
      this.notificationsOpen.set(false);
      this.userMenuOpen.set(false);
    }
  }

  closeMenu(): void {
    this.menuOpen.set(false);
    this.notificationsOpen.set(false);
    this.userMenuOpen.set(false);
  }

  toggleNotifications(event?: Event): void {
    event?.stopPropagation();
    this.notificationsOpen.update((value) => !value);
    if (this.notificationsOpen()) {
      this.userMenuOpen.set(false);
      this.refreshNotifications();
    }
  }

  toggleUserMenu(event?: Event): void {
    event?.stopPropagation();
    this.userMenuOpen.update((value) => !value);
    if (this.userMenuOpen()) {
      this.notificationsOpen.set(false);
    }
  }

  openNotification(notification: AppNotification): void {
    const navigate = () => {
      this.closeMenu();
      if (notification.relatedEntityType === 'Application' && notification.relatedEntityId) {
        const prefix = this.auth.isCandidate() ? '/candidate/applications' : '/recruiter/applications';
        void this.router.navigate([prefix, notification.relatedEntityId]);
      } else {
        void this.router.navigate(['/notifications']);
      }
    };

    if (notification.isRead) {
      navigate();
    } else {
      this.notificationsService.markRead(notification.id).subscribe({
        next: navigate,
        error: navigate,
      });
    }
  }

  logout(): void {
    this.closeMenu();
    this.auth.logout();
  }

  @HostListener('document:click')
  closeFloatingMenus(): void {
    this.notificationsOpen.set(false);
    this.userMenuOpen.set(false);
  }

  @HostListener('document:keydown.escape')
  closeOnEscape(): void {
    this.closeMenu();
  }

  private refreshNotifications(): void {
    this.notificationsService.refreshUnreadCount();
    this.notificationsService.getMine(1, 5).subscribe({
      next: (result) => this.recentNotifications.set(result.items),
      error: () => this.recentNotifications.set([]),
    });
  }
}
