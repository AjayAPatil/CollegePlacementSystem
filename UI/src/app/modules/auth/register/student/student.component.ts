import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { CommonConstants, StudentModel, UserModel, UserRoleConstants, UserStatusConstants } from '../../../../shared';
import { Router } from '@angular/router';
import { GlobalService } from '../../../../core';

@Component({
  selector: 'app-student',
  standalone: false,
  templateUrl: './student.component.html',
  styleUrl: './student.component.scss',
})
export class StudentComponent implements OnInit {

  personalForm!: FormGroup;
  academicForm!: FormGroup;
  contactForm!: FormGroup;
  skillsForm!: FormGroup;
  resumeForm!: FormGroup;
  readonly separatorKeysCodes = [ENTER, COMMA] as const;
  photoFileName: string | null = null;
  resumeFileName: string | null = null;
  genderOptions = CommonConstants.GenderOptions;
  bloodGroupOptions = CommonConstants.BloodGroupOptions;

  constructor(private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private globalService: GlobalService
  ) { }

  ngOnInit(): void {

    this.personalForm = this.fb.group({
      firstName: ['', Validators.required],
      middleName: [''],
      lastName: ['', Validators.required],
      dob: ['', Validators.required],
      gender: ['', Validators.required],
      bloodGroup: ['', Validators.required]
    });

    this.academicForm = this.fb.group({
      enrollmentNo: ['', Validators.required],
      department: ['', Validators.required],
      passingYear: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]]
    });

    this.contactForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      address: ['', Validators.required],
      city: ['', Validators.required],
      district: ['', Validators.required],
      state: ['Maharashtra', Validators.required],
      postalCode: ['', [Validators.required, Validators.pattern('^[0-9]{6}$')]],
      country: ['India', Validators.required]
    });

    this.skillsForm = this.fb.group({
      skills: this.fb.array([], Validators.required),
      skillName: ['']
    });

    this.resumeForm = this.fb.group({
      resume: ['', Validators.required],
      photo: ['']
    });

  }

  get skills(): FormArray {
    return this.skillsForm.get('skills') as FormArray;
  }

  addSkill(skill: string) {
    if (skill && skill.trim() !== '') {
      this.skills.push(this.fb.control(skill));
    }
  }

  removeSkill(index: number) {
    this.skills.removeAt(index);
  }

  uploadPhoto(event: any) {

    const file = event.target.files[0];

    if (file) {
      this.photoFileName = file.name;
      this.resumeForm.patchValue({
        photo: file
      });

      this.resumeForm.get('photo')?.markAsTouched();
    }

  }


  uploadResume(event: any) {

    const file = event.target.files[0];

    if (file) {
      this.resumeFileName = file.name;

      this.resumeForm.patchValue({
        resume: file
      });

      this.resumeForm.get('resume')?.markAsTouched();
    }

  }

  getControl(form: FormGroup, name: string) {
    return form.get(name);
  }

  submit() {

    if (
      this.personalForm.invalid ||
      this.academicForm.invalid ||
      this.contactForm.invalid ||
      this.skillsForm.invalid ||
      this.resumeForm.invalid
    ) {
      return;
    }

    const data = {
      ...this.personalForm.value,
      ...this.academicForm.value,
      ...this.contactForm.value,
      skills: this.skills.value,
      resume: this.resumeForm.value.resume
    };
    console.log(JSON.stringify(data));
    console.log(data);

    const dataToSubmit: UserModel = {
      email: this.contactForm.value.email,
      mobileNo: this.contactForm.value.phone,
      streetAddress: this.contactForm.value.address,
      city: this.contactForm.value.city,
      district: this.contactForm.value.district,
      state: this.contactForm.value.state,
      pinCode: Number(this.contactForm.value.postalCode),
      country: this.contactForm.value.country,
      role: UserRoleConstants.Student,
      status: UserStatusConstants.Active,
      createdAt: new Date(),
      isDeleted: false,
      isLoggedIn: false,
      passwordHash: '',
      updatedAt: new Date(),
      userId: 0,
      userName: this.contactForm.value.email,
      company: null,
      student: null,
      profilePhoto: null
    };
    const student: StudentModel = {
      id: 0,
      userId: 0,
      firstName: this.personalForm.value.firstName,
      middleName: this.personalForm.value.middleName,
      lastName: this.personalForm.value.lastName,
      dateOfBirth: this.personalForm.value.dob,
      nationality: this.contactForm.value.country,
      gender: this.personalForm.value.gender,
      bloodGroup: this.personalForm.value.bloodGroup,
      enrollmentNo: this.academicForm.value.enrollmentNo,
      department: this.academicForm.value.department,
      passingYear: this.academicForm.value.passingYear,
      cgpa: 0,
      createdAt: new Date(),
      resume: null,
      resumeUrl: '',
      skills: this.skills.value.join(','),
      user: new UserModel()
    };
    dataToSubmit.student = student;


    const formData = new FormData();;
    formData.append('resume', this.resumeForm.value.resume);
    if (this.resumeForm.value.photo) {
      formData.append('photo', this.resumeForm.value.photo);
    }
    formData.append('data', JSON.stringify(dataToSubmit));

    this.authService.upsertUser(formData).subscribe({
      next: (response) => {
        console.log('User registered successfully', response);
        this.globalService.showMessage.emit({text: 'User registered successfully', type: 'success'})
        this.router.navigate(['/login']);
      },
      error: (error) => {
        console.error('Error registering user', error);
      }
    });
  }
}