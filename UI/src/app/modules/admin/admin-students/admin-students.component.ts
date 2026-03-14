import { AfterViewInit, Component, OnInit } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { AdminService } from '../services/admin.service';
import { StudentModel } from '../../../shared';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-admin-students',
  standalone: false,
  templateUrl: './admin-students.component.html',
  styleUrl: './admin-students.component.scss',
})
export class AdminStudentsComponent implements OnInit {

  studentTblColumns: string[] = ['firstName', 'lastName', 'userName', 'passwordHash'];
  studentDataSource = new MatTableDataSource<StudentModel>();

  constructor(private adminService: AdminService) {
  }

  ngOnInit(): void {
    this.getStudentList();
  }

  private getStudentList() {
    this.adminService.getStudents().subscribe({
      next: (value: StudentModel[]) => {
        console.log(value);
        this.studentDataSource.data = value;
      }
    })
  }
}
