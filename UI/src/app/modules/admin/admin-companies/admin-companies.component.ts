import { Component, OnInit } from '@angular/core';
import { AdminService } from '../services/admin.service';
import { CompanyModel } from '../../../shared';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-admin-companies',
  standalone: false,
  templateUrl: './admin-companies.component.html',
  styleUrl: './admin-companies.component.scss',
})
export class AdminCompaniesComponent implements OnInit {

  studentTblColumns: string[] = ['companyName'];
  studentDataSource = new MatTableDataSource<CompanyModel>();
  activity: string = 'list'; //list, add, edit, view

  constructor(private adminService: AdminService) {
  }
  ngOnInit(): void {
    this.getCompanyList();
  }

  public getCompanyList() {
    this.adminService.getCompanies().subscribe({
      next: (value: CompanyModel[]) => {
        console.log(value);
        this.studentDataSource.data = value;
      }
    })
  }
  public addNew() {
    this.activity = 'add';
  }
}
