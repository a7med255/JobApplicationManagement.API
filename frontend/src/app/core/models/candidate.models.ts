// ── Candidate DTOs ────────────────────────────────────────────────────────────

export interface CreateCandidateRequest {
  name: string;
  cvUrl: string;
  userId: string;
}

export interface UpdateCandidateRequest {
  name: string;
  cvUrl: string;
  professionalTitle: string | null;
  summary: string | null;
  country: string | null;
  city: string | null;
  yearsOfExperience: number | null;
}

export interface CandidateResponse {
  id: number;
  name: string;
  cvUrl: string;
  userId: string;
  profileImageUrl: string | null;
  professionalTitle: string | null;
  summary: string | null;
  country: string | null;
  city: string | null;
  yearsOfExperience: number | null;
  profileCompleteness: number;
}
