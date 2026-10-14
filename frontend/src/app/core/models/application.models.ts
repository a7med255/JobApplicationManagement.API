// ── Application Status Enum ───────────────────────────────────────────────────
// Matches backend: JobApplicationManagement.Domain.Enums.JobApplicationStatus

export enum JobApplicationStatus {
  Applied = 0,
  UnderReview = 1,
  Cancelled = 2,
  InterView = 3,
  Accepted = 4,
  Rejected = 5,
}

export const JOB_APPLICATION_STATUS_LABELS: Record<JobApplicationStatus, string> = {
  [JobApplicationStatus.Applied]: 'Applied',
  [JobApplicationStatus.UnderReview]: 'Under Review',
  [JobApplicationStatus.Cancelled]: 'Cancelled',
  [JobApplicationStatus.InterView]: 'Interview',
  [JobApplicationStatus.Accepted]: 'Accepted',
  [JobApplicationStatus.Rejected]: 'Rejected',
};

// ── Application DTOs ──────────────────────────────────────────────────────────

export interface CreateJobApplicationRequest {
  jobId: number;
  candidateCvId?: number;
}

export interface UpdateJobApplicationRequest {
  jobApplicationStatus: JobApplicationStatus;
}

export interface JobApplicationResponse {
  id: number;
  candidateId: number;
  candidateName: string | null;
  jobId: number;
  jobTitle: string | null;
  submittedCvId: number | null;
  submittedCvDisplayName: string | null;
  submittedCvFileName: string | null;
  jobApplicationStatus: JobApplicationStatus;
  appliedAt: string;
  statusUpdatedAt: string;
  cancelledAt: string | null;
}
