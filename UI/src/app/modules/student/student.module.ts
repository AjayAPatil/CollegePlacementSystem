import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentRoutingModule } from './student-routing.module';
import { StudentComponent } from './student.component';
import { StudentDashboardComponent } from './student-dashboard/student-dashboard.component';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { StudentProfileComponent } from './student-profile/student-profile.component';
import { SharedModule } from '../../shared';
import { StudentJobsComponent } from './student-jobs/student-jobs.component';

@NgModule({
  declarations: [
    StudentComponent,
    StudentDashboardComponent,
    StudentProfileComponent,
    StudentJobsComponent
  ],
  imports: [
    CommonModule,
    StudentRoutingModule,
    SharedModule
  ],
  providers: [
    provideCharts(withDefaultRegisterables())
  ],
})
export class StudentModule { }
