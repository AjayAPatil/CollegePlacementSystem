import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StudentComponent } from './student.component';
import { StudentDashboardComponent } from './student-dashboard/student-dashboard.component';
import { StudentProfileComponent } from './student-profile/student-profile.component';
import { AuthGuard } from '../../core';
import { StudentJobsComponent } from './student-jobs/student-jobs.component';
import { StudentJobDetailsComponent } from './student-job-details/student-job-details.component';

const routes: Routes = [
  {
    path: '', component: StudentComponent,
    children: [

      {
        path: 'dashboard',
        component: StudentDashboardComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Student'] }
      },
      {
        path: 'profile',
        component: StudentProfileComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Student'] }
      },
      {
        path: 'jobs',
        component: StudentJobsComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Student'] }
      },
      {
        path: 'jobs/:jobId',
        component: StudentJobDetailsComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Student'] }
      }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StudentRoutingModule { }
