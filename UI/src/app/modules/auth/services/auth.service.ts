import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = environment.apiUrl; // Base URL for API endpoints

  constructor(private http: HttpClient) {}

  upsertUser(formData: FormData) {
    return this.http.post<any>(`${this.apiUrl}/user`, formData);
  }

  login(data: any) {
    return this.http.post<any>(`${this.apiUrl}/auth`, data);
  }

  getProfile(userId: number) {
    return this.http.get<any>(`${this.apiUrl}/user/profile/${userId}`);
  }

  updateProfile(data: any) {
    return this.http.put<any>(`${this.apiUrl}/user/profile`, data);
  }

  changePassword(data: { userId: number; oldPassword: string; newPassword: string; verifyNewPassword: string; }) {
    return this.http.put<any>(`${this.apiUrl}/user/change-password`, data);
  }
}
