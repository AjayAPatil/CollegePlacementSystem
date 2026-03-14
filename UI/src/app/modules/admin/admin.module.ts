import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdminRoutingModule } from './admin-routing.module';
import { AdminComponent } from './admin.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { SharedModule } from '../../shared';
import { AdminStudentsComponent } from './admin-students/admin-students.component';
import { AdminCompaniesComponent } from './admin-companies/admin-companies.component';
import { AdminCompaniesModifyComponent } from './admin-companies/admin-companies-modify/admin-companies-modify.component';

@NgModule({
  declarations: [
    AdminComponent,
    AdminDashboardComponent,
    AdminStudentsComponent,
    AdminCompaniesComponent,
    AdminCompaniesModifyComponent
  ],
  imports: [
    CommonModule,
    AdminRoutingModule,
    SharedModule
  ],
})
export class AdminModule { }
