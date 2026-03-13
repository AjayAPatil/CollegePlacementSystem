import { ChangeDetectorRef, Component } from '@angular/core';
import { GlobalService } from '../../core';
import { UserModel } from '../../shared';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  public userInfo: UserModel;

  constructor(
    private globalService: GlobalService,
    private router: Router,
    private cdref: ChangeDetectorRef
  ) {
    this.userInfo = this.globalService.userInfo;
    this.globalService.userInfoUpdated.subscribe(() => {
      this.userInfo = this.globalService.userInfo;
      this.cdref.detectChanges();
    });
  }

  navigateToHome() {
    this.router.navigate(['/']);
  }

  navigateToDashboard() {
    if (this.userInfo.role === 'Student') {
      this.router.navigate(['/student/dashboard']);
    } else if (this.userInfo.role === 'Company') {
      this.router.navigate(['/company/dashboard']);
    } else if (this.userInfo.role === 'Admin') {
      this.router.navigate(['/admin/dashboard']);
    } else {
      this.router.navigate(['/']);
    }
  }

  logout() {
    this.globalService.clearUserInfo();
    this.router.navigate(['/']);
  }
}
