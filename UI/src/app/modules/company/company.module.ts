import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CompanyRoutingModule } from './company-routing.module';
import { CompanyComponent } from './company.component';
import { CompanyDashboardComponent } from './company-dashboard/company-dashboard.component';
import { DATE_FORMATS, SharedModule } from '../../shared';
import { CompanyStudentsComponent } from './company-students/company-students.component';
import { CompanyJobsComponent } from './company-jobs/company-jobs.component';
import { CompanyJobModifyComponent } from './company-jobs/company-jobs-modify/company-jobs-modify.component';
import { CompanyStudentDetailsComponent } from './company-students/company-student-details/company-student-details.component';
import { MomentDateAdapter, MAT_MOMENT_DATE_ADAPTER_OPTIONS } from '@angular/material-moment-adapter';
import { DateAdapter, MAT_DATE_FORMATS } from '@angular/material/core';

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
  providers: [
  { provide: DateAdapter, useClass: MomentDateAdapter },
  { provide: MAT_DATE_FORMATS, useValue: DATE_FORMATS },
  { provide: MAT_MOMENT_DATE_ADAPTER_OPTIONS, useValue: { useUtc: false } }
]
})
export class CompanyModule { }
