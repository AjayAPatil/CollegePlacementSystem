import { EventEmitter, Injectable } from '@angular/core';
import { UserModel, UserRoleConstants } from '../../shared';

@Injectable({
  providedIn: 'root',
})
export class GlobalService {
  public userInfo: UserModel = new UserModel();
  public userInfoUpdated: EventEmitter<boolean> = new EventEmitter<boolean>();
  public showMessage: EventEmitter<{ text: string; type: string }> = new EventEmitter<{ text: string; type: string }>();

  constructor() {
    this.userInfo = JSON.parse(sessionStorage.getItem('userInfo') || '{}');
  }

  public setUserInfo(userInfo: UserModel) {
    this.userInfo = userInfo;
    sessionStorage.setItem('userInfo', JSON.stringify(userInfo));
    this.userInfoUpdated.emit(true);
    this.triggerReload();
  }

  public clearUserInfo(reload: boolean = true) {
    this.userInfo = new UserModel();
    localStorage.clear();
    sessionStorage.clear();
    this.userInfoUpdated.emit(true);
    if (reload) {
      this.triggerReload();
    }
  }
  private triggerReload() {
    setTimeout(() => {
      window.location.reload();
    }, 100);
  }

  public showSuccessMessage(messageText: string) {
    this.showMessage.emit({ text: messageText, type: 'success' });
  }
  public showErrorMessage(messageText: string) {
    this.showMessage.emit({ text: messageText, type: 'error' });
  }
}
