export interface Company {
  id: number;
  name: string;
  logoUrl: string | null;
  description: string | null;
  website: string | null;
  industry: string | null;
  companySize: string | null;
  headquarters: string | null;
  country: string | null;
  city: string | null;
  createdAt: string;
  updatedAt: string;
}

export type UpdateCompanyRequest = Pick<
  Company,
  | 'name'
  | 'logoUrl'
  | 'description'
  | 'website'
  | 'industry'
  | 'companySize'
  | 'headquarters'
  | 'country'
  | 'city'
>;
