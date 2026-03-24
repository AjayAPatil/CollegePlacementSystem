import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CompanyService } from '../../services/company.service';
import { JobModel, ResponseModel } from '../../../../shared';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { GlobalService } from '../../../../core';

@Component({
  selector: 'app-company-jobs-modify',
  standalone: false,
  templateUrl: './company-jobs-modify.component.html',
  styleUrl: './company-jobs-modify.component.scss',
})
export class CompanyJobModifyComponent implements OnInit {

  activity: string = '';
  actionTitle: string = '';
  actionDescription: string = '';
  companyForm!: FormGroup;
  minExpiryData: Date = new Date();
  selectedJob: JobModel | null = null;


  @Input('activity')
  set setActivity(val: string) {
    this.activity = val;
    this.activityChanged();
  }
  @Input('selectedJob')
  set selectedJobData(val: JobModel | null) {
    this.selectedJob = val ? new JobModel(val) : null;
    this.patchJobForm();
  }
  @Output()
  activityChange = new EventEmitter<string>();


  constructor(
    private fb: FormBuilder,
    private companyService: CompanyService,
    private cdr: ChangeDetectorRef,
    private globalService: GlobalService
  ) {
    this.minExpiryData.setHours(0, 0, 0, 0);
    this.companyForm = this.fb.group({
      jobTitle: ['', [Validators.required, Validators.maxLength(200)]],
      department: ['', Validators.maxLength(100)],
      jobType: ['', Validators.required],
      workMode: ['', Validators.required],
      location: ['', [Validators.required, Validators.maxLength(150)]],
      experienceMin: [0, [Validators.required, Validators.min(0)]],
      experienceMax: [0, [Validators.required, Validators.min(0)]],
      salaryMin: [null, Validators.min(0)],
      salaryMax: [null, Validators.min(0)],
      openings: [1, [Validators.required, Validators.min(1)]],

      responsibilities: [''],
      requiredSkills: [''],
      preferredSkills: [''],
      qualifications: [''],
      benefits: [''],

      expiryDate: [''],
      status: ['draft']
    }, {
      validators: [
        this.rangeValidator('experienceMin', 'experienceMax', 'experienceRange'),
        this.rangeValidator('salaryMin', 'salaryMax', 'salaryRange')
      ]
    });
  }
  ngOnInit(): void {
  }

  getControl(controlName: string): AbstractControl | null {
    return this.companyForm.get(controlName);
  }

  private rangeValidator(minControlName: string, maxControlName: string, errorKey: string): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const minControl = group.get(minControlName);
      const maxControl = group.get(maxControlName);

      if (!minControl || !maxControl) return null;

      const minValue = minControl.value;
      const maxValue = maxControl.value;

      if (minValue === null || minValue === '' || maxValue === null || maxValue === '') {
        return null;
      }

      return Number(maxValue) < Number(minValue) ? { [errorKey]: true } : null;
    };
  }

  activityChanged() {
    switch (this.activity) {
      case 'add':
        this.actionTitle = 'Add New Job';
        this.actionDescription = 'Fill all required details!';
        break;
      case 'edit':
        this.actionTitle = 'Edit Job';
        this.actionDescription = 'Update required job details.';
        break;
    }
    this.cdr.detectChanges();
  }
  backToList() {
    this.clear();
    this.activityChange.emit('list');
  }

  clear() {
    this.companyForm.reset({
      jobTitle: '',
      department: '',
      jobType: '',
      workMode: '',
      location: '',
      experienceMin: 0,
      experienceMax: 0,
      salaryMin: null,
      salaryMax: null,
      openings: 1,
      responsibilities: '',
      requiredSkills: '',
      preferredSkills: '',
      qualifications: '',
      benefits: '',
      expiryDate: '',
      status: 'draft'
    });
  }

  private patchJobForm() {
    if (!this.selectedJob) {
      this.clear();
      return;
    }

    this.companyForm.patchValue({
      jobTitle: this.selectedJob.jobTitle,
      department: this.selectedJob.department ?? '',
      jobType: this.selectedJob.jobType,
      workMode: this.selectedJob.workMode,
      location: this.selectedJob.location,
      experienceMin: this.selectedJob.experienceMin,
      experienceMax: this.selectedJob.experienceMax,
      salaryMin: this.selectedJob.salaryMin ?? null,
      salaryMax: this.selectedJob.salaryMax ?? null,
      openings: this.selectedJob.openings,
      responsibilities: this.selectedJob.responsibilities ?? '',
      requiredSkills: this.selectedJob.requiredSkills ?? '',
      preferredSkills: this.selectedJob.preferredSkills ?? '',
      qualifications: this.selectedJob.qualifications ?? '',
      benefits: this.selectedJob.benefits ?? '',
      expiryDate: this.selectedJob.expiryDate ?? '',
      status: this.selectedJob.status ?? 'draft'
    });
  }

  save(status: string) {
    this.companyForm.patchValue({ status });
    this.companyForm.markAllAsTouched();
    if (this.companyForm.invalid) return;

    const companyId = this.globalService.userInfo.company?.id ?? 0;
    const createdBy = this.globalService.userInfo.userId;
    const jobData = {
      ...this.companyForm.getRawValue(),
      jobId: this.selectedJob?.jobId ?? 0,
      status,
      companyId,
      createdBy
    };

    const dataToSubmit = new FormData();
    dataToSubmit.append('data', JSON.stringify(jobData));

    const request$ = this.activity === 'edit' && this.selectedJob?.jobId
      ? this.companyService.updateJob(this.selectedJob.jobId, dataToSubmit)
      : this.companyService.saveJobs(dataToSubmit);

    request$.subscribe({
      next: (response: ResponseModel) => {
        if (response?.status == "0") {
          this.globalService.showSuccessMessage(response.message);
          this.backToList();
        } else {
          this.globalService.showErrorMessage(response.message);
        }
      },
      error: () => {
        this.globalService.showErrorMessage('Failed to save job details.');
      }
    });

  }
}
