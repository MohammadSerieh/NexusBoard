import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { Router, RouterModule } from '@angular/router'; // Add RouterModule here

import { AuthService, User } from '../../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatCardModule,
    RouterModule  // Add this line
  ],
  template: `
    <mat-toolbar color="primary">
      <span>NexusBoard</span>
      <button mat-button routerLink="/teams">Teams</button>
      <button mat-button routerLink="/projects">Projects</button>
      <span class="spacer"></span>
      <span *ngIf="currentUser">Welcome, {{ currentUser.firstName }}!</span>
      <button mat-button (click)="logout()">Logout</button>
    </mat-toolbar>

    <div class="dashboard-container">
      <mat-card class="welcome-card">
        <mat-card-header>
          <mat-card-title>Dashboard</mat-card-title>
          <mat-card-subtitle>Welcome to NexusBoard</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div *ngIf="currentUser">
            <p><strong>Name:</strong> {{ currentUser.firstName }} {{ currentUser.lastName }}</p>
            <p><strong>Email:</strong> {{ currentUser.email }}</p>
            <p><strong>Role:</strong> {{ getRoleName(currentUser.role) }}</p>
          </div>
          <p>Your teams and projects will appear here soon!</p>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .spacer {
      flex: 1 1 auto;
    }
    
    .dashboard-container {
      padding: 20px;
    }
    
    .welcome-card {
      max-width: 600px;
      margin: 0 auto;
    }
  `]
})
export class DashboardComponent implements OnInit {
  currentUser: User | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      if (!user) {
        this.router.navigate(['/login']);
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getRoleName(role: number): string {
    switch (role) {
      case 1: return 'Admin';
      case 2: return 'Manager';
      case 3: return 'Member';
      default: return 'Unknown';
    }
  }
}