import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminComponent } from './admin.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { AdminStudentsComponent } from './admin-students/admin-students.component';
import { AdminCompaniesComponent } from './admin-companies/admin-companies.component';
import { AuthGuard } from '../../core';

const routes: Routes = [
  {
    path: '', component: AdminComponent,
    children: [
      {
        path: 'dashboard',
        component: AdminDashboardComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Admin'] }
      },
      {
        path: 'students',
        component: AdminStudentsComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Admin'] }
      },
      {
        path: 'companies',
        component: AdminCompaniesComponent,
        canActivate: [AuthGuard],
        data: { roles: ['Admin'] }
      },
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule { }
