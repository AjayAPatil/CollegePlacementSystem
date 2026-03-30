import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AdminService } from '../../services/admin.service';
import { CompanyModel, isSuccessResponse, ResponseModel, UserModel } from '../../../../shared';
import { MatTableDataSource } from '@angular/material/table';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GlobalService } from '../../../../core';

@Component({
  selector: 'app-admin-companies-modify',
  standalone: false,
  templateUrl: './admin-companies-modify.component.html',
  styleUrl: './admin-companies-modify.component.scss',
})
export class AdminCompaniesModifyComponent implements OnInit {

  activity: string = '';
  actionTitle: string = '';
  actionDescription: string = '';
  logoFileName: string = '';
  companyForm!: FormGroup;


  @Input('activity')
  set setActivity(val: string) {
    this.activity = val;
    this.activityChanged();
  }
  @Output()
  activityChange = new EventEmitter<string>();


  constructor(
    private fb: FormBuilder,
    private adminService: AdminService,
    private cdr: ChangeDetectorRef,
    private globalService: GlobalService
  ) {
    this.companyForm = this.fb.group({
      companyName: ['', Validators.required],
      website: [''],
      industry: [''],
      location: [''],
      hrName: [''],
      contactEmail: ['', Validators.required],
      contactPhone: ['', Validators.required],
      foundedYear: ['', Validators.maxLength(4)],
      companySize: [''],
      description: [''],
      logo: ['']
    });
  }
  ngOnInit(): void {
  }

  activityChanged() {
    switch (this.activity) {
      case 'add':
        this.actionTitle = 'Add New Company';
        this.actionDescription = 'Fill all required details!';
        break;
    }
    this.cdr.detectChanges();
  }
  backToList() {
    this.clear();
    this.activityChange.emit('list');
  }
  uploadLogo(event: any) {
    const file = event.target.files[0];

    this.logoFileName = file.name;
    this.companyForm.patchValue({
      logo: file
    });
  }

  clear() {
    this.companyForm.reset();
  }

  save() {
    this.companyForm.markAllAsTouched();
    if (this.companyForm.invalid) return;

    const companyData = new CompanyModel();
    companyData.companyName = this.companyForm.value.companyName;
    companyData.website = this.companyForm.value.website;
    companyData.industry = this.companyForm.value.industry;
    companyData.location = this.companyForm.value.location;
    companyData.hrName = this.companyForm.value.hrName;
    companyData.contactEmail = this.companyForm.value.contactEmail;
    companyData.contactPhone = this.companyForm.value.contactPhone;
    companyData.foundedYear = this.companyForm.value.foundedYear;
    companyData.companySize = this.companyForm.value.companySize;
    companyData.description = this.companyForm.value.description;

    const dataToSubmit = new FormData();
    dataToSubmit.append('companyLogo', this.companyForm.value.logo);
    dataToSubmit.append('data', JSON.stringify(companyData));

    this.adminService.saveCompany(dataToSubmit).subscribe({
      next: (response: ResponseModel) => {
        if (isSuccessResponse(response)) {
          this.globalService.showSuccessMessage(response.message);
          this.backToList()
        } else {
          this.globalService.showErrorMessage(response.message);
        }
      }
    })

  }
}
