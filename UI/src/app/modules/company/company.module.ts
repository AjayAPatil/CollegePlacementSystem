import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CompanyRoutingModule } from './company-routing.module';
import { CompanyComponent } from './company.component';
import { CompanyDashboardComponent } from './company-dashboard/company-dashboard.component';
import { SharedModule } from '../../shared';
import { CompanyStudentsComponent } from './company-students/company-students.component';
import { CompanyJobsComponent } from './company-jobs/company-jobs.component';
import { CompanyJobModifyComponent } from './company-jobs/company-jobs-modify/company-jobs-modify.component';
import { CompanyStudentDetailsComponent } from './company-students/company-student-details/company-student-details.component';

@NgModule({
  declarations: [
    CompanyComponent,
    CompanyDashboardComponent,
    CompanyStudentsComponent,
    CompanyStudentDetailsComponent,
    CompanyJobsComponent,
    CompanyJobModifyComponent
  ],
  imports: [
    CommonModule,
    CompanyRoutingModule,
    SharedModule
  ],
})
export class CompanyModule { }
