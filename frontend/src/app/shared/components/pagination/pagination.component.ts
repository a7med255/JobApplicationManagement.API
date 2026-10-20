import { ChangeDetectionStrategy, Component, EventEmitter, input, Output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  template: `
    @if (totalPages() > 1) {
      <nav class="pagination" aria-label="Page navigation">
        <button
          type="button"
          class="btn btn--secondary btn--sm"
          [disabled]="pageNumber() <= 1"
          (click)="onPage(pageNumber() - 1)"
          aria-label="Previous page"
        >
          &larr; Previous
        </button>

        <span class="pagination__info">
          Page <strong>{{ pageNumber() }}</strong> of <strong>{{ totalPages() }}</strong>
          @if (totalItems() !== undefined) {
            <span class="pagination__total">({{ totalItems() }} items)</span>
          }
        </span>

        <button
          type="button"
          class="btn btn--secondary btn--sm"
          [disabled]="pageNumber() >= totalPages()"
          (click)="onPage(pageNumber() + 1)"
          aria-label="Next page"
        >
          Next &rarr;
        </button>
      </nav>
    }
  `,
  styles: `
    .pagination__total {
      color: var(--text-muted);
      font-weight: 400;
      margin-left: 0.25rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginationComponent {
  readonly pageNumber = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly totalItems = input<number | undefined>(undefined);

  @Output() readonly pageChange = new EventEmitter<number>();

  onPage(page: number): void {
    if (page >= 1 && page <= this.totalPages() && page !== this.pageNumber()) {
      this.pageChange.emit(page);
    }
  }
}
