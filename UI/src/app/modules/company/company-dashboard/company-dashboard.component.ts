import { Component } from '@angular/core';
import { ChartConfiguration } from 'chart.js';

@Component({
  selector: 'app-company-dashboard',
  standalone: false,
  templateUrl: './company-dashboard.component.html',
  styleUrl: './company-dashboard.component.scss',
})
export class CompanyDashboardComponent {

  displayedColumns: string[] = ['company', 'role', 'status'];

dataSource = [
  { company: 'TCS', role: 'Software Engineer', status: 'Shortlisted' },
  { company: 'Infosys', role: 'Developer', status: 'Applied' },
  { company: 'Wipro', role: 'Backend Developer', status: 'Rejected' }
];
pieChartData: ChartConfiguration<'pie'>['data'] = {
  labels: ['Frontend', 'Backend', 'Database', 'DevOps', 'Soft Skills'],
  datasets: [{
    data: [30, 25, 20, 10, 15]
  }]
};

barChartData: ChartConfiguration<'bar'>['data'] = {
  labels: ['Angular', '.NET', 'SQL', 'Docker', 'Communication'],
  datasets: [{
    label: 'Skill Level (%)',
    data: [85, 75, 80, 60, 70]
  }]
};
}
