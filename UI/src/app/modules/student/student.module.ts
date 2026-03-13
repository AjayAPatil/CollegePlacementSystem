import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentRoutingModule } from './student-routing.module';
import { StudentComponent } from './student.component';
import { StudentDashboardComponent } from './student-dashboard/student-dashboard.component';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { BaseChartDirective, provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { StudentProfileComponent } from './student-profile/student-profile.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@NgModule({
  declarations: [
    StudentComponent, 
    StudentDashboardComponent, 
    StudentProfileComponent
  ],
  imports: [
    CommonModule,
    StudentRoutingModule,
    MatCardModule,
    MatTableModule,
    BaseChartDirective,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatChipsModule,
    ReactiveFormsModule,
    MatIconModule
  ],
  providers: [
    provideCharts(withDefaultRegisterables())
  ],
})
export class StudentModule { }
