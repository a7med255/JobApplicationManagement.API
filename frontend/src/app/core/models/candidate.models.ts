// ── Candidate DTOs ────────────────────────────────────────────────────────────

export interface CreateCandidateRequest {
  name: string;
  cvUrl: string;
  userId: string;
}

export interface UpdateCandidateRequest {
  name: string;
  cvUrl: string;
}

export interface CandidateResponse {
  id: number;
  name: string;
  cvUrl: string;
  userId: string;
}
