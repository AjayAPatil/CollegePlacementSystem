import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { GlobalService } from '../../../core';
import { convertUTCToIST, isSuccessResponse, ResponseModel, StudentJobApplicationListItem } from '../../../shared';
import { StudentService } from '../services/student.service';

@Component({
  selector: 'app-student-applications',
  standalone: false,
  templateUrl: './student-applications.component.html',
  styleUrl: './student-applications.component.scss',
})
export class StudentApplicationsComponent implements OnInit {
  loading = true;
  applications: StudentJobApplicationListItem[] = [];

  constructor(
    private readonly studentService: StudentService,
    private readonly globalService: GlobalService,
    private readonly cdref: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const studentId = this.globalService.userInfo?.student?.id ?? 0;
    if (!studentId) {
      this.loading = false;
      this.globalService.showErrorMessage('Student information is missing.');
      return;
    }

    this.studentService.getApplications(studentId).subscribe({
      next: (response: ResponseModel<StudentJobApplicationListItem[]>) => {
        this.loading = false;
        if (!isSuccessResponse(response)) {
          this.globalService.showErrorMessage(response.message || 'Failed to load applications.');
          return;
        }

        this.applications = (response.data ?? []).map((item) => this.normalizeApplication(item));
        this.cdref.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.globalService.showErrorMessage('Failed to load applications.');
      }
    });
  }

  getStatusLabel(status: string | undefined): string {
    if (!status) {
      return 'Applied';
    }

    return status
      .split('_')
      .map((part) => part.charAt(0).toUpperCase() + part.slice(1).toLowerCase())
      .join(' ');
  }

  private normalizeApplication(item: any): StudentJobApplicationListItem {
    var application: StudentJobApplicationListItem = {
      applicationId: item?.applicationId ?? item?.ApplicationId ?? 0,
      jobId: item?.jobId ?? item?.JobId ?? 0,
      companyId: item?.companyId ?? item?.CompanyId ?? 0,
      studentId: item?.studentId ?? item?.StudentId ?? 0,
      studentUserId: item?.studentUserId ?? item?.StudentUserId ?? 0,
      studentName: item?.studentName ?? item?.StudentName ?? '',
      studentEmail: item?.studentEmail ?? item?.StudentEmail ?? '',
      studentPhone: item?.studentPhone ?? item?.StudentPhone,
      resumeFilePath: item?.resumeFilePath ?? item?.ResumeFilePath,
      status: item?.status ?? item?.Status ?? 'applied',
      appliedAt: item?.appliedAt ?? item?.AppliedAt ?? '',
      interviewScheduledAt: item?.interviewScheduledAt ?? item?.InterviewScheduledAt,
      interviewMode: item?.interviewMode ?? item?.InterviewMode,
      interviewLocation: item?.interviewLocation ?? item?.InterviewLocation,
      interviewNotes: item?.interviewNotes ?? item?.InterviewNotes,
      decisionAt: item?.decisionAt ?? item?.DecisionAt,
      joiningDate: item?.joiningDate ?? item?.JoiningDate,
      updatedAt: item?.updatedAt ?? item?.UpdatedAt,
      jobTitle: item?.jobTitle ?? item?.JobTitle ?? '',
      companyName: item?.companyName ?? item?.CompanyName ?? '',
      companyLogoUrl: item?.companyLogoUrl ?? item?.CompanyLogoUrl,
      companyLocation: item?.companyLocation ?? item?.CompanyLocation,
      workMode: item?.workMode ?? item?.WorkMode
    };
    application.appliedAt = convertUTCToIST(application.appliedAt);
    if (application.interviewScheduledAt) {
      application.interviewScheduledAt = convertUTCToIST(application.interviewScheduledAt);
    }
    if (application.decisionAt) {
      application.decisionAt = convertUTCToIST(application.decisionAt);
    }
    if (application.joiningDate) {
      application.joiningDate = convertUTCToIST(application.joiningDate);
    }
    return application;
  }
}
