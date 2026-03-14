import { Component } from '@angular/core';
import { GlobalService } from '../../../core';

@Component({
  selector: 'app-unauthorized',
  templateUrl: './unauthorized.component.html',
  standalone: false,
  styleUrl: './unauthorized.component.scss'
})
export class UnauthorizedComponent {
  public isLoggedIn: boolean = false;
  constructor(private globalService: GlobalService) {
    this.isLoggedIn = this.globalService.userInfo.isLoggedIn;
  }
}
