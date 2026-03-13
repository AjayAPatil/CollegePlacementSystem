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
      email: ['admin@admin.com', [Validators.required, Validators.email]],
      password: ['A@123.aaa', Validators.required]
    });
  }

  ngOnInit(): void {
    this.globalService.clearUserInfo(false); // Clear any existing user info on login page load
    this.cdref.detectChanges();
  }

  onSubmit() {
    if (this.loginForm.invalid) return;
    
    //   this.authService.login(this.loginForm.value)
    //     .subscribe(response => {
    let response = new UserModel();
    
      response.isLoggedIn = true;
      response.userId = 1;
      response.userName = this.loginForm.value.email;
      response.email = this.loginForm.value.email;
      response.passwordHash = this.loginForm.value.password;
      response.role = UserRoleConstants.Admin;
      response.status = 'Active';
      //response.firstName = 'Student';
      //response.lastName = 'One';
      //response.bloodGroup = 'A+';
      response.city = 'Mumbai';
      response.state = 'Maharashtra';
      response.country = 'India';
      response.pinCode = 400001;

    this.globalService.setUserInfo(response);
    this.globalService.showMessage.emit({text: 'logged in', type:'success'})
    this.cdref.detectChanges();
    //       localStorage.setItem('token', response.token);
    //       localStorage.setItem('role', response.role);

          if (response.role === UserRoleConstants.Admin) {
            this.router.navigate(['/admin/dashboard']);
          }
          else if (response.role === UserRoleConstants.Student) {
            this.router.navigate(['/student/dashboard']);
          }
          else if (response.role === UserRoleConstants.Company) {
            this.router.navigate(['/company/dashboard']);
          }

    //     }, error => {
    //       alert('Invalid credentials');
    //     });
  }
}