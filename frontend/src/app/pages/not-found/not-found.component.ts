import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="error-page">
      <div class="error-page__code">404</div>
      <h1 class="error-page__title">Page Not Found</h1>
      <p class="error-page__message">The page you're looking for doesn't exist or has been moved.</p>
      <a routerLink="/" class="btn btn--primary">Go Home</a>
    </div>
  `,
  styles: `
    .error-page {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 60vh;
      text-align: center;
      padding: 2rem;

      &__code {
        font-size: 6rem;
        font-weight: 800;
        color: var(--color-primary);
        line-height: 1;
        margin-bottom: 1rem;
      }

      &__title {
        font-size: 1.5rem;
        margin-bottom: 0.75rem;
      }

      &__message {
        color: var(--text-secondary);
        margin-bottom: 2rem;
        max-width: 400px;
      }
    }
  `,
})
export class NotFoundComponent {}
