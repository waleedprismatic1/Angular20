import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { SessionStorageService } from '../services/session-storage.service';
import { MaterialModule } from '../../material.module';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  templateUrl: './sign-in.component.html',
  styleUrl: './sign-in.component.css',
  imports: [ReactiveFormsModule, RouterModule, MaterialModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class SignInComponent {
  loginForm: FormGroup;
  errorMessage: string = '';
  showPassword: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private sessionStorage: SessionStorageService // 👈 yaha `private` lagana zaruri hai
  ) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }
  togglePassword() {
    this.showPassword = !this.showPassword;
  }
onSubmit() {
  if (!this.loginForm.valid) return;

  const { username, password } = this.loginForm.value;

  this.authService.login(username, password).subscribe({
    next: (res) => {
      const token = res.token;
      this.sessionStorage.setItem('token', token);

      // Decode token to get user id & role
      const decodedToken: any = this.authService.decodeToken(token);
      const userId = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

      // Call GetUser API with user id
      this.authService.getUser(userId, token).subscribe({
        next: (user) => {
          this.sessionStorage.setItem('userid', user.id);
          this.sessionStorage.setItem('username', user.userName);
          this.sessionStorage.setItem('email', user.email);

          // ✅ role from token
          this.sessionStorage.setItem(
            'role', 
            decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || ''
          );

          this.sessionStorage.setItem('firstname', user.firstName);
          this.sessionStorage.setItem('lastname', user.lastName);

          this.router.navigate(['/home']);
        },
        error: () => this.errorMessage = 'Failed to fetch user info'
      });
    },
    error: () => this.errorMessage = 'Invalid username or password'
  });
}




}
