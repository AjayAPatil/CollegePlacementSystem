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

  getStudents(): Observable<ResponseModel<StudentModel[]>> {
    return this.http.get<ResponseModel<StudentModel[]>>(`${this.apiUrl}/student`);
  }
  getJobs(): Observable<ResponseModel<JobModel[]>> {
    return this.http.get<ResponseModel<JobModel[]>>(`${this.apiUrl}/jobs`);
  }

  getStudentById(studentId: number): Observable<ResponseModel<StudentModel>> {
    return this.http.get<ResponseModel<StudentModel>>(`${this.apiUrl}/student/${studentId}`);
  }

  getCompanyApplications(companyId: number): Observable<ResponseModel<CompanyJobApplicationListItem[]>> {
    return this.http.get<ResponseModel<CompanyJobApplicationListItem[]>>(
      `${this.apiUrl}/jobs/applications/company/${companyId}`
    );
  }

  getApplicationDetails(applicationId: number, companyId: number): Observable<ResponseModel<CompanyJobApplicationDetail>> {
    return this.http.get<ResponseModel<CompanyJobApplicationDetail>>(
      `${this.apiUrl}/jobs/applications/${applicationId}?companyId=${companyId}`
    );
  }

  scheduleInterview(applicationId: number, requestData: ScheduleInterviewRequestModel): Observable<ResponseModel<CompanyJobApplicationDetail>> {
    return this.http.patch<ResponseModel<CompanyJobApplicationDetail>>(
      `${this.apiUrl}/jobs/applications/${applicationId}/schedule-interview`,
      requestData
    );
  }

  updateApplicationStatus(applicationId: number, requestData: JobApplicationStatusUpdateRequestModel): Observable<ResponseModel<CompanyJobApplicationDetail>> {
    return this.http.patch<ResponseModel<CompanyJobApplicationDetail>>(
      `${this.apiUrl}/jobs/applications/${applicationId}/status`,
      requestData
    );
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
