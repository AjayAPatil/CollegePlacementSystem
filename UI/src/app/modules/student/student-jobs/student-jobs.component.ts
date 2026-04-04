import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { GlobalService } from '../../../core';
import { StudentService } from '../services/student.service';
import { isSuccessResponse, JobFeedItem, PagedResult, resolveAssetUrl, ResponseModel } from '../../../shared';

@Component({
  standalone: false,
  selector: 'app-student-jobs',
  templateUrl: './student-jobs.component.html',
  styleUrl: './student-jobs.component.scss',
})
export class StudentJobsComponent implements OnInit {
  jobs: JobFeedItem[] = [];
  page = 1;
  readonly pageSize = 10;
  loading = false;
  initialLoading = true;
  hasMore = true;
  totalCount = 0;
  studentId = 0;

  constructor(private readonly studentService: StudentService,
    private readonly globalService: GlobalService,
    private cdref: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.studentId = this.globalService.userInfo?.student?.id ?? 0;
    this.loadJobs();
  }

  onFeedScroll(event: Event): void {
    const element = event.target as HTMLElement;
    const threshold = 320;
    const remaining = element.scrollHeight - element.scrollTop - element.clientHeight;

    if (remaining <= threshold) {
      this.loadJobs();
    }
  }

  trackByJobId(_index: number, job: JobFeedItem): number {
    return job.jobId;
  }

  getCompanyLogo(job: JobFeedItem): string {
    return resolveAssetUrl(job.logoUrl);
  }

  private loadJobs(): void {
    if (this.loading || !this.hasMore) {
      return;
    }

    this.loading = true;

    this.studentService.getJobs(this.page, this.pageSize, this.studentId).subscribe({
      next: (response: ResponseModel<PagedResult<JobFeedItem>>) => {
        if (!isSuccessResponse(response)) {
          this.loading = false;
          this.initialLoading = false;
          this.globalService.showErrorMessage(response.message || 'Failed to load jobs.');
          return;
        }

        const pageData = this.normalizePageData(response.data);
        const items = pageData.items ?? [];

        this.jobs = [...this.jobs, ...items];
        this.totalCount = pageData?.totalCount ?? this.totalCount;
        this.hasMore = pageData?.hasMore ?? false;
        this.page += 1;
        this.loading = false;
        this.initialLoading = false;
        this.cdref.detectChanges()
      },
      error: () => {
        this.loading = false;
        this.initialLoading = false;
      }
    });
  }

  private normalizePageData(data: any): PagedResult<JobFeedItem> {
    return {
      items: (data?.items ?? data?.Items ?? []).map((item: any) => ({
        ...item,
        isApplied: item?.isApplied ?? item?.IsApplied ?? false
      })),
      page: data?.page ?? data?.Page ?? this.page,
      pageSize: data?.pageSize ?? data?.PageSize ?? this.pageSize,
      totalCount: data?.totalCount ?? data?.TotalCount ?? 0,
      hasMore: data?.hasMore ?? data?.HasMore ?? false
    };
  }
}
