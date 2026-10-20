import { ChangeDetectionStrategy, Component, EventEmitter, input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="empty-state">
      @if (icon()) {
        <div class="empty-state__icon" aria-hidden="true">{{ icon() }}</div>
      }
      <h3 class="empty-state__title">{{ title() }}</h3>
      @if (message()) {
        <p class="empty-state__message">{{ message() }}</p>
      }
      <div class="empty-state__actions">
        <ng-content></ng-content>
        @if (actionText()) {
          @if (actionLink()) {
            <a [routerLink]="actionLink()" class="btn btn--primary">{{ actionText() }}</a>
          } @else {
            <button type="button" class="btn btn--primary" (click)="action.emit()">{{ actionText() }}</button>
          }
        }
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyStateComponent {
  readonly title = input.required<string>();
  readonly message = input<string>('');
  readonly icon = input<string>('🔍');
  readonly actionText = input<string | undefined>(undefined);
  readonly actionLink = input<string | undefined>(undefined);

  @Output() readonly action = new EventEmitter<void>();
}
