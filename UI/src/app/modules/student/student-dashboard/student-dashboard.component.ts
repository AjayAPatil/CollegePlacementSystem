import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { GlobalService } from '../../../core';
import { isSuccessResponse, ResponseModel, StudentDashboardChartItem, StudentDashboardModel, StudentDashboardRecentApplication } from '../../../shared';
import { StudentService } from '../services/student.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: false,
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss',
})
export class StudentDashboardComponent implements OnInit {
  displayedColumns: string[] = ['company', 'role', 'status'];
  loading = true;

  stats = {
    profileCompletionPercentage: 0,
    appliedJobsCount: 0,
    upcomingDrivesCount: 0,
    shortlistedCount: 0
  };

  dataSource: StudentDashboardRecentApplication[] = [];

  pieChartData: ChartConfiguration<'pie'>['data'] = {
    labels: [],
    datasets: [{
      data: []
    }]
  };

  barChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [{
      label: 'Open Jobs',
      data: []
    }]
  };

  constructor(
    private readonly studentService: StudentService,
    private readonly globalService: GlobalService,
    private cdref: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const studentId = this.globalService.userInfo?.student?.id ?? 0;

    if (!studentId) {
      this.loading = false;
      this.globalService.showErrorMessage('Student information is missing.');
      return;
    }

    this.loadDashboard(studentId);
  }

  private loadDashboard(studentId: number): void {
    this.studentService.getDashboard(studentId).subscribe({
      next: (response: ResponseModel<StudentDashboardModel>) => {
        if (!isSuccessResponse(response)) {
          this.loading = false;
          this.globalService.showErrorMessage(response.message || 'Failed to load dashboard.');
          return;
        }

        const dashboard = this.normalizeDashboard(response.data);
        this.stats = {
          profileCompletionPercentage: dashboard.profileCompletionPercentage,
          appliedJobsCount: dashboard.appliedJobsCount,
          upcomingDrivesCount: dashboard.upcomingDrivesCount,
          shortlistedCount: dashboard.shortlistedCount
        };
        this.dataSource = dashboard.recentApplications;
        this.pieChartData = this.buildPieChart(dashboard.applicationStatusChart);
        this.barChartData = this.buildBarChart(dashboard.skillDemandChart);
        this.loading = false;
        this.cdref.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.globalService.showErrorMessage('Failed to load dashboard.');
      }
    });
  }

  private normalizeDashboard(data: any): StudentDashboardModel {
    return {
      profileCompletionPercentage: data?.profileCompletionPercentage ?? data?.ProfileCompletionPercentage ?? 0,
      appliedJobsCount: data?.appliedJobsCount ?? data?.AppliedJobsCount ?? 0,
      upcomingDrivesCount: data?.upcomingDrivesCount ?? data?.UpcomingDrivesCount ?? 0,
      shortlistedCount: data?.shortlistedCount ?? data?.ShortlistedCount ?? 0,
      recentApplications: (data?.recentApplications ?? data?.RecentApplications ?? []).map((item: any) => ({
        applicationId: item?.applicationId ?? item?.ApplicationId ?? 0,
        jobId: item?.jobId ?? item?.JobId ?? 0,
        company: item?.company ?? item?.Company ?? '',
        role: item?.role ?? item?.Role ?? '',
        status: this.formatStatus(item?.status ?? item?.Status ?? ''),
        appliedAt: item?.appliedAt ?? item?.AppliedAt ?? ''
      })),
      applicationStatusChart: this.normalizeChartItems(data?.applicationStatusChart ?? data?.ApplicationStatusChart ?? []),
      skillDemandChart: this.normalizeChartItems(data?.skillDemandChart ?? data?.SkillDemandChart ?? [])
    };
  }

  private normalizeChartItems(items: any[]): StudentDashboardChartItem[] {
    return (items ?? []).map((item: any) => ({
      label: item?.label ?? item?.Label ?? '',
      value: item?.value ?? item?.Value ?? 0
    }));
  }

  private buildPieChart(items: StudentDashboardChartItem[]): ChartConfiguration<'pie'>['data'] {
    return {
      labels: items.map((item) => item.label),
      datasets: [{
        data: items.map((item) => item.value)
      }]
    };
  }

  private buildBarChart(items: StudentDashboardChartItem[]): ChartConfiguration<'bar'>['data'] {
    return {
      labels: items.map((item) => item.label),
      datasets: [{
        label: 'Open Jobs',
        data: items.map((item) => item.value)
      }]
    };
  }

  private formatStatus(status: string): string {
    if (!status) {
      return 'Applied';
    }

    return status
      .split('_')
      .map((part) => part.charAt(0).toUpperCase() + part.slice(1).toLowerCase())
      .join(' ');
  }
}
