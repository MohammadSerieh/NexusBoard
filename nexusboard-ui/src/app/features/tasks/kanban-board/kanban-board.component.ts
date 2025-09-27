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
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';

import { WorkItemsService, WorkItem } from '../../../core/services/work-items.service';
import { ProjectsService, Project } from '../../../core/services/projects.service';
import { CreateTaskDialogComponent } from '../create-task-dialog/create-task-dialog.component';

interface KanbanColumn {
  id: number;
  name: string;
  color: string;
  items: WorkItem[];
}

@Component({
  selector: 'app-kanban-board',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatChipsModule,
    MatToolbarModule,
    DragDropModule
  ],
  template: `
    <div class="kanban-container">
      <mat-toolbar class="project-header">
        <button mat-icon-button (click)="goBack()">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <span class="project-info" *ngIf="project">
          <h2>{{ project.name }}</h2>
          <span class="team-name">{{ project.team.name }}</span>
        </span>
        <span class="spacer"></span>
        <button mat-raised-button color="primary" (click)="openCreateTaskDialog()">
          <mat-icon>add</mat-icon>
          Add Task
        </button>
      </mat-toolbar>

      <div class="kanban-board" cdkDropListGroup>
        <div class="kanban-column" *ngFor="let column of columns">
          <div class="column-header" [style.border-color]="column.color">
            <h3 [style.color]="column.color">{{ column.name }}</h3>
            <span class="task-count">{{ column.items.length }}</span>
          </div>
          
          <div 
            class="task-list"
            cdkDropList
            [cdkDropListData]="column.items"
            [id]="'list-' + column.id"
            (cdkDropListDropped)="onTaskDrop($event)">
            
            <div 
              class="task-card"
              *ngFor="let task of column.items"
              cdkDrag>
              
              <mat-card class="task-item">
                <mat-card-header>
                  <mat-card-title class="task-title">{{ task.title }}</mat-card-title>
                  <mat-card-subtitle class="task-id">#{{ task.id.substring(0, 8) }}</mat-card-subtitle>
                </mat-card-header>
                
                <mat-card-content>
                  <p class="task-description" *ngIf="task.description">
                    {{ task.description | slice:0:100 }}{{ task.description.length > 100 ? '...' : '' }}
                  </p>
                  
                  <div class="task-meta">
                    <mat-chip 
                      class="priority-chip"
                      [style.background-color]="getPriorityColor(task.priority)"
                      [style.color]="'white'">
                      {{ getPriorityName(task.priority) }}
                    </mat-chip>
                    
                    <div class="task-assignee" *ngIf="task.assignee">
                      <mat-icon>person</mat-icon>
                      <span>{{ task.assignee.firstName }} {{ task.assignee.lastName }}</span>
                    </div>
                  </div>
                  
                  <div class="task-footer">
                    <div class="task-dates">
                      <span class="due-date" *ngIf="task.dueDate" [class.overdue]="isOverdue(task.dueDate)">
                        <mat-icon>schedule</mat-icon>
                        {{ formatDate(task.dueDate) }}
                      </span>
                    </div>
                    
                    <div class="task-actions">
                      <button mat-icon-button (click)="editTask(task)" class="edit-btn">
                        <mat-icon>edit</mat-icon>
                      </button>
                      <button mat-icon-button (click)="deleteTask(task)" class="delete-btn">
                        <mat-icon>delete</mat-icon>
                      </button>
                    </div>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
            
            <div class="empty-column" *ngIf="column.items.length === 0">
              <mat-icon>assignment</mat-icon>
              <span>No {{ column.name.toLowerCase() }} tasks</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .kanban-container {
      height: 100vh;
      display: flex;
      flex-direction: column;
    }

    .project-header {
      background-color: #fff;
      border-bottom: 1px solid #e0e0e0;
      color: #333;
    }

    .project-info h2 {
      margin: 0;
      font-size: 18px;
    }

    .team-name {
      font-size: 12px;
      color: #666;
    }

    .spacer {
      flex: 1 1 auto;
    }

    .kanban-board {
      flex: 1;
      display: flex;
      gap: 16px;
      padding: 16px;
      overflow-x: auto;
      background-color: #f5f5f5;
    }

    .kanban-column {
      min-width: 300px;
      background-color: #fff;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .column-header {
      padding: 16px;
      border-bottom: 1px solid #e0e0e0;
      border-top: 3px solid;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .column-header h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 500;
    }

    .task-count {
      background-color: #f0f0f0;
      padding: 2px 8px;
      border-radius: 12px;
      font-size: 12px;
      color: #666;
    }

    .task-list {
      padding: 8px;
      min-height: 200px;
    }

    .task-card {
      margin-bottom: 8px;
    }

    .task-item {
      cursor: pointer;
      transition: box-shadow 0.2s;
    }

    .task-item:hover {
      box-shadow: 0 4px 8px rgba(0,0,0,0.15);
    }

    .cdk-drag-preview {
      box-shadow: 0 8px 16px rgba(0,0,0,0.2);
      transform: rotate(5deg);
    }

    .cdk-drag-placeholder {
      opacity: 0.4;
    }

    .cdk-drag-animating {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }

    .task-list.cdk-drop-list-dragging .task-item:not(.cdk-drag-placeholder) {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }

    .task-title {
      font-size: 14px;
      font-weight: 500;
      margin-bottom: 4px;
    }

    .task-id {
      font-size: 11px;
      color: #999;
    }

    .task-description {
      font-size: 12px;
      color: #666;
      margin-bottom: 12px;
      line-height: 1.4;
    }

    .task-meta {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;
    }

    .priority-chip {
      font-size: 10px;
      height: 20px;
      line-height: 20px;
    }

    .task-assignee {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 11px;
      color: #666;
    }

    .task-assignee mat-icon {
      font-size: 14px;
      width: 14px;
      height: 14px;
    }

    .task-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .due-date {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 11px;
      color: #666;
    }

    .due-date.overdue {
      color: #f44336;
    }

    .due-date mat-icon {
      font-size: 14px;
      width: 14px;
      height: 14px;
    }

    .task-actions {
      display: flex;
      opacity: 0;
      transition: opacity 0.2s;
    }

    .task-item:hover .task-actions {
      opacity: 1;
    }

    .task-actions button {
      width: 24px;
      height: 24px;
      line-height: 24px;
    }

    .task-actions mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }

    .edit-btn {
      color: #2196f3;
    }

    .delete-btn {
      color: #f44336;
    }

    .empty-column {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 40px 20px;
      color: #ccc;
      text-align: center;
    }

    .empty-column mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 8px;
    }
  `]
})
export class KanbanBoardComponent implements OnInit {
  projectId!: string;
  project?: Project;
  columns: KanbanColumn[] = [];

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
    this.initializeColumns();
    this.loadTasks();
  }

  initializeColumns(): void {
    const statusColumns = this.workItemsService.getStatusColumns();
    this.columns = statusColumns.map(col => ({
      ...col,
      items: []
    }));
  }

  loadTasks(): void {
    this.workItemsService.getProjectWorkItems(this.projectId).subscribe({
      next: (tasks) => {
        this.distributeTasks(tasks);
      },
      error: (error) => {
        this.snackBar.open('Failed to load tasks', 'Close', { duration: 3000 });
      }
    });
  }

  distributeTasks(tasks: WorkItem[]): void {
    // Clear existing items
    this.columns.forEach(col => col.items = []);
    
    // Distribute tasks to appropriate columns
    tasks.forEach(task => {
      const column = this.columns.find(col => col.id === task.status);
      if (column) {
        column.items.push(task);
      }
    });
  }

  onTaskDrop(event: CdkDragDrop<WorkItem[]>): void {
    if (event.previousContainer === event.container) {
      // Reorder within same column
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Move to different column
      const task = event.previousContainer.data[event.previousIndex];
      const newStatus = parseInt(event.container.id.replace('list-', ''));
      
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );

      // Update task status in backend
      this.updateTaskStatus(task, newStatus);
    }
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
      },
      error: (error) => {
        this.snackBar.open('Failed to update task status', 'Close', { duration: 3000 });
        // Revert the change
        this.loadTasks();
      }
    });
  }

  openCreateTaskDialog(): void {
    const dialogRef = this.dialog.open(CreateTaskDialogComponent, {
      width: '600px',
      data: { projectId: this.projectId }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTasks();
      }
    });
  }

  getPriorityName(priority: number): string {
    return this.workItemsService.getPriorityName(priority);
  }

  getPriorityColor(priority: number): string {
    return this.workItemsService.getPriorityColor(priority);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric' 
    });
  }

  isOverdue(dueDate: string): boolean {
    return new Date(dueDate) < new Date();
  }

  editTask(task: WorkItem): void {
    console.log('Edit task:', task.id);
    // TODO: Open edit dialog
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