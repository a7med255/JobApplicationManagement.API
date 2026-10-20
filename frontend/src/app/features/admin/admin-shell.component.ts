import { ChangeDetectionStrategy, Component, HostListener, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="admin-shell">
      <div class="admin-mobile-bar">
        <strong>Administration</strong>
        <button
          type="button"
          class="btn btn--secondary btn--sm"
          (click)="toggleNavigation()"
          [attr.aria-expanded]="navigationOpen()"
          aria-controls="admin-navigation"
        >Menu</button>
      </div>

      <aside id="admin-navigation" class="admin-sidebar" [class.admin-sidebar--open]="navigationOpen()">
        <div class="admin-sidebar__heading">
          <span class="admin-sidebar__eyebrow">Careerly</span>
          <strong>Admin workspace</strong>
        </div>
        <nav class="admin-nav" aria-label="Administration">
          <a routerLink="/admin/dashboard" routerLinkActive="active" (click)="closeNavigation()">Overview</a>
          <a routerLink="/admin/jobs" routerLinkActive="active" (click)="closeNavigation()">Jobs</a>
          <a routerLink="/admin/applications" routerLinkActive="active" (click)="closeNavigation()">Applications</a>
          <a routerLink="/admin/candidates" routerLinkActive="active" (click)="closeNavigation()">Candidates</a>
          <a routerLink="/admin/recruiters" routerLinkActive="active" (click)="closeNavigation()">Recruiters</a>
          <a routerLink="/admin/users" routerLinkActive="active" (click)="closeNavigation()">Users & operations</a>
        </nav>
      </aside>

      @if (navigationOpen()) {
        <button class="admin-backdrop" type="button" aria-label="Close administration menu" (click)="closeNavigation()"></button>
      }

      <div class="admin-content">
        <router-outlet />
      </div>
    </div>
  `,
  styles: `
    .admin-shell {
      display: grid;
      grid-template-columns: 232px minmax(0, 1fr);
      min-height: calc(100vh - var(--header-height));
      background: var(--bg-page);
    }

    .admin-sidebar {
      position: sticky;
      top: var(--header-height);
      align-self: start;
      height: calc(100vh - var(--header-height));
      padding: 1.5rem 1rem;
      overflow-y: auto;
      border-right: 1px solid var(--border-color);
      background: #ffffff;
      z-index: 40;
    }

    .admin-sidebar__heading {
      display: grid;
      gap: 0.15rem;
      padding: 0 0.75rem 1.25rem;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-primary);
    }

    .admin-sidebar__eyebrow {
      font-size: 0.6875rem;
      font-weight: 800;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      color: var(--color-primary);
    }

    .admin-nav {
      display: grid;
      gap: 0.25rem;
      padding-top: 1rem;
    }

    .admin-nav a {
      padding: 0.65rem 0.75rem;
      border-radius: var(--radius-md);
      color: var(--text-secondary);
      font-size: 0.875rem;
      font-weight: 600;
      text-decoration: none;
    }

    .admin-nav a:hover { background: var(--bg-surface); color: var(--text-primary); }
    .admin-nav a.active { background: var(--color-primary-light); color: var(--color-primary); }
    .admin-content { min-width: 0; }
    .admin-mobile-bar { display: none; }
    .admin-backdrop { display: none; }

    @media (max-width: 900px) {
      .admin-shell { display: block; }
      .admin-mobile-bar {
        position: sticky;
        top: var(--header-height);
        z-index: 35;
        display: flex;
        align-items: center;
        justify-content: space-between;
        min-height: 52px;
        padding: 0.5rem 1rem;
        border-bottom: 1px solid var(--border-color);
        background: #ffffff;
      }
      .admin-sidebar {
        position: fixed;
        top: var(--header-height);
        bottom: 0;
        left: 0;
        width: min(280px, 85vw);
        height: auto;
        transform: translateX(-100%);
        transition: transform 0.2s ease;
        box-shadow: var(--shadow-lg);
        z-index: 50;
      }
      .admin-sidebar--open { transform: translateX(0); }
      .admin-backdrop {
        display: block;
        position: fixed;
        inset: var(--header-height) 0 0;
        z-index: 45;
        width: 100%;
        border: 0;
        background: rgba(15, 23, 42, 0.5);
      }
    }
  `,
})
export class AdminShellComponent {
  protected readonly navigationOpen = signal(false);

  protected toggleNavigation(): void {
    this.navigationOpen.update((open) => !open);
  }

  protected closeNavigation(): void {
    this.navigationOpen.set(false);
  }

  @HostListener('document:keydown.escape')
  protected closeOnEscape(): void {
    this.closeNavigation();
  }
}
