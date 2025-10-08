import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterModule } from '@angular/router';

import { AuthService, User } from '../../../core/services/auth.service';
import { TeamsService, Team } from '../../../core/services/teams.service';
import { ProjectsService, Project } from '../../../core/services/projects.service';

interface DashboardStats {
  totalTeams: number;
  totalProjects: number;
  totalTasks: number;
  completedTasks: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    MatChipsModule,
    MatButtonModule,
    RouterModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  currentUser: User | null = null;
  stats: DashboardStats = {
    totalTeams: 0,
    totalProjects: 0,
    totalTasks: 0,
    completedTasks: 0
  };
  
  recentProjects: Project[] = [];
  teams: Team[] = [];
  isLoading = true;

  constructor(
    private authService: AuthService,
    private teamsService: TeamsService,
    private projectsService: ProjectsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      if (!user) {
        this.router.navigate(['/login']);
      } else {
        this.loadDashboardData();
      }
    });
  }

  loadDashboardData(): void {
    this.isLoading = true;

    // Load teams
    this.teamsService.getMyTeams().subscribe({
      next: (teams) => {
        this.teams = teams;
        this.stats.totalTeams = teams.length;
        
        // Load projects after teams
        this.loadProjects();
      },
      error: (error) => {
        console.error('Failed to load teams:', error);
        this.isLoading = false;
      }
    });
  }

  loadProjects(): void {
    this.projectsService.getMyProjects().subscribe({
      next: (projects) => {
        this.recentProjects = projects.slice(0, 3); // Get 3 most recent
        this.stats.totalProjects = projects.length;
        
        // Calculate total tasks across all projects
        this.stats.totalTasks = projects.reduce((sum, p) => sum + p.taskCounts.total, 0);
        this.stats.completedTasks = projects.reduce((sum, p) => sum + p.taskCounts.done, 0);
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load projects:', error);
        this.isLoading = false;
      }
    });
  }

  getCompletionPercentage(): number {
    if (this.stats.totalTasks === 0) return 0;
    return Math.round((this.stats.completedTasks / this.stats.totalTasks) * 100);
  }

  getProjectProgress(project: Project): number {
    if (project.taskCounts.total === 0) return 0;
    return Math.round((project.taskCounts.done / project.taskCounts.total) * 100);
  }

  navigateToProjects(): void {
    this.router.navigate(['/projects']);
  }

  navigateToTeams(): void {
    this.router.navigate(['/teams']);
  }

  navigateToProject(projectId: string): void {
    this.router.navigate(['/projects', projectId, 'tasks']);
  }

  getStatusColor(status: number): string {
    return this.projectsService.getStatusColor(status);
  }

  getStatusName(status: number): string {
    return this.projectsService.getStatusName(status);
  }

  getPriorityColor(priority: number): string {
    return this.projectsService.getPriorityColor(priority);
  }

  getPriorityName(priority: number): string {
    return this.projectsService.getPriorityName(priority);
  }
}