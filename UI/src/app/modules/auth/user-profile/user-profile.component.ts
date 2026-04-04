import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { GlobalService } from '../../../core';
import { CompanyModel, isSuccessResponse, resolveAssetUrl, ResponseModel, StudentModel, UserModel } from '../../../shared';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-user-profile',
  standalone: false,
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.scss',
})
export class UserProfileComponent implements OnInit {
  profileForm: FormGroup;
  passwordForm: FormGroup;
  userInfo: UserModel;
  isLoading = false;
  isSavingProfile = false;
  isSavingPassword = false;
  passwordErrorMessage = '';
  passwordSuccessMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private globalService: GlobalService,
    private cdref: ChangeDetectorRef
  ) {
    this.userInfo = this.globalService.userInfo;
    this.profileForm = this.fb.group({
      userName: [{ value: '', disabled: true }],
      email: [{ value: '', disabled: true }, [Validators.required, Validators.email]],
      mobileNo: [''],
      streetAddress: [''],
      city: [''],
      district: [''],
      state: [''],
      country: [''],
      pinCode: [''],
      student: this.fb.group({
        firstName: ['', Validators.required],
        middleName: [''],
        lastName: ['', Validators.required],
        dateOfBirth: [''],
        nationality: [''],
        gender: [''],
        bloodGroup: [''],
        enrollmentNo: [''],
        department: [''],
        passingYear: [''],
        cgpa: [''],
        skills: [''],
      }),
      company: this.fb.group({
        companyName: ['', Validators.required],
        website: [''],
        description: [''],
        industry: [''],
        location: [''],
        hrName: [''],
        contactPhone: [''],
        foundedYear: [''],
        companySize: [''],
      })
    });

    this.passwordForm = this.fb.group({
      oldPassword: ['', Validators.required],
      newPassword: ['', Validators.required],
      verifyNewPassword: ['', Validators.required],
    }, { validators: this.passwordMatchValidator() });
  }

  ngOnInit(): void {
    this.configureRoleForms();
    this.loadProfile();
  }

  get isStudent(): boolean {
    return this.userInfo.role === 'Student';
  }

  get isCompany(): boolean {
    return this.userInfo.role === 'Company';
  }

  get passwordMismatch(): boolean {
    return this.passwordForm.hasError('passwordMismatch')
      && !!this.passwordForm.get('verifyNewPassword')?.touched;
  }

  get oldPasswordControl() {
    return this.passwordForm.get('oldPassword');
  }

  get newPasswordControl() {
    return this.passwordForm.get('newPassword');
  }

  get verifyNewPasswordControl() {
    return this.passwordForm.get('verifyNewPassword');
  }

  get profileImageUrl(): string {
    return resolveAssetUrl(this.userInfo.profileImagePath);
  }

  loadProfile() {
    if (!this.userInfo.userId) {
      this.globalService.showErrorMessage('User information not found.');
      return;
    }

    this.isLoading = true;
    this.authService.getProfile(this.userInfo.userId).subscribe({
      next: (response: ResponseModel<UserModel>) => {
        this.isLoading = false;
        if (!this.isSuccess(response)) {
          this.globalService.showErrorMessage(response?.message || 'Unable to fetch profile.');
          return;
        }

        const profile = response.data as UserModel;
        profile.profileImagePath = profile.profileImagePath ?? (profile as any).ProfileImagePath;
        this.userInfo = profile;
        this.patchProfile(profile);
      },
      error: () => {
        this.isLoading = false;
        this.globalService.showErrorMessage('Error while fetching profile.');
      }
    });
  }

  saveProfile() {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const updatedUser = this.buildProfilePayload();
    this.isSavingProfile = true;

    this.authService.updateProfile(updatedUser).subscribe({
      next: (response: ResponseModel<UserModel>) => {
        this.isSavingProfile = false;
        if (!this.isSuccess(response)) {
          this.globalService.showErrorMessage(response?.message || 'Unable to update profile.');
          return;
        }

        const savedUser = response.data as UserModel;
        savedUser.profileImagePath = savedUser.profileImagePath ?? (savedUser as any).ProfileImagePath;
        savedUser.isLoggedIn = true;
        this.globalService.showSuccessMessage(response.message);
        this.globalService.setUserInfo(savedUser);
      },
      error: () => {
        this.isSavingProfile = false;
        this.globalService.showErrorMessage('Error while updating profile.');
      }
    });
  }

  updatePassword() {
    this.passwordErrorMessage = '';
    this.passwordSuccessMessage = '';

    this.passwordForm.markAllAsTouched();
    this.cdref.detectChanges();
    if (this.passwordForm.invalid) {
      return;
    }

    this.isSavingPassword = true;
    this.authService.changePassword({
      userId: this.userInfo.userId,
      oldPassword: this.passwordForm.get('oldPassword')?.value,
      newPassword: this.passwordForm.get('newPassword')?.value,
      verifyNewPassword: this.passwordForm.get('verifyNewPassword')?.value,
    }).subscribe({
      next: (response: ResponseModel) => {
        this.isSavingPassword = false;
        if (!this.isSuccess(response)) {
          this.passwordErrorMessage = response?.message || 'Unable to update password.';
          this.globalService.showErrorMessage(response?.message || 'Unable to update password.');
          return;
        }

        this.passwordSuccessMessage = response.message;
        this.globalService.showSuccessMessage(response.message);
        this.passwordForm.reset();
        this.cdref.detectChanges();
      },
      error: () => {
        this.isSavingPassword = false;
        this.passwordErrorMessage = 'Error while updating password.';
        this.globalService.showErrorMessage('Error while updating password.');
      }
    });
  }

  private patchProfile(user: UserModel) {
    this.configureRoleForms();
    this.profileForm.patchValue({
      userName: user.userName,
      email: user.email,
      mobileNo: user.mobileNo,
      streetAddress: user.streetAddress,
      city: user.city,
      district: user.district,
      state: user.state,
      country: user.country,
      pinCode: user.pinCode || '',
      student: {
        firstName: user.student?.firstName || '',
        middleName: user.student?.middleName || '',
        lastName: user.student?.lastName || '',
        dateOfBirth: this.toDateInputValue(user.student?.dateOfBirth),
        nationality: user.student?.nationality || '',
        gender: user.student?.gender || '',
        bloodGroup: user.student?.bloodGroup || '',
        enrollmentNo: user.student?.enrollmentNo || '',
        department: user.student?.department || '',
        passingYear: user.student?.passingYear || '',
        cgpa: user.student?.cgpa || '',
        skills: user.student?.skills || '',
      },
      company: {
        companyName: user.company?.companyName || '',
        website: user.company?.website || '',
        description: user.company?.description || '',
        industry: user.company?.industry || '',
        location: user.company?.location || '',
        hrName: user.company?.hrName || '',
        contactPhone: user.company?.contactPhone || '',
        foundedYear: user.company?.foundedYear || '',
        companySize: user.company?.companySize || '',
      }
    });
    this.cdref.detectChanges();
  }

  private configureRoleForms() {
    const studentGroup = this.profileForm.get('student');
    const companyGroup = this.profileForm.get('company');

    if (this.isStudent) {
      studentGroup?.enable({ emitEvent: false });
      companyGroup?.disable({ emitEvent: false });
      return;
    }

    if (this.isCompany) {
      companyGroup?.enable({ emitEvent: false });
      studentGroup?.disable({ emitEvent: false });
      return;
    }

    studentGroup?.disable({ emitEvent: false });
    companyGroup?.disable({ emitEvent: false });
  }

  private buildProfilePayload(): UserModel {
    const rawValue = this.profileForm.getRawValue();
    const payload = { ...this.userInfo } as UserModel;

    payload.mobileNo = rawValue.mobileNo || '';
    payload.streetAddress = rawValue.streetAddress || '';
    payload.city = rawValue.city || '';
    payload.district = rawValue.district || '';
    payload.state = rawValue.state || '';
    payload.country = rawValue.country || '';
    payload.pinCode = rawValue.pinCode ? Number(rawValue.pinCode) : 0;

    if (this.isStudent) {
      payload.student = {
        ...(payload.student || new StudentModel()),
        ...rawValue.student,
        passingYear: rawValue.student?.passingYear ? Number(rawValue.student.passingYear) : 0,
        cgpa: rawValue.student?.cgpa ? Number(rawValue.student.cgpa) : 0,
        dateOfBirth: rawValue.student?.dateOfBirth || null,
      };
    }

    if (this.isCompany) {
      payload.company = {
        ...(payload.company || new CompanyModel()),
        ...rawValue.company,
        foundedYear: rawValue.company?.foundedYear ? Number(rawValue.company.foundedYear) : null,
        companySize: rawValue.company?.companySize ? Number(rawValue.company.companySize) : null,
      };
    }

    return payload;
  }

  private passwordMatchValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const newPassword = control.get('newPassword')?.value;
      const verifyNewPassword = control.get('verifyNewPassword')?.value;

      if (!newPassword || !verifyNewPassword) {
        return null;
      }

      return newPassword === verifyNewPassword ? null : { passwordMismatch: true };
    };
  }

  private toDateInputValue(value?: Date): string {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '';
    }

    return date.toISOString().split('T')[0];
  }

  private isSuccess(response: ResponseModel | null | undefined): boolean {
    return isSuccessResponse(response);
  }
}
