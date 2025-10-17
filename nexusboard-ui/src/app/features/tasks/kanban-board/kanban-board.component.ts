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
  templateUrl: './kanban-board.component.html',
  styleUrl: './kanban-board.component.scss'
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
    this.loadProject();
    this.initializeColumns();
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
    this.columns.forEach(col => col.items = []);
    
    tasks.forEach(task => {
      const column = this.columns.find(col => col.id === task.status);
      if (column) {
        column.items.push(task);
      }
    });
  }

  onTaskDrop(event: CdkDragDrop<WorkItem[]>): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const task = event.previousContainer.data[event.previousIndex];
      const newStatus = parseInt(event.container.id.replace('list-', ''));
      
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );

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
        this.loadTasks();
      }
    });
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

  switchToListView(): void {
    this.router.navigate(['/projects', this.projectId, 'tasks-list']);
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