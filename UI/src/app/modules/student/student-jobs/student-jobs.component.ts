import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { StudentService } from '../services/student.service';
import { JobFeedItem, PagedResult } from '../../../shared';

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

  constructor(private readonly studentService: StudentService,
    private cdref: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
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

  private loadJobs(): void {
    if (this.loading || !this.hasMore) {
      return;
    }

    this.loading = true;

    this.studentService.getJobs(this.page, this.pageSize).subscribe({
      next: (response) => {
        if (!this.isSuccessStatus(response.status)) {
          this.loading = false;
          this.initialLoading = false;
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

  private isSuccessStatus(status: string | number | undefined): boolean {
    return status === 0 || status === '0';
  }

  private normalizePageData(data: any): PagedResult<JobFeedItem> {
    return {
      items: data?.items ?? data?.Items ?? [],
      page: data?.page ?? data?.Page ?? this.page,
      pageSize: data?.pageSize ?? data?.PageSize ?? this.pageSize,
      totalCount: data?.totalCount ?? data?.TotalCount ?? 0,
      hasMore: data?.hasMore ?? data?.HasMore ?? false
    };
  }
}
