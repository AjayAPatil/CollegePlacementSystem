import { Component, OnInit } from '@angular/core';
import { CompanyService } from '../services/company.service';
import { isSuccessResponse, JobModel, ResponseModel } from '../../../shared';
import { MatTableDataSource } from '@angular/material/table';
import { GlobalService } from '../../../core';

@Component({
  selector: 'app-company-jobs',
  standalone: false,
  templateUrl: './company-jobs.component.html',
  styleUrl: './company-jobs.component.scss',
})
export class CompanyJobsComponent implements OnInit {

  jobTblColumns: string[] = ['action', 'jobTitle', 'jobType', 'workMode', 'location', 'experience', 'openings', 'status', 'expiryDate'];
  jobDataSource = new MatTableDataSource<JobModel>();
  activity: string = 'list'; //list, add, edit, view
  selectedJob: JobModel | null = null;

  constructor(
    private companyService: CompanyService,
    private globalService: GlobalService
  ) {
  }

  ngOnInit(): void {
    this.getJobList();
  }

  public getJobList() {
    this.companyService.getJobs().subscribe({
      next: (response: ResponseModel<JobModel[]>) => {
        if (!isSuccessResponse(response)) {
          this.jobDataSource.data = [];
          this.globalService.showErrorMessage(response.message || 'Failed to load jobs.');
          return;
        }

        this.jobDataSource.data = response.data ?? [];
      }
    })
  }
  public addNew() {
    this.selectedJob = null;
    this.activity = 'add';
  }

  public editJob(job: JobModel) {
    this.selectedJob = new JobModel(job);
    this.activity = 'edit';
  }

  public publishJob(job: JobModel) {
    this.updateJobStatus(job.jobId, 'published', 'Job published successfully.');
  }

  public closeJob(job: JobModel) {
    this.updateJobStatus(job.jobId, 'closed', 'Job closed successfully.');
  }

  public deleteJob(job: JobModel) {
    const shouldDelete = window.confirm(`Delete job "${job.jobTitle}"?`);
    if (!shouldDelete) return;

    this.companyService.deleteJob(job.jobId).subscribe({
      next: (response: ResponseModel) => {
        if (isSuccessResponse(response)) {
          this.globalService.showSuccessMessage(response.message);
          this.getJobList();
          return;
        }

        this.globalService.showErrorMessage(response.message);
      },
      error: () => {
        this.globalService.showErrorMessage('Failed to delete job.');
      }
    });
  }

  private updateJobStatus(jobId: number, status: string, successMessage: string) {
    this.companyService.updateJobStatus(jobId, status).subscribe({
      next: (response: ResponseModel) => {
        if (isSuccessResponse(response)) {
          this.globalService.showSuccessMessage(response.message || successMessage);
          this.getJobList();
          return;
        }

        this.globalService.showErrorMessage(response.message);
      },
      error: () => {
        this.globalService.showErrorMessage('Failed to update job status.');
      }
    });
  }
}
