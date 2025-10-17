import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

import { WorkItemsService, WorkItem } from '../../../core/services/work-items.service';
import { ProjectsService, Project } from '../../../core/services/projects.service';
import { CreateTaskDialogComponent } from '../create-task-dialog/create-task-dialog.component';

@Component({
  selector: 'app-task-list-view',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatChipsModule,
    MatToolbarModule,
    MatTableModule,
    MatSortModule,
    MatTooltipModule,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './task-list-view.component.html',
  styleUrl: './task-list-view.component.scss'
})
export default class TaskListViewComponent implements OnInit {
  projectId!: string;
  project?: Project;
  tasks: WorkItem[] = [];
  filteredTasks: WorkItem[] = [];
  displayedColumns: string[] = ['status', 'title', 'priority', 'assignee', 'dueDate', 'created', 'actions'];
  
  statusFilter: number | null = null;
  priorityFilter: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private workItemsService: WorkItemsService,
    private projectsService: ProjectsService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id')!;
    this.loadProject();
    this.loadTasks();
  }

  loadProject(): void {
    this.projectsService.getMyProjects().subscribe({
      next: (projects) => {
        this.project = projects.find(p => p.id === this.projectId);
      },
      error: (error) => {
        console.error('Failed to load project:', error);
      }
    });
  }

  loadTasks(): void {
    this.workItemsService.getProjectWorkItems(this.projectId).subscribe({
      next: (tasks) => {
        this.tasks = tasks;
        this.applyFilters();
      },
      error: (error) => {
        this.snackBar.open('Failed to load tasks', 'Close', { duration: 3000 });
      }
    });
  }

  applyFilters(): void {
    this.filteredTasks = this.tasks.filter(task => {
      const statusMatch = this.statusFilter === null || task.status === this.statusFilter;
      const priorityMatch = this.priorityFilter === null || task.priority === this.priorityFilter;
      return statusMatch && priorityMatch;
    });
  }

  filterByStatus(status: number | null): void {
    this.statusFilter = status;
    this.applyFilters();
  }

  filterByPriority(priority: number | null): void {
    this.priorityFilter = priority;
    this.applyFilters();
  }

  sortData(sort: Sort): void {
    const data = this.filteredTasks.slice();
    
    if (!sort.active || sort.direction === '') {
      this.filteredTasks = data;
      return;
    }

    this.filteredTasks = data.sort((a, b) => {
      const isAsc = sort.direction === 'asc';
      
      switch (sort.active) {
        case 'status':
          return this.compare(a.status, b.status, isAsc);
        case 'title':
          return this.compare(a.title.toLowerCase(), b.title.toLowerCase(), isAsc);
        case 'priority':
          return this.compare(a.priority, b.priority, isAsc);
        case 'assignee':
          const aName = a.assignee ? `${a.assignee.firstName} ${a.assignee.lastName}` : '';
          const bName = b.assignee ? `${b.assignee.firstName} ${b.assignee.lastName}` : '';
          return this.compare(aName.toLowerCase(), bName.toLowerCase(), isAsc);
        case 'dueDate':
          return this.compare(
            a.dueDate ? new Date(a.dueDate).getTime() : 0,
            b.dueDate ? new Date(b.dueDate).getTime() : 0,
            isAsc
          );
        case 'created':
          return this.compare(
            new Date(a.createdAt).getTime(),
            new Date(b.createdAt).getTime(),
            isAsc
          );
        default:
          return 0;
      }
    });
  }

  compare(a: number | string, b: number | string, isAsc: boolean): number {
    return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
  }

  openCreateTaskDialog(): void {
    if (!this.project) {
      this.snackBar.open('Project information not loaded', 'Close', { duration: 3000 });
      return;
    }

    const dialogRef = this.dialog.open(CreateTaskDialogComponent, {
      width: '600px',
      data: { 
        projectId: this.projectId,
        teamId: this.project.team.id
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTasks();
      }
    });
  }

  switchToKanban(): void {
    this.router.navigate(['/projects', this.projectId, 'tasks']);
  }

  getStatusName(status: number): string {
    return this.workItemsService.getStatusName(status);
  }

  getStatusColor(status: number): string {
    const colors = {
      1: '#2196f3',
      2: '#ff9800',
      3: '#9c27b0',
      4: '#4caf50'
    };
    return colors[status as keyof typeof colors] || '#666';
  }

  getPriorityName(priority: number): string {
    return this.workItemsService.getPriorityName(priority);
  }

  getPriorityColor(priority: number): string {
    return this.workItemsService.getPriorityColor(priority);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', { 
      year: 'numeric',
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatDueDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', { 
      year: 'numeric',
      month: 'short', 
      day: 'numeric'
    });
  }

  isOverdue(dueDate: string): boolean {
    return new Date(dueDate) < new Date();
  }

  isDueSoon(dueDate: string): boolean {
    const due = new Date(dueDate);
    const now = new Date();
    const threeDaysFromNow = new Date(now.getTime() + (3 * 24 * 60 * 60 * 1000));
    return due > now && due <= threeDaysFromNow;
  }

  getTasksByStatus(status: number): number {
    return this.tasks.filter(t => t.status === status).length;
  }

  updateTaskStatus(task: WorkItem, newStatus: number): void {
    const updateRequest = {
      title: task.title,
      description: task.description,
      status: newStatus,
      priority: task.priority,
      assigneeId: task.assignee?.id || null,
      dueDate: task.dueDate
    };

    this.workItemsService.updateWorkItem(task.id, updateRequest).subscribe({
      next: () => {
        task.status = newStatus;
        this.snackBar.open('Task status updated', 'Close', { duration: 2000 });
        this.applyFilters();
      },
      error: (error) => {
        this.snackBar.open('Failed to update task status', 'Close', { duration: 3000 });
      }
    });
  }

  editTask(task: WorkItem): void {
    console.log('Edit task:', task.id);
    // TODO: Implement edit dialog
  }

  deleteTask(task: WorkItem): void {
    if (confirm(`Are you sure you want to delete "${task.title}"?`)) {
      this.workItemsService.deleteWorkItem(task.id).subscribe({
        next: () => {
          this.snackBar.open('Task deleted successfully', 'Close', { duration: 2000 });
          this.loadTasks();
        },
        error: (error) => {
          this.snackBar.open('Failed to delete task', 'Close', { duration: 3000 });
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/projects']);
  }
}