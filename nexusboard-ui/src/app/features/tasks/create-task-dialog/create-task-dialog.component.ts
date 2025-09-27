import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { WorkItemsService, CreateWorkItemRequest } from '../../../core/services/work-items.service';
import { TeamsService, TeamMember } from '../../../core/services/teams.service';

@Component({
  selector: 'app-create-task-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  template: `
    <h2 mat-dialog-title>Create New Task</h2>
    
    <mat-dialog-content>
      <form [formGroup]="taskForm">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Task Title</mat-label>
          <input matInput formControlName="title" placeholder="Enter task title">
          <mat-error *ngIf="taskForm.get('title')?.hasError('required')">
            Task title is required
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea 
            matInput 
            formControlName="description" 
            placeholder="Describe what needs to be done"
            rows="3">
          </textarea>
        </mat-form-field>

        <div class="form-row">
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Status</mat-label>
            <mat-select formControlName="status">
              <mat-option [value]="1">Todo</mat-option>
              <mat-option [value]="2">In Progress</mat-option>
              <mat-option [value]="3">Review</mat-option>
              <mat-option [value]="4">Done</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Priority</mat-label>
            <mat-select formControlName="priority">
              <mat-option [value]="1">Low</mat-option>
              <mat-option [value]="2">Medium</mat-option>
              <mat-option [value]="3">High</mat-option>
              <mat-option [value]="4">Critical</mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <div class="form-row">
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Assignee</mat-label>
            <mat-select formControlName="assigneeId">
              <mat-option [value]="null">Unassigned</mat-option>
              <mat-option *ngFor="let member of teamMembers" [value]="member.id">
                {{ member.firstName }} {{ member.lastName }}
              </mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Due Date</mat-label>
            <input matInput [matDatepicker]="duePicker" formControlName="dueDate">
            <mat-datepicker-toggle matIconSuffix [for]="duePicker"></mat-datepicker-toggle>
            <mat-datepicker #duePicker></mat-datepicker>
          </mat-form-field>
        </div>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onCreate()"
        [disabled]="!taskForm.valid || isLoading">
        {{ isLoading ? 'Creating...' : 'Create Task' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    
    .form-row {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
    }
    
    .half-width {
      flex: 1;
    }
    
    mat-dialog-content {
      min-width: 500px;
      max-height: 70vh;
      overflow-y: auto;
    }
  `]
})
export class CreateTaskDialogComponent implements OnInit {
  taskForm: FormGroup;
  teamMembers: TeamMember[] = [];
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private workItemsService: WorkItemsService,
    private teamsService: TeamsService,
    private dialogRef: MatDialogRef<CreateTaskDialogComponent>,
    private snackBar: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: { projectId: string }
  ) {
    this.taskForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(2)]],
      description: [''],
      status: [1], // Default to Todo
      priority: [2], // Default to Medium
      assigneeId: [null],
      dueDate: [null]
    });
  }

  ngOnInit(): void {
    this.loadTeamMembers();
  }

  loadTeamMembers(): void {
    // Note: This is a simplified approach. In a real app, you'd get the project's team members
    this.teamsService.getMyTeams().subscribe({
      next: (teams) => {
        // For now, we'll use members from all user's teams
        // In practice, you'd get the specific project's team members
        this.teamMembers = teams.flatMap(team => team.members || []);
      },
      error: (error) => {
        console.error('Failed to load team members:', error);
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onCreate(): void {
    if (this.taskForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      const formValue = this.taskForm.value;
      const request: CreateWorkItemRequest = {
        title: formValue.title,
        description: formValue.description,
        projectId: this.data.projectId,
        assigneeId: formValue.assigneeId,
        status: formValue.status,
        priority: formValue.priority,
        dueDate: formValue.dueDate ? formValue.dueDate.toISOString() : null
      };

      this.workItemsService.createWorkItem(request).subscribe({
        next: (workItem) => {
          this.isLoading = false;
          this.snackBar.open(`Task "${workItem.title}" created successfully!`, 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          this.isLoading = false;
          const message = error.error?.message || 'Failed to create task';
          this.snackBar.open(message, 'Close', { duration: 5000 });
        }
      });
    }
  }
}