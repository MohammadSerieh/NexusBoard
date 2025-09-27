import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard/dashboard.component').then(m => m.DashboardComponent) },
  { path: 'teams', loadComponent: () => import('./features/teams/teams-list/teams-list.component').then(m => m.TeamsListComponent) },
  { path: '**', redirectTo: '/login' }
];