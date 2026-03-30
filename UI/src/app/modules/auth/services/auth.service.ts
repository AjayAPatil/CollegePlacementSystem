import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments';
import { ResponseModel, UserModel } from '../../../shared';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = environment.apiUrl; // Base URL for API endpoints

  constructor(private http: HttpClient) {}

  upsertUser(formData: FormData): Observable<ResponseModel<UserModel>> {
    return this.http.post<ResponseModel<UserModel>>(`${this.apiUrl}/user`, formData);
  }

  login(data: Partial<UserModel>): Observable<ResponseModel<UserModel>> {
    return this.http.post<ResponseModel<UserModel>>(`${this.apiUrl}/auth`, data);
  }

  getProfile(userId: number): Observable<ResponseModel<UserModel>> {
    return this.http.get<ResponseModel<UserModel>>(`${this.apiUrl}/user/profile/${userId}`);
  }

  updateProfile(data: UserModel): Observable<ResponseModel<UserModel>> {
    return this.http.put<ResponseModel<UserModel>>(`${this.apiUrl}/user/profile`, data);
  }

  changePassword(data: { userId: number; oldPassword: string; newPassword: string; verifyNewPassword: string; }): Observable<ResponseModel> {
    return this.http.put<ResponseModel>(`${this.apiUrl}/user/change-password`, data);
  }
}
