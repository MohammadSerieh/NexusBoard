import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ProjectsService, CreateProjectRequest } from '../../../core/services/projects.service';
import { TeamsService, Team } from '../../../core/services/teams.service';

@Component({
  selector: 'app-create-project-dialog',
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
    <h2 mat-dialog-title>Create New Project</h2>
    
    <mat-dialog-content>
      <form [formGroup]="projectForm">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Project Name</mat-label>
          <input matInput formControlName="name" placeholder="Enter project name">
          <mat-error *ngIf="projectForm.get('name')?.hasError('required')">
            Project name is required
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea 
            matInput 
            formControlName="description" 
            placeholder="Describe the project goals and scope"
            rows="3">
          </textarea>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Team</mat-label>
          <mat-select formControlName="teamId">
            <mat-option *ngFor="let team of teams" [value]="team.id">
              {{ team.name }}
            </mat-option>
          </mat-select>
          <mat-error *ngIf="projectForm.get('teamId')?.hasError('required')">
            Please select a team
          </mat-error>
        </mat-form-field>

        <div class="form-row">
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Status</mat-label>
            <mat-select formControlName="status">
              <mat-option [value]="1">Planning</mat-option>
              <mat-option [value]="2">Active</mat-option>
              <mat-option [value]="3">On Hold</mat-option>
              <mat-option [value]="4">Completed</mat-option>
              <mat-option [value]="5">Cancelled</mat-option>
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
            <mat-label>Start Date</mat-label>
            <input matInput [matDatepicker]="startPicker" formControlName="startDate">
            <mat-datepicker-toggle matIconSuffix [for]="startPicker"></mat-datepicker-toggle>
            <mat-datepicker #startPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field appearance="outline" class="half-width">
            <mat-label>End Date</mat-label>
            <input matInput [matDatepicker]="endPicker" formControlName="endDate">
            <mat-datepicker-toggle matIconSuffix [for]="endPicker"></mat-datepicker-toggle>
            <mat-datepicker #endPicker></mat-datepicker>
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
        [disabled]="!projectForm.valid || isLoading">
        {{ isLoading ? 'Creating...' : 'Create Project' }}
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
export class CreateProjectDialogComponent implements OnInit {
  projectForm: FormGroup;
  teams: Team[] = [];
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private projectsService: ProjectsService,
    private teamsService: TeamsService,
    private dialogRef: MatDialogRef<CreateProjectDialogComponent>,
    private snackBar: MatSnackBar
  ) {
    this.projectForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: [''],
      teamId: ['', Validators.required],
      status: [1], // Default to Planning
      priority: [2], // Default to Medium
      startDate: [null],
      endDate: [null]
    });
  }

  ngOnInit(): void {
    this.loadTeams();
  }

  loadTeams(): void {
    this.teamsService.getMyTeams().subscribe({
      next: (teams) => {
        this.teams = teams;
        // Auto-select team if user only belongs to one
        if (teams.length === 1) {
          this.projectForm.patchValue({ teamId: teams[0].id });
        }
      },
      error: (error) => {
        this.snackBar.open('Failed to load teams', 'Close', { duration: 3000 });
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onCreate(): void {
    if (this.projectForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      const formValue = this.projectForm.value;
      const request: CreateProjectRequest = {
        ...formValue,
        startDate: formValue.startDate ? formValue.startDate.toISOString() : null,
        endDate: formValue.endDate ? formValue.endDate.toISOString() : null
      };

      this.projectsService.createProject(request).subscribe({
        next: (project) => {
          this.isLoading = false;
          this.snackBar.open(`Project "${project.name}" created successfully!`, 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          this.isLoading = false;
          const message = error.error?.message || 'Failed to create project';
          this.snackBar.open(message, 'Close', { duration: 5000 });
        }
      });
    }
  }
}