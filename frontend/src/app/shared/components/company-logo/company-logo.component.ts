import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';

@Component({
  selector: 'app-company-logo',
  standalone: true,
  template: `
    <div class="company-logo" [class]="'company-logo--' + size()">
      @if (logoUrl() && !imageError()) {
        <img
          [src]="logoUrl()"
          [alt]="name() ? (name() + ' logo') : 'Company logo'"
          class="company-logo__img"
          (error)="onImageError()"
        />
      } @else {
        <span class="company-logo__fallback">
          {{ initials() }}
        </span>
      }
    </div>
  `,
  styles: `
    .company-logo {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      border-radius: var(--radius-md);
      overflow: hidden;
      border: 1px solid var(--border-color);
      background: #ffffff;
      box-shadow: var(--shadow-xs);

      &--sm { width: 36px; height: 36px; font-size: 0.75rem; border-radius: var(--radius-sm); }
      &--md { width: 48px; height: 48px; font-size: 0.9375rem; }
      &--lg { width: 64px; height: 64px; font-size: 1.25rem; border-radius: var(--radius-lg); }

      &__img {
        width: 100%;
        height: 100%;
        object-fit: contain;
        padding: 4px;
        background: #ffffff;
      }

      &__fallback {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 700;
        color: #ffffff;
        letter-spacing: -0.5px;
        text-transform: uppercase;
        background: var(--color-primary);
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompanyLogoComponent {
  readonly name = input<string | undefined>('');
  readonly logoUrl = input<string | undefined | null>(null);
  readonly size = input<'sm' | 'md' | 'lg'>('md');

  protected readonly imageError = signal(false);

  protected readonly initials = computed(() => {
    const raw = (this.name() || '').trim();
    if (!raw) return 'C';
    const words = raw.split(/\s+/).filter(Boolean);
    if (words.length >= 2) {
      return (words[0][0] + words[1][0]).toUpperCase();
    }
    return raw.slice(0, 2).toUpperCase();
  });

  onImageError(): void {
    this.imageError.set(true);
  }
}
