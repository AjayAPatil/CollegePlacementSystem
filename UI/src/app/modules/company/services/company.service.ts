import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments';
import {
  CompanyJobApplicationDetail,
  CompanyJobApplicationListItem,
  CompanyModel,
  JobApplicationStatusUpdateRequestModel,
  JobModel,
  ResponseModel,
  ScheduleInterviewRequestModel,
  StudentModel
} from '../../../shared';
import { Observable } from 'rxjs';

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

  getCompanyApplications(companyId: number): Observable<ResponseModel & { data: CompanyJobApplicationListItem[] }> {
    return this.http.get<ResponseModel & { data: CompanyJobApplicationListItem[] }>(
      `${this.apiUrl}/jobs/applications/company/${companyId}`
    );
  }

  getApplicationDetails(applicationId: number, companyId: number): Observable<ResponseModel & { data: CompanyJobApplicationDetail }> {
    return this.http.get<ResponseModel & { data: CompanyJobApplicationDetail }>(
      `${this.apiUrl}/jobs/applications/${applicationId}?companyId=${companyId}`
    );
  }

  scheduleInterview(applicationId: number, requestData: ScheduleInterviewRequestModel): Observable<ResponseModel & { data: CompanyJobApplicationDetail }> {
    return this.http.patch<ResponseModel & { data: CompanyJobApplicationDetail }>(
      `${this.apiUrl}/jobs/applications/${applicationId}/schedule-interview`,
      requestData
    );
  }

  updateApplicationStatus(applicationId: number, requestData: JobApplicationStatusUpdateRequestModel): Observable<ResponseModel & { data: CompanyJobApplicationDetail }> {
    return this.http.patch<ResponseModel & { data: CompanyJobApplicationDetail }>(
      `${this.apiUrl}/jobs/applications/${applicationId}/status`,
      requestData
    );
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
