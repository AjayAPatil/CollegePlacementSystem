import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments';
import { CompanyModel, ResponseModel, StudentModel, UserModel } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  private apiUrl = environment.apiUrl; // Base URL for API endpoints

  constructor(private http: HttpClient) { }

  getStudents(): Observable<ResponseModel<StudentModel[]>> {
    return this.http.get<ResponseModel<StudentModel[]>>(`${this.apiUrl}/student`);
  }

  getStudentById(studentId: number): Observable<ResponseModel<StudentModel>> {
    return this.http.get<ResponseModel<StudentModel>>(`${this.apiUrl}/student/${studentId}`);
  }

  getCompanies(): Observable<ResponseModel<CompanyModel[]>> {
    return this.http.get<ResponseModel<CompanyModel[]>>(`${this.apiUrl}/company`);
  }
  saveCompany(formData: FormData): Observable<ResponseModel<CompanyModel>> {
    return this.http.post<ResponseModel<CompanyModel>>(`${this.apiUrl}/company`, formData);
  }
}
