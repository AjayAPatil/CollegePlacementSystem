import { UserModel } from "./user.model";

export class CompanyModel {

  id: number = 0;
  userId: number = 0;

  companyName: string = '';
  website?: string;
  description?: string;
  industry?: string;
  location?: string;

  hrName?: string;
  contactEmail?: string;
  contactPhone?: string;

  foundedYear?: number;
  companySize?: number;

  logoUrl?: string;

  createdAt: Date = new Date();

  // Navigation
  user?: UserModel;

}