import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router } from '@angular/router';
import { ProjectsService, Project } from '../../../core/services/projects.service';
import { CreateProjectDialogComponent } from '../create-project-dialog/create-project-dialog.component';

@Component({
  selector: 'app-projects-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatChipsModule,
    MatProgressBarModule
  ],
  template: `
    <div class="projects-container">
      <div class="header">
        <h1>My Projects</h1>
        <button mat-raised-button color="primary" (click)="openCreateDialog()">
          <mat-icon>add</mat-icon>
          Create Project
        </button>
      </div>

      <div class="projects-grid" *ngIf="projects.length > 0; else noProjects">
        <mat-card *ngFor="let project of projects" class="project-card">
          <mat-card-header>
            <div mat-card-avatar class="status-avatar" [style.background-color]="getStatusColor(project.status)">
              <mat-icon>work</mat-icon>
            </div>
            <mat-card-title>{{ project.name }}</mat-card-title>
            <mat-card-subtitle>{{ project.team.name }}</mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content>
            <p class="description">{{ project.description }}</p>
            
            <div class="project-meta">
              <mat-chip-set>
                <mat-chip [style.color]="getStatusColor(project.status)">
                  {{ getStatusName(project.status) }}
                </mat-chip>
                <mat-chip [style.color]="getPriorityColor(project.priority)">
                  {{ getPriorityName(project.priority) }} Priority
                </mat-chip>
              </mat-chip-set>
            </div>

            <div class="progress-section" *ngIf="project.taskCounts.total > 0">
              <div class="progress-header">
                <span>Progress</span>
                <span>{{ project.taskCounts.done }}/{{ project.taskCounts.total }} tasks</span>
              </div>
              <mat-progress-bar 
                mode="determinate" 
                [value]="getProgressPercentage(project)"
                color="primary">
              </mat-progress-bar>
              <div class="task-breakdown">
                <span class="task-stat todo">{{ project.taskCounts.todo }} Todo</span>
                <span class="task-stat progress">{{ project.taskCounts.inProgress }} In Progress</span>
                <span class="task-stat review">{{ project.taskCounts.review }} Review</span>
                <span class="task-stat done">{{ project.taskCounts.done }} Done</span>
              </div>
            </div>

            <div class="no-tasks" *ngIf="project.taskCounts.total === 0">
              <mat-icon>assignment</mat-icon>
              <span>No tasks yet</span>
            </div>

            <div class="project-dates" *ngIf="project.startDate || project.endDate">
              <div class="date-info" *ngIf="project.startDate">
                <mat-icon>event</mat-icon>
                <span>Started: {{ formatDate(project.startDate) }}</span>
              </div>
              <div class="date-info" *ngIf="project.endDate">
                <mat-icon>event_available</mat-icon>
                <span>Due: {{ formatDate(project.endDate) }}</span>
              </div>
            </div>
          </mat-card-content>
          
          <mat-card-actions>
            <button mat-button (click)="viewProject(project.id)">
              <mat-icon>visibility</mat-icon>
              View Details
            </button>
            <button mat-button (click)="manageTasks(project.id)" color="primary">
              <mat-icon>assignment</mat-icon>
              Manage Tasks
            </button>
          </mat-card-actions>
        </mat-card>
      </div>

      <ng-template #noProjects>
        <div class="no-projects">
          <mat-icon class="large-icon">work_outline</mat-icon>
          <h2>No projects yet</h2>
          <p>Create your first project to start organizing your team's work.</p>
          <button mat-raised-button color="primary" (click)="openCreateDialog()">
            Create Your First Project
          </button>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .projects-container {
      padding: 20px;
      max-width: 1400px;
      margin: 0 auto;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
    }

    .projects-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
      gap: 24px;
    }

    .project-card {
      height: fit-content;
    }

    .status-avatar {
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
    }

    .description {
      color: #666;
      margin-bottom: 16px;
      line-height: 1.4;
    }

    .project-meta {
      margin-bottom: 16px;
    }

    .progress-section {
      margin: 16px 0;
    }

    .progress-header {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
      font-size: 14px;
      color: #666;
    }

    .task-breakdown {
      display: flex;
      gap: 16px;
      margin-top: 8px;
      font-size: 12px;
    }

    .task-stat {
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .task-stat:before {
      content: '';
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }

    .todo:before { background-color: #2196f3; }
    .progress:before { background-color: #ff9800; }
    .review:before { background-color: #9c27b0; }
    .done:before { background-color: #4caf50; }

    .no-tasks {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #999;
      font-size: 14px;
      margin: 16px 0;
    }

    .project-dates {
      margin-top: 16px;
    }

    .date-info {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 4px;
      font-size: 12px;
      color: #666;
    }

    .no-projects {
      text-align: center;
      padding: 80px 20px;
      color: #666;
    }

    .large-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
      color: #ccc;
    }

    mat-card-actions button {
      margin-right: 8px;
    }

    mat-chip {
      font-size: 11px;
    }
  `]
})
export class ProjectsListComponent implements OnInit {
  projects: Project[] = [];

  constructor(
    private projectsService: ProjectsService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.projectsService.getMyProjects().subscribe({
      next: (projects) => {
        this.projects = projects;
      },
      error: (error) => {
        this.snackBar.open('Failed to load projects', 'Close', { duration: 3000 });
      }
    });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateProjectDialogComponent, {
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadProjects();
      }
    });
  }

  getStatusName(status: number): string {
    return this.projectsService.getStatusName(status);
  }

  getPriorityName(priority: number): string {
    return this.projectsService.getPriorityName(priority);
  }

  getStatusColor(status: number): string {
    return this.projectsService.getStatusColor(status);
  }

  getPriorityColor(priority: number): string {
    return this.projectsService.getPriorityColor(priority);
  }

  getProgressPercentage(project: Project): number {
    if (project.taskCounts.total === 0) return 0;
    return (project.taskCounts.done / project.taskCounts.total) * 100;
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  viewProject(projectId: string): void {
    console.log('View project:', projectId);
    // TODO: Navigate to project details
  }

  // In the ProjectsListComponent, update the manageTasks method:
manageTasks(projectId: string): void {
  this.router.navigate(['/projects', projectId, 'tasks']);
}
}