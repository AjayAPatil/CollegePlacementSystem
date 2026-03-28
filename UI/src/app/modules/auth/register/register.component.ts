
import { AfterViewInit, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RegisterTypeDialog } from './register-type.dialog';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent implements OnInit, AfterViewInit {

  registerType: 'student' | 'company' | null = 'student';

  studentForm!: FormGroup;
  companyForm!: FormGroup;

  constructor(
    private dialog: MatDialog, 
    private fb: FormBuilder,
    private cdref: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
      this.initializeForms();
  }

  ngAfterViewInit() {
    // const dialogRef = this.dialog.open(RegisterTypeDialog, {
    //   disableClose: true,
    //   width: '400px'
    // });

    // dialogRef.afterClosed().subscribe(result => {
    //   this.registerType = result;
    //   this.cdref.detectChanges();
    // });
  }

  initializeForms() {
    this.studentForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      password: ['', Validators.required],
      skills: ['']
    });

    this.companyForm = this.fb.group({
      companyName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      password: ['', Validators.required],
      website: [''],
      address: ['']
    });
  }

  submitStudent() {
    if (this.studentForm.invalid) return;
    console.log('Student Data:', this.studentForm.value);
  }

  submitCompany() {
    if (this.companyForm.invalid) return;
    console.log('Company Data:', this.companyForm.value);
  }
}