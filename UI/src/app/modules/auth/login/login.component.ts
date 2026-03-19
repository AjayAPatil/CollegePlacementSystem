import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Form, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { GlobalService, UserModel, UserRoleConstants } from '../../index';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent implements OnInit {

  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private globalService: GlobalService,
    private cdref: ChangeDetectorRef
  ) {
    this.loginForm = this.fb.group({
      //email: ['ajaypatil9765@gmail.com', [Validators.required]],
      //email: ['Admin', [Validators.required]],
      email: ['mic@ms.com', [Validators.required]],
      passwordHash: ['Pass@123', Validators.required]
    });
  }

  ngOnInit(): void {
    this.globalService.clearUserInfo(false); // Clear any existing user info on login page load
    this.cdref.detectChanges();
  }

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.authService.login(this.loginForm.value)
      .subscribe({
        next: (response: any) => {
          if (response.status == '0') {
            this.globalService.showSuccessMessage(response.message);
            const userInfo = response.data as UserModel;
            userInfo.isLoggedIn = true;
            this.globalService.setUserInfo(userInfo);
            this.cdref.detectChanges();
            localStorage.setItem('token', response.token);
            localStorage.setItem('role', userInfo.role);
            this.navigateToDashboard(userInfo.role)
          } else {

            this.globalService.showErrorMessage(response.message);
          }
        }, error: () => {
          this.globalService.showErrorMessage('Error While Fetching API')
        }
      });
  }

  private navigateToDashboard(role: string) {
    if (role === UserRoleConstants.Admin) {
      this.router.navigate(['/admin/dashboard']);
    }
    else if (role === UserRoleConstants.Student) {
      this.router.navigate(['/student/dashboard']);
    }
    else if (role === UserRoleConstants.Company) {
      this.router.navigate(['/company/dashboard']);
    }
  }
}