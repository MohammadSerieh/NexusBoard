import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';

import { TeamsService, CreateTeamRequest } from '../../../core/services/teams.service';

@Component({
  selector: 'app-create-team-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>Create New Team</h2>
    
    <mat-dialog-content>
      <form [formGroup]="teamForm">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Team Name</mat-label>
          <input matInput formControlName="name" placeholder="Enter team name">
          <mat-error *ngIf="teamForm.get('name')?.hasError('required')">
            Team name is required
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea 
            matInput 
            formControlName="description" 
            placeholder="Describe what this team does"
            rows="3">
          </textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onCreate()"
        [disabled]="!teamForm.valid || isLoading">
        {{ isLoading ? 'Creating...' : 'Create Team' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    
    mat-dialog-content {
      min-width: 400px;
    }
  `]
})
export class CreateTeamDialogComponent {
  teamForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private teamsService: TeamsService,
    private dialogRef: MatDialogRef<CreateTeamDialogComponent>,
    private snackBar: MatSnackBar
  ) {
    this.teamForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: ['']
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onCreate(): void {
    if (this.teamForm.valid && !this.isLoading) {
      this.isLoading = true;
      const request: CreateTeamRequest = this.teamForm.value;

      this.teamsService.createTeam(request).subscribe({
        next: (team) => {
          this.isLoading = false;
          this.snackBar.open(`Team "${team.name}" created successfully!`, 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          this.isLoading = false;
          const message = error.error?.message || 'Failed to create team';
          this.snackBar.open(message, 'Close', { duration: 5000 });
        }
      });
    }
  }
}