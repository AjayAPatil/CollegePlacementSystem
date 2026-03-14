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
}