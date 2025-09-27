import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';

import { TeamsService, AddMemberRequest, Team } from '../../../core/services/teams.service';

@Component({
  selector: 'app-add-member-dialog',
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
    <h2 mat-dialog-title>Add Member to {{ data.team.name }}</h2>
    
    <mat-dialog-content>
      <p>Invite a new member to join your team by entering their email address.</p>
      
      <form [formGroup]="memberForm">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email Address</mat-label>
          <input 
            matInput 
            type="email" 
            formControlName="email" 
            placeholder="Enter member's email">
          <mat-error *ngIf="memberForm.get('email')?.hasError('required')">
            Email is required
          </mat-error>
          <mat-error *ngIf="memberForm.get('email')?.hasError('email')">
            Please enter a valid email address
          </mat-error>
        </mat-form-field>
      </form>
      
      <p class="note">
        <strong>Note:</strong> The user must already have an account to be added to the team.
      </p>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onAddMember()"
        [disabled]="!memberForm.valid || isLoading">
        {{ isLoading ? 'Adding...' : 'Add Member' }}
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
    
    .note {
      font-size: 12px;
      color: #666;
      margin-top: 16px;
      padding: 8px;
      background-color: #f5f5f5;
      border-radius: 4px;
    }
  `]
})
export class AddMemberDialogComponent {
  memberForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private teamsService: TeamsService,
    private dialogRef: MatDialogRef<AddMemberDialogComponent>,
    private snackBar: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: { team: Team }
  ) {
    this.memberForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onAddMember(): void {
    if (this.memberForm.valid && !this.isLoading) {
      this.isLoading = true;
      const request: AddMemberRequest = this.memberForm.value;

      this.teamsService.addTeamMember(this.data.team.id, request).subscribe({
        next: (member) => {
          this.isLoading = false;
          this.snackBar.open(
            `${member.firstName} ${member.lastName} added to team successfully!`, 
            'Close', 
            { duration: 3000 }
          );
          this.dialogRef.close(true);
        },
        error: (error) => {
          this.isLoading = false;
          const message = error.error?.message || 'Failed to add team member';
          this.snackBar.open(message, 'Close', { duration: 5000 });
        }
      });
    }
  }
}