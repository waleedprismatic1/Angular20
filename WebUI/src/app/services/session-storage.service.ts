import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SessionStorageService {

  // generic methods (already hain)
 setItem(key: string, value: any): void {
  if (typeof value === 'string') {
    sessionStorage.setItem(key, value);   // token, email, id jese strings
  } else {
    sessionStorage.setItem(key, JSON.stringify(value)); // objects/arrays
  }
}

 getItem<T>(key: string): T | null {
  const item = sessionStorage.getItem(key);
  if (!item) return null;

  try {
    return JSON.parse(item) as T;   // agar object/array hoga to parse ho jayega
  } catch {
    return item as unknown as T;    // agar plain string hoga to waisa hi return
  }
}

  removeItem(key: string): void {
    sessionStorage.removeItem(key);
  }

  clear(): void {
    sessionStorage.clear();
  }

  // ✅ custom helpers
  setToken(token: string): void {
  sessionStorage.setItem('token', token); // ✅ direct save, quotes nahi
}

getToken(): string | null {
  return sessionStorage.getItem('token'); // ✅ direct fetch
}


  setUsername(username: string): void {
    this.setItem('username', username);
  }

  getUsername(): string | null {
    return this.getItem<string>('username');
  }
}
