import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './layout/navbar/navbar.component';

@Component({
  imports: [RouterOutlet, NavbarComponent],
  selector: 'app-root',
  template: `
    <a class="skip-link" href="#main-content">Skip to main content</a>
    <app-navbar />
    <main id="main-content" class="app-main" tabindex="-1">
      <router-outlet />
    </main>
  `,
  styles: `
    .skip-link {
      position: fixed;
      z-index: 2000;
      top: 0.5rem;
      left: 0.5rem;
      padding: 0.625rem 1rem;
      border-radius: var(--radius-sm);
      background: var(--text-primary);
      color: var(--bg-body);
      transform: translateY(-150%);
    }

    .skip-link:focus {
      transform: translateY(0);
    }

    .app-main {
      min-height: calc(100vh - 64px);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
