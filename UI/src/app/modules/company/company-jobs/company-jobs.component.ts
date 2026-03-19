import { AfterViewInit, Component, OnInit } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { CompanyService } from '../services/company.service';
import { StudentModel } from '../../../shared';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-company-jobs',
  standalone: false,
  templateUrl: './company-jobs.component.html',
  styleUrl: './company-jobs.component.scss',
})
export class CompanyJobsComponent implements OnInit {

  studentTblColumns: string[] = ['firstName', 'lastName', 'userName', 'passwordHash'];
  studentDataSource = new MatTableDataSource<StudentModel>();

  constructor(private companyService: CompanyService) {
  }

  ngOnInit(): void {
    this.getStudentList();
  }

  private getStudentList() {
    this.companyService.getStudents().subscribe({
      next: (value: StudentModel[]) => {
        console.log(value);
        this.studentDataSource.data = value;
      }
    })
  }
}
