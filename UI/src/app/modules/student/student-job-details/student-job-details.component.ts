import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GlobalService } from '../../../core';
import { JobDetailModel, resolveAssetUrl } from '../../../shared';
import { StudentService } from '../services/student.service';

@Component({
  selector: 'app-student-job-details',
  standalone: false,
  templateUrl: './student-job-details.component.html',
  styleUrl: './student-job-details.component.scss',
})
export class StudentJobDetailsComponent implements OnInit {
  jobId = 0;
  jobDetails: JobDetailModel | null = null;
  loading = true;
  applying = false;
  hasApplied = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly studentService: StudentService,
    private readonly globalService: GlobalService,
    private cdref: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.jobId = Number(this.route.snapshot.paramMap.get('jobId') || 0);
    if (!this.jobId) {
      this.router.navigate(['/student/jobs']);
      return;
    }

    this.loadJobDetails();
  }

  goBack(): void {
    this.router.navigate(['/student/jobs']);
  }

  applyForJob(): void {
    const studentId = this.globalService.userInfo?.student?.id ?? 0;
    if (!studentId || this.applying || this.hasApplied) {
      if (!studentId) {
        this.globalService.showErrorMessage('Student profile is required before applying.');
      }
      return;
    }

    this.applying = true;
    this.studentService.applyForJob(this.jobId, { studentId }).subscribe({
      next: (response) => {
        this.applying = false;
        if (this.isSuccessStatus(response.status)) {
          this.hasApplied = true;
          this.globalService.showSuccessMessage(response.message);
          this.goBack();
          return;
        }

        this.globalService.showErrorMessage(response.message);
      },
      error: () => {
        this.applying = false;
        this.globalService.showErrorMessage('Failed to submit job application.');
      }
    });
  }

  get companyLogoUrl(): string {
    return resolveAssetUrl(this.jobDetails?.logoUrl);
  }

  private loadJobDetails(): void {
    this.loading = true;
    this.studentService.getJobDetails(this.jobId).subscribe({
      next: (response) => {
        this.loading = false;
        if (!this.isSuccessStatus(response.status)) {
          this.globalService.showErrorMessage(response.message);
          return;
        }

        this.jobDetails = this.normalizeJobDetails(response.data);
        this.cdref.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.globalService.showErrorMessage('Failed to load job details.');
      }
    });
  }

  private isSuccessStatus(status: string | number | undefined): boolean {
    return status === '0' || status === 0;
  }

  private normalizeJobDetails(data: any): JobDetailModel {
    return {
      jobId: data?.jobId ?? data?.JobId ?? 0,
      companyId: data?.companyId ?? data?.CompanyId ?? 0,
      jobTitle: data?.jobTitle ?? data?.JobTitle ?? '',
      companyName: data?.companyName ?? data?.CompanyName ?? '',
      logoUrl: data?.logoUrl ?? data?.LogoUrl,
      creatorName: data?.creatorName ?? data?.CreatorName ?? '',
      location: data?.location ?? data?.Location ?? '',
      qualifications: data?.qualifications ?? data?.Qualifications,
      jobType: data?.jobType ?? data?.JobType ?? '',
      workMode: data?.workMode ?? data?.WorkMode ?? '',
      experienceMin: data?.experienceMin ?? data?.ExperienceMin ?? 0,
      experienceMax: data?.experienceMax ?? data?.ExperienceMax ?? 0,
      salaryMin: data?.salaryMin ?? data?.SalaryMin,
      salaryMax: data?.salaryMax ?? data?.SalaryMax,
      openings: data?.openings ?? data?.Openings ?? 0,
      benefits: data?.benefits ?? data?.Benefits,
      responsibilities: data?.responsibilities ?? data?.Responsibilities,
      requiredSkills: data?.requiredSkills ?? data?.RequiredSkills,
      preferredSkills: data?.preferredSkills ?? data?.PreferredSkills,
      status: data?.status ?? data?.Status,
      createdAt: data?.createdAt ?? data?.CreatedAt ?? '',
      expiryDate: data?.expiryDate ?? data?.ExpiryDate,
      department: data?.department ?? data?.Department,
      companyWebsite: data?.companyWebsite ?? data?.CompanyWebsite,
      companyDescription: data?.companyDescription ?? data?.CompanyDescription,
      companyIndustry: data?.companyIndustry ?? data?.CompanyIndustry,
      companyLocation: data?.companyLocation ?? data?.CompanyLocation,
      companyHrName: data?.companyHrName ?? data?.CompanyHrName,
      companyContactEmail: data?.companyContactEmail ?? data?.CompanyContactEmail,
      companyContactPhone: data?.companyContactPhone ?? data?.CompanyContactPhone,
      companyFoundedYear: data?.companyFoundedYear ?? data?.CompanyFoundedYear,
      companySize: data?.companySize ?? data?.CompanySize
    };
  }
}
