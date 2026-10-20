export enum NotificationType {
  ApplicationSubmitted = 0,
  NewApplication = 1,
  ApplicationUnderReview = 2,
  CandidateShortlisted = 3,
  InterviewScheduled = 4,
  InterviewRescheduled = 5,
  InterviewCancelled = 6,
  ApplicationAccepted = 7,
  ApplicationRejected = 8,
  System = 9,
}

export interface AppNotification {
  id: number;
  type: NotificationType;
  title: string;
  message: string;
  relatedEntityId: number | null;
  relatedEntityType: string | null;
  isRead: boolean;
  createdAt: string;
  readAt: string | null;
}

export interface NotificationPreferences {
  inAppEnabled: boolean;
  emailEnabled: boolean;
  jobAlertsEnabled: boolean;
}
