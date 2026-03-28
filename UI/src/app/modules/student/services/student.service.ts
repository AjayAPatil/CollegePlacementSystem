import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments';
import { CompanyModel, JobApplyRequestModel, JobDetailModel, JobFeedItem, JobModel, PagedResult, ResponseModel, StudentModel, UserModel } from '../../../shared';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StudentService {

  private apiUrl = environment.apiUrl; // Base URL for API endpoints

  constructor(private http: HttpClient) { }

  getStudents() {
    return this.http.get<Array<StudentModel>>(`${this.apiUrl}/student`);
  }
  getJobs(page: number, pageSize: number, studentId?: number): Observable<ResponseModel & { data: PagedResult<JobFeedItem> }> {
    const studentQuery = studentId ? `?studentId=${studentId}` : '';
    return this.http.get<ResponseModel & { data: PagedResult<JobFeedItem> }>(`${this.apiUrl}/jobs/${page}/${pageSize}${studentQuery}`);
  }

  getJobDetails(jobId: number): Observable<ResponseModel & { data: JobDetailModel }> {
    return this.http.get<ResponseModel & { data: JobDetailModel }>(`${this.apiUrl}/jobs/details/${jobId}`);
  }

  applyForJob(jobId: number, requestData: JobApplyRequestModel): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.apiUrl}/jobs/${jobId}/apply`, requestData);
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
