import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments';
import { CompanyModel, ResponseModel, StudentModel, UserModel } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class CompanyService {

  private apiUrl = environment.apiUrl; // Base URL for API endpoints

  constructor(private http: HttpClient) { }

  getStudents() {
    return this.http.get<Array<StudentModel>>(`${this.apiUrl}/student`);
  }

  getStudentById(studentId: number) {
    return this.http.get<StudentModel>(`${this.apiUrl}/student/${studentId}`);
  }

  getCompanies() {
    return this.http.get<Array<CompanyModel>>(`${this.apiUrl}/company`);
  }
  saveCompany(formData: FormData) {
    return this.http.post<ResponseModel>(`${this.apiUrl}/company`, formData);
  }
}