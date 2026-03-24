import { Component, signal, inject } from '@angular/core';
import { GlobalService } from './core';
import { UserModel } from './shared';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.scss'
})
export class AppComponent {
  protected readonly title = signal('Placement Portal');
  public userInfo: UserModel;
  private snackBar = inject(MatSnackBar);

  constructor(private globalService: GlobalService) {
    this.userInfo = this.globalService.userInfo;
    this.showMessage();
  }

  private showMessage() {
    this.globalService.showMessage.subscribe(msg => {
      //msgtypes = error, success 
      this.snackBar.open(msg.text, 'Close', {
        duration: 3000,
        horizontalPosition: 'center',
        verticalPosition: 'top',
        panelClass: [msg.type + '-snackbar']
      });
    });
  }
}
