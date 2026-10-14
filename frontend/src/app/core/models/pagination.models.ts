// ── Pagination Model ──────────────────────────────────────────────────────────
// Matches backend: JobApplicationManagement.Application.Common.Models.PaginatedResult<T>

export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
