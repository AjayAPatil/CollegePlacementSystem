import { CompanyModel } from "./company.model";
import { StudentModel } from "./student.model";

export class UserModel {
    isLoggedIn: boolean = false;

    // User Info
    userId: number = 0;
    userName: string = '';
    passwordHash: string = '';
    role: string = ''; // Admin, Student, Company
    status: string = 'Active'; // Active, Pending, Rejected
    profileImagePath?: string;
    profilePhoto?: File | null = null;

    // Contact Info
    email: string = '';
    mobileNo: string = '';
    streetAddress: string = '';
    city: string = '';
    district: string = '';
    state: string = '';
    country: string = '';
    pinCode: number = 0;

    isDeleted: boolean = false;
    createdAt: Date = new Date();
    updatedAt: Date = new Date();

    // Navigation
    student?: StudentModel | null = new StudentModel();
    company?: CompanyModel | null = new CompanyModel();

}
