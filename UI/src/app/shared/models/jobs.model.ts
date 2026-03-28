import { CompanyModel } from "./company.model";
import { UserModel } from "./user.model";

export class JobModel {
  jobId: number;
  companyId: number;
  jobTitle: string;
  department?: string;
  jobType: string;
  workMode: string;
  location: string;
  experienceMin: number;
  experienceMax: number;
  salaryMin?: number;
  salaryMax?: number;
  openings: number;
  responsibilities?: string;
  requiredSkills?: string;
  preferredSkills?: string;
  qualifications?: string;
  benefits?: string;
  status: string;
  expiryDate?: Date;
  createdBy: number;
  createdAt: Date;
  updatedAt?: Date;

  // Navigation
  company?: CompanyModel;
  creator?: UserModel;

  constructor(init?: Partial<JobModel>) {
    this.jobId = init?.jobId ?? 0;
    this.companyId = init?.companyId ?? 0;
    this.jobTitle = init?.jobTitle ?? '';
    this.department = init?.department;
    this.jobType = init?.jobType ?? '';
    this.workMode = init?.workMode ?? '';
    this.location = init?.location ?? '';
    this.experienceMin = init?.experienceMin ?? 0;
    this.experienceMax = init?.experienceMax ?? 0;
    this.salaryMin = init?.salaryMin;
    this.salaryMax = init?.salaryMax;
    this.openings = init?.openings ?? 1;
    this.responsibilities = init?.responsibilities;
    this.requiredSkills = init?.requiredSkills;
    this.preferredSkills = init?.preferredSkills;
    this.qualifications = init?.qualifications;
    this.benefits = init?.benefits;
    this.status = init?.status ?? 'draft';
    this.expiryDate = init?.expiryDate ? new Date(init.expiryDate) : undefined;
    this.createdBy = init?.createdBy ?? 0;
    this.createdAt = init?.createdAt ? new Date(init.createdAt) : new Date();
    this.updatedAt = init?.updatedAt ? new Date(init.updatedAt) : undefined;

    this.company = init?.company;
    this.creator = init?.creator;
  }
}

export interface JobFeedItem {
  jobId: number;
  companyId?: number;
  jobTitle: string;
  companyName: string;
  logoUrl?: string;
  creatorName: string;
  location: string;
  qualifications?: string;
  jobType: string;
  workMode: string;
  experienceMin: number;
  experienceMax: number;
  salaryMin?: number;
  salaryMax?: number;
  openings: number;
  benefits?: string;
  responsibilities?: string;
  requiredSkills?: string;
  preferredSkills?: string;
  status?: string;
  isApplied?: boolean;
  createdAt: Date | string;
  expiryDate?: Date | string;
}

export interface JobDetailModel extends JobFeedItem {
  department?: string;
  responsibilities?: string;
  requiredSkills?: string;
  preferredSkills?: string;
  companyWebsite?: string;
  companyDescription?: string;
  companyIndustry?: string;
  companyLocation?: string;
  companyHrName?: string;
  companyContactEmail?: string;
  companyContactPhone?: string;
  companyFoundedYear?: number;
  companySize?: number;
}

export interface JobApplyRequestModel {
  studentId: number;
}

export interface CompanyJobApplicationListItem {
  applicationId: number;
  jobId: number;
  companyId: number;
  studentId: number;
  studentUserId: number;
  studentName: string;
  studentEmail: string;
  studentPhone?: string;
  resumeFilePath?: string;
  status: string;
  appliedAt: Date | string;
  interviewScheduledAt?: Date | string;
  interviewMode?: string;
  interviewLocation?: string;
  interviewNotes?: string;
  decisionAt?: Date | string;
  updatedAt?: Date | string;
  jobTitle: string;
}

export interface CompanyJobApplicationDetail extends CompanyJobApplicationListItem {
  studentFirstName: string;
  studentMiddleName?: string;
  studentLastName: string;
  department?: string;
  passingYear: number;
  cgpa: number;
  skills?: string;
  resumeUrl?: string;
  jobType: string;
  workMode: string;
  location: string;
  qualifications?: string;
  requiredSkills?: string;
}

export interface ScheduleInterviewRequestModel {
  companyId: number;
  interviewScheduledAt: string;
  interviewMode?: string;
  interviewLocation?: string;
  interviewNotes?: string;
}

export interface JobApplicationStatusUpdateRequestModel {
  companyId: number;
  status: string;
}
