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
import { MatIconModule } from '@angular/material/icon';

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
    MatNativeDateModule,
    MatIconModule
  ],
  templateUrl: './create-task-dialog.component.html',
  styleUrl: './create-task-dialog.component.scss'
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
    @Inject(MAT_DIALOG_DATA) public data: { projectId: string; teamId: string }
  ) {
    this.taskForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(2)]],
      description: [''],
      status: [1],
      priority: [2],
      assigneeId: [null],
      dueDate: [null]
    });
  }

  ngOnInit(): void {
    this.loadTeamMembers();
  }

  loadTeamMembers(): void {
    // Load only members from the project's team
    this.teamsService.getTeamMembers(this.data.teamId).subscribe({
      next: (members) => {
        this.teamMembers = members;
      },
      error: (error) => {
        console.error('Failed to load team members:', error);
        this.snackBar.open('Failed to load team members', 'Close', { duration: 3000 });
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