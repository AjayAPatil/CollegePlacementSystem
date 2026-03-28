import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CompanyService } from '../services/company.service';
import { CompanyJobApplicationListItem } from '../../../shared';
import { MatTableDataSource } from '@angular/material/table';
import { GlobalService } from '../../../core';

@Component({
  selector: 'app-company-students',
  standalone: false,
  templateUrl: './company-students.component.html',
  styleUrl: './company-students.component.scss',
})
export class CompanyStudentsComponent implements OnInit {
  studentTblColumns: string[] = ['studentName', 'email', 'jobTitle', 'appliedAt', 'status', 'interview', 'action'];
  studentDataSource = new MatTableDataSource<CompanyJobApplicationListItem>();
  loading = false;

  constructor(
    private readonly companyService: CompanyService,
    private readonly globalService: GlobalService,
    private readonly router: Router,
    private cdref: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.getStudentList();
  }

  viewDetails(application: CompanyJobApplicationListItem): void {
    this.router.navigate(['/company/students', application.applicationId]);
  }

  getStatusLabel(status: string | undefined): string {
    if (!status) return 'Applied';

    switch (status.toLowerCase()) {
      case 'interview_scheduled':
        return 'Interview Scheduled';
      case 'accepted':
        return 'Accepted';
      case 'rejected':
        return 'Rejected';
      default:
        return 'Applied';
    }
  }

  private getStudentList(): void {
    const companyId = this.globalService.userInfo.company?.id ?? 0;
    if (!companyId) {
      this.globalService.showErrorMessage('Company profile is required to view applicants.');
      return;
    }

    this.loading = true;
    this.companyService.getCompanyApplications(companyId).subscribe({
      next: (response) => {
        this.loading = false;
        if (response?.status == '0') {
          this.studentDataSource.data = (response.data ?? []).map((item: CompanyJobApplicationListItem) => this.normalizeApplication(item));
          this.cdref.detectChanges();
          return;
        }

        this.globalService.showErrorMessage(response.message);
      },
      error: () => {
        this.loading = false;
        this.globalService.showErrorMessage('Failed to load applicants.');
      }
    });
  }

  private normalizeApplication(item: any): CompanyJobApplicationListItem {
    return {
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
      updatedAt: item?.updatedAt ?? item?.UpdatedAt,
      jobTitle: item?.jobTitle ?? item?.JobTitle ?? ''
    };
  }
}
