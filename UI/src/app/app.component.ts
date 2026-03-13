import { Component, signal } from '@angular/core';
import { GlobalService } from './core';
import { UserModel } from './shared';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.scss'
})
export class AppComponent {
  protected readonly title = signal('Placement Portal');
  public userInfo: UserModel;
  constructor(private globalService: GlobalService) {
    this.userInfo = this.globalService.userInfo;
  }
}
