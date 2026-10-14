// ── Recruiter DTOs ────────────────────────────────────────────────────────────

export interface CreateRecruiterRequest {
  name: string;
  userId: string;
}

export interface UpdateRecruiterRequest {
  name: string;
}

export interface RecruiterResponse {
  id: number;
  name: string;
  userId: string;
}
