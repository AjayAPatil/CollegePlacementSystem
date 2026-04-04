import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { GlobalService } from '../../../../core';
import { CompanyJobApplicationDetail } from '../../../../shared';
import { CompanyService } from '../../services/company.service';

@Component({
  selector: 'app-company-student-details',
  standalone: false,
  templateUrl: './company-student-details.component.html',
  styleUrl: './company-student-details.component.scss',
})
export class CompanyStudentDetailsComponent implements OnInit {
  applicationId = 0;
  companyId = 0;
  applicationDetails: CompanyJobApplicationDetail | null = null;
  loading = true;
  savingInterview = false;
  updatingDecision = false;
  interviewForm;
  minInterviewDate = new Date();
  minJoiningDate = new Date();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly formBuilder: FormBuilder,
    private readonly companyService: CompanyService,
    private readonly globalService: GlobalService,
    private cdref: ChangeDetectorRef
  ) {
    this.interviewForm = this.formBuilder.group({
      interviewDate: this.formBuilder.control<Date | null>(null, Validators.required),
      interviewTime: this.formBuilder.control<Date | null>(this.createTime, Validators.required),
      interviewMode: this.formBuilder.control<string>('Online'),
      interviewLocation: this.formBuilder.control<string>(''),
      interviewNotes: this.formBuilder.control<string>(''),
      joiningDate: this.formBuilder.control<Date | null>(null)
    });
    this.minJoiningDate.setDate(this.minJoiningDate.getDate() + 1);
  }

  ngOnInit(): void {
    this.applicationId = Number(this.route.snapshot.paramMap.get('applicationId') || 0);
    this.companyId = this.globalService.userInfo.company?.id ?? 0;

    if (!this.applicationId || !this.companyId) {
      this.router.navigate(['/company/students']);
      return;
    }

    this.loadApplicationDetails();
  }

  goBack(): void {
    this.router.navigate(['/company/students']);
  }

  scheduleInterview(): void {
    if (this.interviewForm.invalid || this.savingInterview) {
      this.interviewForm.markAllAsTouched();
      return;
    }

    const formValue = this.interviewForm.getRawValue();
    const interviewScheduledAt = this.combineInterviewDateAndTime(formValue.interviewDate, formValue.interviewTime || this.createTime);
    if (!interviewScheduledAt) {
      this.globalService.showErrorMessage('Please select a valid interview date and time.');
      return;
    }

    this.savingInterview = true;
    this.companyService.scheduleInterview(this.applicationId, {
      companyId: this.companyId,
      interviewScheduledAt: interviewScheduledAt.toISOString(),
      interviewMode: formValue.interviewMode || undefined,
      interviewLocation: formValue.interviewLocation || undefined,
      interviewNotes: formValue.interviewNotes || undefined
    }).subscribe({
      next: (response) => {
        this.savingInterview = false;
        if (response?.status == '0') {
          this.applicationDetails = this.normalizeApplication(response.data);
          this.patchInterviewForm(this.applicationDetails);
          this.globalService.showSuccessMessage(response.message);

          this.loadApplicationDetails();
          return;
        }

        this.globalService.showErrorMessage(response.message);
      },
      error: () => {
        this.savingInterview = false;
        this.globalService.showErrorMessage('Failed to schedule interview.');
      }
    });
  }

  updateDecision(status: 'accepted' | 'rejected'): void {
    if (!this.canTakeFinalDecision() || this.updatingDecision) {
      return;
    }

    const joiningDate = this.interviewForm.controls.joiningDate.value;
    if (status === 'accepted' && !joiningDate) {
      this.interviewForm.controls.joiningDate.markAsTouched();
      this.globalService.showErrorMessage('Joining date is required to accept the student.');
      return;
    }

    this.updatingDecision = true;
    this.companyService.updateApplicationStatus(this.applicationId, {
      companyId: this.companyId,
      status,
      joiningDate: status === 'accepted' && joiningDate ? this.toDateOnlyIsoString(joiningDate) : undefined
    }).subscribe({
      next: (response) => {
        this.updatingDecision = false;
        if (response?.status == '0') {
          this.applicationDetails = this.normalizeApplication(response.data);
          this.patchInterviewForm(this.applicationDetails);
          this.globalService.showSuccessMessage(response.message);
          return;
        }

        this.globalService.showErrorMessage(response.message);
      },
      error: () => {
        this.updatingDecision = false;
        this.globalService.showErrorMessage('Failed to update application status.');
      }
    });
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

  canTakeFinalDecision(): boolean {
    const interviewTime = this.applicationDetails?.interviewScheduledAt;
    if (!interviewTime) {
      return false;
    }

    return new Date(interviewTime).getTime() <= Date.now();
  }

  hasFinalDecision(): boolean {
    const status = (this.applicationDetails?.status || '').toLowerCase();
    return status === 'accepted' || status === 'rejected';
  }

  private loadApplicationDetails(): void {
    this.loading = true;
    this.companyService.getApplicationDetails(this.applicationId, this.companyId).subscribe({
      next: (response) => {
        this.loading = false;
        if (response?.status == '0') {
          this.applicationDetails = this.normalizeApplication(response.data);
          this.patchInterviewForm(this.applicationDetails);
          this.cdref.detectChanges();
          return;
        }

        this.globalService.showErrorMessage(response.message);
        this.goBack();
      },
      error: () => {
        this.loading = false;
        this.globalService.showErrorMessage('Failed to load applicant details.');
        this.goBack();
      }
    });
  }

  private patchInterviewForm(application: CompanyJobApplicationDetail | null): void {
    if (!application) {
      return;
    }

    this.interviewForm.patchValue({
      interviewDate: this.toInterviewDate(application.interviewScheduledAt),
      interviewTime: this.toInterviewTime(application.interviewScheduledAt),
      interviewMode: application.interviewMode || 'Online',
      interviewLocation: application.interviewLocation || '',
      interviewNotes: application.interviewNotes || '',
      joiningDate: this.toInterviewDate(application.joiningDate)
    });
  }

  private toInterviewDate(value: Date | string | undefined): Date | null {
    if (!value) {
      return null;
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return null;
    }

    return date;
  }

  private toInterviewTime(value: Date | string | undefined): Date | null {
    console.log('Converting interview time:', value);
    if (!value) {
      return this.createTime;
    }

    const date = new Date(value); // Treat input as UTC
    if (Number.isNaN(date.getTime())) {
      return this.createTime;
    }

    return date;
  }
  private get createTime(): Date {
    const d = new Date();
    d.setHours(9, 0, 0, 0);
    return d;
  }


  private combineInterviewDateAndTime(dateValue: Date | string | null, timeValue: any): Date | null {
    if (!dateValue || !timeValue) {
      return null;
    }

    const interviewDate = new Date(dateValue);
    if (Number.isNaN(interviewDate.getTime())) {
      return null;
    }

    if (typeof (timeValue) == 'string' && !/^\d{2}:\d{2}$/.test(timeValue)) {
      const [hoursText, minutesText] = timeValue.split(':');
      const hours = Number(hoursText);
      const minutes = Number(minutesText);
      if (Number.isNaN(hours) || Number.isNaN(minutes)) {
        return null;
      }

      interviewDate.setHours(hours, minutes, 0, 0);
    } else if (timeValue instanceof Date) {
      interviewDate.setHours(timeValue.getHours(), timeValue.getMinutes(), timeValue.getSeconds(), timeValue.getMilliseconds());
    } else {
      const dateTime = timeValue.toDate();
      if (Number.isNaN(dateTime.getTime())) {
        return null;
      }
      interviewDate.setHours(dateTime.getHours(), dateTime.getMinutes(), dateTime.getSeconds(), dateTime.getMilliseconds());
    }
    return interviewDate;
  }

  private normalizeApplication(item: any): CompanyJobApplicationDetail {
    var application: CompanyJobApplicationDetail = {
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
      studentFirstName: item?.studentFirstName ?? item?.StudentFirstName ?? '',
      studentMiddleName: item?.studentMiddleName ?? item?.StudentMiddleName ?? '',
      studentLastName: item?.studentLastName ?? item?.StudentLastName ?? '',
      department: item?.department ?? item?.Department,
      passingYear: item?.passingYear ?? item?.PassingYear ?? 0,
      cgpa: item?.cgpa ?? item?.CGPA ?? 0,
      skills: item?.skills ?? item?.Skills,
      resumeUrl: item?.resumeUrl ?? item?.ResumeUrl,
      jobType: item?.jobType ?? item?.JobType ?? '',
      workMode: item?.workMode ?? item?.WorkMode ?? '',
      location: item?.location ?? item?.Location ?? '',
      qualifications: item?.qualifications ?? item?.Qualifications,
      requiredSkills: item?.requiredSkills ?? item?.RequiredSkills
    };
    application.appliedAt = application.appliedAt ? this.convertToIST(application.appliedAt) : application.appliedAt;
    application.decisionAt = application.decisionAt ? this.convertToIST(application.decisionAt) : application.decisionAt;
    application.interviewScheduledAt = application.interviewScheduledAt ? this.convertToIST(application.interviewScheduledAt) : application.interviewScheduledAt;

    return application;
  }
  private toDateOnlyIsoString(value: Date | string): string {
    const date = new Date(value);
    return `${date.getFullYear()}-${`${date.getMonth() + 1}`.padStart(2, '0')}-${`${date.getDate()}`.padStart(2, '0')}`;
  }
  private convertToIST(isoDate: Date | string | null | undefined): string {
    if (!isoDate) {
      return '';
    }
    const utcDate = new Date(isoDate + "Z"); // Treat input as UTC
    return utcDate.toLocaleString("en-IN", {
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      year: "numeric",
      month: "short",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      hour12: true
    });
  }
}
