import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' }, // Automatic redirection to homepage
  { path: 'home', loadComponent: () => import('./features/homepage/homepage.component').then(m => m.HomepageComponent) }, // Homepage/Landing page
  { path: 'login', component: LoginComponent }, // Regular component loading
  { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) }, // loadComponent = "Only load this page when someone actually visits it"
  { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard/dashboard.component').then(m => m.DashboardComponent) },
  { path: 'teams', loadComponent: () => import('./features/teams/teams-list/teams-list.component').then(m => m.TeamsListComponent) },
  { path: 'projects', loadComponent: () => import('./features/projects/projects-list/projects-list.component').then(m => m.ProjectsListComponent) },
  { path: 'projects/:id/tasks', loadComponent: () => import('./features/tasks/kanban-board/kanban-board.component').then(m => m.KanbanBoardComponent) },
  { path: '**', redirectTo: '/home' } // Wildcard route for a 404 page (redirect to homepage)
];