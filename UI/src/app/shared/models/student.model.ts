import { UserModel } from "./user.model";

export class StudentModel {

  id: number = 0;
  userId: number = 0;

  // Personal Info
  firstName: string = '';
  middleName: string = '';
  lastName: string = '';
  dateOfBirth?: Date;
  nationality: string = '';
  gender: string = '';
  bloodGroup: string = '';

  // Academic Info
  enrollmentNo: string = '';
  department: string = '';
  passingYear: number = 0;
  cgpa: number = 0;
  resumeUrl?: string;
  resume?: File | null = null;
  skills?: string;
  selectedCompanyName?: string;

  createdAt: Date = new Date();

  // Navigation
  user?: UserModel;

}
