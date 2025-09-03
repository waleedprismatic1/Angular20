import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { SessionStorageService } from './session-storage.service';  // 👈 apna service import

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  userName: string;
  roles: string[];
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseUrl = environment.apiUrl; // 👈 env se API base URL

  constructor(
    private http: HttpClient,
    private sessionStorage: SessionStorageService   // 👈 inject
  ) {}

  getUsers(): Observable<User[]> {
    const token = this.sessionStorage.getToken();   // 👈 session se token lo

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<User[]>(`${this.baseUrl}/Users/GetUser`, { headers });
  }
}
