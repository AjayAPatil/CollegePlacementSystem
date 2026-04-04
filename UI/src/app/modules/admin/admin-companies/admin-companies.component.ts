import { Component, OnInit } from '@angular/core';
import { AdminService } from '../services/admin.service';
import { CompanyModel, isSuccessResponse, resolveAssetUrl, ResponseModel } from '../../../shared';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-admin-companies',
  standalone: false,
  templateUrl: './admin-companies.component.html',
  styleUrl: './admin-companies.component.scss',
})
export class AdminCompaniesComponent implements OnInit {

  studentTblColumns: string[] = ['logo', 'companyName', 'contactEmail' ,'contactPhone', 'hrName'];
  studentDataSource = new MatTableDataSource<CompanyModel>();
  activity: string = 'list'; //list, add, edit, view

  constructor(private adminService: AdminService) {
  }
  ngOnInit(): void {
    this.getCompanyList();
  }

  getCompanyLogo(company: CompanyModel): string {
    return resolveAssetUrl(company.logoUrl);
  }

  public getCompanyList() {
    this.adminService.getCompanies().subscribe({
      next: (response: ResponseModel<CompanyModel[]>) => {
        if (!isSuccessResponse(response)) {
          this.studentDataSource.data = [];
          return;
        }

        this.studentDataSource.data = response.data ?? [];
      }
    })
  }
  public addNew() {
    this.activity = 'add';
  }
}
