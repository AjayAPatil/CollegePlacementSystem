import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CompanyRoutingModule } from './company-routing.module';
import { CompanyComponent } from './company.component';
import { CompanyDashboardComponent } from './company-dashboard/company-dashboard.component';
import { SharedModule } from '../../shared';
import { CompanyStudentsComponent } from './company-students/company-students.component';
import { CompanyJobsComponent } from './company-jobs/company-jobs.component';

@NgModule({
  declarations: [
    CompanyComponent,
    CompanyDashboardComponent,
    CompanyStudentsComponent,
    CompanyJobsComponent
  ],
  imports: [
    CommonModule,
    CompanyRoutingModule,
    SharedModule
  ],
})
export class CompanyModule { }
