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

  getStudents(): Observable<ResponseModel<StudentModel[]>> {
    return this.http.get<ResponseModel<StudentModel[]>>(`${this.apiUrl}/student`);
  }
  getJobs(page: number, pageSize: number, studentId?: number): Observable<ResponseModel<PagedResult<JobFeedItem>>> {
    const studentQuery = studentId ? `?studentId=${studentId}` : '';
    return this.http.get<ResponseModel<PagedResult<JobFeedItem>>>(`${this.apiUrl}/jobs/${page}/${pageSize}${studentQuery}`);
  }

  getJobDetails(jobId: number): Observable<ResponseModel<JobDetailModel>> {
    return this.http.get<ResponseModel<JobDetailModel>>(`${this.apiUrl}/jobs/details/${jobId}`);
  }

  applyForJob(jobId: number, requestData: JobApplyRequestModel): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.apiUrl}/jobs/${jobId}/apply`, requestData);
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
  saveJobs(formData: FormData): Observable<ResponseModel<JobModel>> {
    return this.http.post<ResponseModel<JobModel>>(`${this.apiUrl}/jobs`, formData);
  }
  updateJob(jobId: number, formData: FormData): Observable<ResponseModel<JobModel>> {
    return this.http.put<ResponseModel<JobModel>>(`${this.apiUrl}/jobs/${jobId}`, formData);
  }
  updateJobStatus(jobId: number, status: string): Observable<ResponseModel> {
    return this.http.patch<ResponseModel>(`${this.apiUrl}/jobs/${jobId}/status`, { status });
  }
  deleteJob(jobId: number): Observable<ResponseModel> {
    return this.http.delete<ResponseModel>(`${this.apiUrl}/jobs/${jobId}`);
  }
}
