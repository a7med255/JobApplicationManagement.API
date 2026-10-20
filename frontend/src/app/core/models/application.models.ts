export enum JobApplicationStatus {
  Applied = 0,
  UnderReview = 1,
  Cancelled = 2,
  Interview = 3,
  /** @deprecated Kept temporarily for cached clients created before the spelling fix. */
  InterView = 3,
  Accepted = 4,
  Rejected = 5,
  Shortlisted = 6,
}

export const JOB_APPLICATION_STATUS_LABELS: Record<number, string> = {
  [JobApplicationStatus.Applied]: 'Applied',
  [JobApplicationStatus.UnderReview]: 'Under Review',
  [JobApplicationStatus.Cancelled]: 'Cancelled',
  [JobApplicationStatus.Interview]: 'Interview',
  [JobApplicationStatus.Accepted]: 'Accepted',
  [JobApplicationStatus.Rejected]: 'Rejected',
  [JobApplicationStatus.Shortlisted]: 'Shortlisted',
};

export interface ApplicationStatusHistory {
  id: number;
  oldStatus: JobApplicationStatus | null;
  newStatus: JobApplicationStatus;
  changedAt: string;
  note: string | null;
}

export enum InterviewType {
  Online = 0,
  OnSite = 1,
  Phone = 2,
}

export enum InterviewStatus {
  Scheduled = 0,
  Completed = 1,
  Cancelled = 2,
}

export interface InterviewSummary {
  id: number;
  scheduledAt: string;
  durationMinutes: number;
  interviewType: string;
  meetingLink: string | null;
  location: string | null;
  notes: string | null;
  status: string;
}

export interface Interview {
  id: number;
  applicationId: number;
  scheduledAt: string;
  durationMinutes: number;
  interviewType: InterviewType;
  meetingLink: string | null;
  location: string | null;
  notes: string | null;
  status: InterviewStatus;
  createdAt: string;
  updatedAt: string;
}

export interface ScheduleInterviewRequest {
  applicationId: number;
  scheduledAt: string;
  durationMinutes: number;
  interviewType: InterviewType;
  meetingLink: string | null;
  location: string | null;
  notes: string | null;
}

export type RescheduleInterviewRequest = Omit<ScheduleInterviewRequest, 'applicationId'>;

export interface CreateJobApplicationRequest {
  jobId: number;
  candidateCvId?: number;
  coverLetter?: string | null;
}

export interface UpdateJobApplicationRequest {
  jobApplicationStatus: JobApplicationStatus;
  note?: string | null;
}

export interface JobApplicationResponse {
  id: number;
  candidateId: number;
  candidateName: string | null;
  jobId: number;
  jobTitle: string | null;
  companyName: string | null;
  coverLetter: string | null;
  submittedCvId: number | null;
  submittedCvDisplayName: string | null;
  submittedCvFileName: string | null;
  jobApplicationStatus: JobApplicationStatus;
  appliedAt: string;
  statusUpdatedAt: string;
  cancelledAt: string | null;
  statusHistory: ApplicationStatusHistory[];
  interviews: InterviewSummary[];
}
