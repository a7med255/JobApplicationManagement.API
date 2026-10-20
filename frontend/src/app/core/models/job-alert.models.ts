import { EmploymentType } from './job.models';

export interface JobAlert {
  id: number;
  name: string;
  keyword: string | null;
  location: string | null;
  employmentType: EmploymentType | null;
  isActive: boolean;
  createdAt: string;
}

export type SaveJobAlertRequest = Omit<JobAlert, 'id' | 'createdAt'>;
