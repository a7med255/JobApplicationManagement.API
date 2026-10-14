// ── API Error Response ────────────────────────────────────────────────────────
// Matches backend: ExceptionHandlingMiddleware response format

export interface ApiErrorResponse {
  statusCode: number;
  message: string;
  errors: Record<string, string[]> | null;
  traceId: string;
}

export interface ValidationProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}
