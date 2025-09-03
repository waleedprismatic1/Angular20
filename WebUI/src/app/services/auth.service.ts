import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { UserRequest } from '../models/user-request';
import { SessionStorageService } from './session-storage.service';
import { Role } from '../models/user-request';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient,private sessionStorage: SessionStorageService) {}

  // ✅ login API
  login(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/login`, { username, password });
  }

  // ✅ decode token aur user ka data nikalna
  getUser(username: string, token: string) {
  return this.http.get<any>(`${this.apiUrl}/users/${username}`, {
    headers: { Authorization: `Bearer ${token}` }
  });
}
decodeToken(token: string): any {
  if (!token) return null;
  const payload = token.split('.')[1];
  return JSON.parse(atob(payload));
}

 createUser(user: UserRequest, headers?: HttpHeaders) {
  return this.http.post(`${this.apiUrl}/Users/CreateUser`, user, { headers });
}


  // Roles fetch
  getRoles(headers?: HttpHeaders): Observable<Role[]> {
  return this.http.get<Role[]>(`${this.apiUrl}/Roles/GetRoles`, { headers });
}





}


