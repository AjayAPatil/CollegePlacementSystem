import { Component, OnInit } from '@angular/core';
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

  studentTblColumns: string[] = [
    'firstName',
    'lastName',
    'enrollmentNo',
    'department',
    'passingYear',
    'email',
    'phoneNumber',
    'selectedCompanyName',
  ];
  studentDataSource = new MatTableDataSource<StudentModel>();

  constructor(private adminService: AdminService) {
  }

  ngOnInit(): void {
    this.getStudentList();
  }

  private getStudentList() {
    this.adminService.getStudents().subscribe({
      next: (value: StudentModel[]) => {
        this.studentDataSource.data = value.map((student) => ({
          ...student,
          user: student.user
            ? {
                ...student.user,
                passwordHash: '',
              }
            : student.user,
        }));
      }
    })
  }
}
