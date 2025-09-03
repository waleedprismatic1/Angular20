import { Routes } from '@angular/router';
import { SideNavComponent } from './side-nav/side-nav.component';
import { SignInComponent } from './sign-in/sign-in.component';
import { SignUpComponent } from './sign-up/sign-up.component';
import { HomeComponent } from './home/home.component';
import { AddUserComponent } from './add-user/add-user.component';
import { UsersListComponent } from './users-list/users-list.component';

export const routes: Routes = [
  // 👇 default route redirect
  { path: '', redirectTo: 'sign-in', pathMatch: 'full' },

  // auth pages
  { path: 'sign-in', component: SignInComponent },
  { path: 'sign-up', component: SignUpComponent },

  // app pages (inside sidenav)
  {
    path: '',
    component: SideNavComponent,
    children: [
      { path: 'home', component: HomeComponent },
      {path: 'add-user', component: AddUserComponent},
      {path: 'users-list', component: UsersListComponent}
      
      // aur bhi child routes
    ]
  },

  // fallback
  { path: '**', redirectTo: 'sign-in', pathMatch: 'full' }
];
