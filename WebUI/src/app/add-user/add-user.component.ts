import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { BreadcrumbComponent } from '../breadcrumb/breadcrumb.component';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { SessionStorageService } from '../services/session-storage.service';
import { Role, UserRequest } from '../models/user-request';
import { HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { MaterialModule } from '../../material.module';

@Component({
  selector: 'app-add-user',
  standalone: true,
  imports: [BreadcrumbComponent, MaterialModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './add-user.component.html',
  styleUrl: './add-user.component.css'
})
export class AddUserComponent {
title: string = 'Add User';
userForm!: FormGroup;
   roles: Role[] = [];
  loading = false;

  constructor(private fb: FormBuilder, private userService: AuthService, private sessionStorage: SessionStorageService,private router: Router) {}

  ngOnInit(): void {
    this.userForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      userName: ['', Validators.required],
      password: ['', Validators.required],
      roleIds: this.fb.control([], Validators.required) // multi-select roles
    });

       this.loadRoles();
  }

loadRoles() {
  const token = sessionStorage.getItem('token');  // ✅ direct use
  const headers = new HttpHeaders({
    Authorization: `Bearer ${token}`
  });

  this.userService.getRoles(headers).subscribe({
    next: (res) => {
      this.roles = res;
      console.log("Roles loaded:", this.roles);
    },
    error: (err) => {
      console.error('Error loading roles', err);
      alert('Failed to load roles, check console for details');
    }
  });
}

 onCancel() {
    this.router.navigate(['/home']);
  }


submitForm(): void {
  if (this.userForm.invalid) return;

  this.loading = true;

  const formValue = this.userForm.value;

  const request: UserRequest = {
    firstName: formValue.firstName,
    lastName: formValue.lastName,
    email: formValue.email,
    phoneNumber: formValue.phoneNumber,
    userName: formValue.userName,
    password: formValue.password,
    roleIds: formValue.roleIds?.map((r: any) => r.toString()) || []
  };

  // Get token
  const token = sessionStorage.getItem('token');
if (!token) {
  alert('User not authenticated');
  this.loading = false;
  return;
}

const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

this.userService.createUser(request, headers).subscribe({
  next: (res) => {
    alert('User created successfully!');
    this.userForm.reset();
    this.loading = false;
  },
  error: (err) => {
  console.error('API Error full response:', err);
  if (err.error) {
    console.error('Backend error:', err.error);
    alert('Error creating user: ' + JSON.stringify(err.error));
  } else {
    alert('Error creating user');
  }
  this.loading = false;
}

});

}




}
