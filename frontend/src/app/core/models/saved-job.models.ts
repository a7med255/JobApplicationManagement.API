// ── Saved Job DTOs ────────────────────────────────────────────────────────────

export interface SavedJobResponse {
  id: number;
  jobId: number;
  jobTitle: string;
  jobDescription: string;
  isActive: boolean;
  savedAt: string;
}

export interface SavedJobStatus {
  isSaved: boolean;
}
