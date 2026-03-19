import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CompanyComponent } from './company.component';
import { CompanyDashboardComponent } from './company-dashboard/company-dashboard.component';
import { CompanyStudentsComponent } from './company-students/company-students.component';
import { CompanyJobsComponent } from './company-jobs/company-jobs.component';

const routes: Routes = [
  {
    path: '', component: CompanyComponent,
    children: [
      { path: 'dashboard', component: CompanyDashboardComponent },
      { path: 'students', component: CompanyStudentsComponent },
      { path: 'jobs', component: CompanyJobsComponent },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CompanyRoutingModule { }
