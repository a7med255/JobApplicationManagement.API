export enum JobStatus {
  Draft = 0,
  Published = 1,
  Closed = 2,
  Expired = 3,
  Archived = 4,
}

export enum WorkplaceType {
  OnSite = 0,
  Remote = 1,
  Hybrid = 2,
}

export enum EmploymentType {
  FullTime = 0,
  PartTime = 1,
  Contract = 2,
  Internship = 3,
  Freelance = 4,
  Temporary = 5,
}

export enum ExperienceLevel {
  Internship = 0,
  EntryLevel = 1,
  Junior = 2,
  MidLevel = 3,
  Senior = 4,
  Lead = 5,
  Manager = 6,
}

export enum SalaryPeriod {
  Hourly = 0,
  Monthly = 1,
  Yearly = 2,
}

export enum JobSortOrder {
  Newest = 0,
  Oldest = 1,
  SalaryLowToHigh = 2,
  SalaryHighToLow = 3,
}

export interface CompanySummary {
  id: number;
  name: string;
  logoUrl: string | null;
  description: string | null;
  website: string | null;
  industry: string | null;
  companySize: string | null;
  headquarters: string | null;
}

export interface JobSummary {
  id: number;
  title: string;
  company: CompanySummary;
  department: string;
  category: string;
  country: string;
  city: string;
  workplaceType: WorkplaceType;
  employmentType: EmploymentType;
  experienceLevel: ExperienceLevel;
  minimumYearsExperience: number;
  maximumYearsExperience: number | null;
  minimumSalary: number | null;
  maximumSalary: number | null;
  currency: string | null;
  salaryPeriod: SalaryPeriod | null;
  isSalaryVisible: boolean;
  numberOfVacancies: number;
  postedAt: string | null;
  applicationDeadline: string | null;
  status: JobStatus;
  isActive: boolean;
  requiredSkills: string[];
}

export interface JobResponse extends JobSummary {
  description: string;
  location: string | null;
  responsibilities: string;
  requirements: string;
  preferredQualifications: string | null;
  educationRequirements: string | null;
  benefits: string | null;
  createdAt: string;
  updatedAt: string;
  closedAt: string | null;
  preferredSkills: string[];
  isSaved: boolean;
  hasApplied: boolean;
  applicationId: number | null;
}

export interface JobWriteRequest {
  title: string;
  description: string;
  department: string;
  category: string;
  country: string;
  city: string;
  location: string | null;
  workplaceType: WorkplaceType;
  employmentType: EmploymentType;
  experienceLevel: ExperienceLevel;
  minimumYearsExperience: number;
  maximumYearsExperience: number | null;
  responsibilities: string;
  requirements: string;
  preferredQualifications: string | null;
  educationRequirements: string | null;
  benefits: string | null;
  minimumSalary: number | null;
  maximumSalary: number | null;
  currency: string | null;
  salaryPeriod: SalaryPeriod | null;
  isSalaryVisible: boolean;
  numberOfVacancies: number;
  applicationDeadline: string | null;
  requiredSkills: string[];
  preferredSkills: string[];
}

export interface CreateJobRequest extends JobWriteRequest {
  publishImmediately: boolean;
}

export type UpdateJobRequest = JobWriteRequest;
export type CloseJobResponse = JobResponse;

export interface JobSearchFilters {
  search?: string;
  companyId?: number;
  department?: string;
  category?: string;
  country?: string;
  city?: string;
  workplaceType?: WorkplaceType;
  employmentType?: EmploymentType;
  experienceLevel?: ExperienceLevel;
  minimumSalary?: number;
  maximumSalary?: number;
  skill?: string;
  postedWithinDays?: number;
  status?: JobStatus;
  includeInactive?: boolean;
  sort?: JobSortOrder;
}

export const JOB_STATUS_LABELS: Record<JobStatus, string> = {
  [JobStatus.Draft]: 'Draft',
  [JobStatus.Published]: 'Published',
  [JobStatus.Closed]: 'Closed',
  [JobStatus.Expired]: 'Expired',
  [JobStatus.Archived]: 'Archived',
};

export const WORKPLACE_TYPE_LABELS: Record<WorkplaceType, string> = {
  [WorkplaceType.OnSite]: 'On-site',
  [WorkplaceType.Remote]: 'Remote',
  [WorkplaceType.Hybrid]: 'Hybrid',
};

export const EMPLOYMENT_TYPE_LABELS: Record<EmploymentType, string> = {
  [EmploymentType.FullTime]: 'Full-time',
  [EmploymentType.PartTime]: 'Part-time',
  [EmploymentType.Contract]: 'Contract',
  [EmploymentType.Internship]: 'Internship',
  [EmploymentType.Freelance]: 'Freelance',
  [EmploymentType.Temporary]: 'Temporary',
};

export const EXPERIENCE_LEVEL_LABELS: Record<ExperienceLevel, string> = {
  [ExperienceLevel.Internship]: 'Internship',
  [ExperienceLevel.EntryLevel]: 'Entry level',
  [ExperienceLevel.Junior]: 'Junior',
  [ExperienceLevel.MidLevel]: 'Mid level',
  [ExperienceLevel.Senior]: 'Senior',
  [ExperienceLevel.Lead]: 'Lead',
  [ExperienceLevel.Manager]: 'Manager',
};

export const SALARY_PERIOD_LABELS: Record<SalaryPeriod, string> = {
  [SalaryPeriod.Hourly]: 'hour',
  [SalaryPeriod.Monthly]: 'month',
  [SalaryPeriod.Yearly]: 'year',
};
