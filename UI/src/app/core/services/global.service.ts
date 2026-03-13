import { EventEmitter, Injectable } from '@angular/core';
import { UserModel, UserRoleConstants } from '../../shared';

@Injectable({
  providedIn: 'root',
})
export class GlobalService {
  public userInfo: UserModel = new UserModel();
  public userInfoUpdated: EventEmitter<boolean> = new EventEmitter<boolean>();

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
    sessionStorage.removeItem('userInfo');
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
}
