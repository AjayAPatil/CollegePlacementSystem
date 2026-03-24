import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CompanyComponent } from './company.component';
import { CompanyDashboardComponent } from './company-dashboard/company-dashboard.component';
import { CompanyStudentsComponent } from './company-students/company-students.component';
import { CompanyJobsComponent } from './company-jobs/company-jobs.component';
import { AuthGuard } from '../../core';

const routes: Routes = [
  {
    path: '', component: CompanyComponent,
    children: [
      {
        path: 'dashboard',
        component: CompanyDashboardComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Company'] }
      },
      {
        path: 'students', component: CompanyStudentsComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Company'] }
      },
      {
        path: 'jobs', component: CompanyJobsComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Company'] }
      },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CompanyRoutingModule { }
