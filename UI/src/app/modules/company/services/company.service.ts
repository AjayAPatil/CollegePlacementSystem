import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments';
import { CompanyModel, JobModel, ResponseModel, StudentModel, UserModel } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class CompanyService {

  private apiUrl = environment.apiUrl; // Base URL for API endpoints

  constructor(private http: HttpClient) { }

  getStudents() {
    return this.http.get<Array<StudentModel>>(`${this.apiUrl}/student`);
  }
  getJobs() {
    return this.http.get<Array<JobModel>>(`${this.apiUrl}/jobs`);
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
  saveJobs(formData: FormData) {
    return this.http.post<ResponseModel>(`${this.apiUrl}/jobs`, formData);
  }
  updateJob(jobId: number, formData: FormData) {
    return this.http.put<ResponseModel>(`${this.apiUrl}/jobs/${jobId}`, formData);
  }
  updateJobStatus(jobId: number, status: string) {
    return this.http.patch<ResponseModel>(`${this.apiUrl}/jobs/${jobId}/status`, { status });
  }
  deleteJob(jobId: number) {
    return this.http.delete<ResponseModel>(`${this.apiUrl}/jobs/${jobId}`);
  }
}
