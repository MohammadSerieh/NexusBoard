import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { TeamsService, Team, TeamMember } from '../../../core/services/teams.service';

@Component({
  selector: 'app-team-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  templateUrl: './team-details-dialog.component.html',
  styleUrl: './team-details-dialog.component.scss'
})
export class TeamDetailsDialogComponent {
  displayedColumns: string[] = ['name', 'email', 'role', 'actions'];
  isRemoving = false;

  constructor(
    private dialogRef: MatDialogRef<TeamDetailsDialogComponent>,
    private teamsService: TeamsService,
    private snackBar: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: { team: Team; currentUserId: string }
  ) {}

  getRoleName(role: string): string {
    return this.teamsService.getRoleName(role);
  }

  getRoleClass(role: string): string {
    return this.teamsService.isTeamLead(role) ? 'role-badge team-lead' : 'role-badge member';
  }

  canRemoveMember(member: TeamMember): boolean {
    // Can only remove if:
    // 1. Current user is team lead (myRole === 'TeamLead')
    // 2. Member is not themselves
    // 3. Member is not a team lead
    return (
      this.data.team.myRole === 'TeamLead' && 
      member.id !== this.data.currentUserId &&
      member.role !== 'TeamLead'
    );
  }

  removeMember(member: TeamMember): void {
    const confirmMessage = `Are you sure you want to remove ${member.firstName} ${member.lastName} from the team?`;
    
    if (!confirm(confirmMessage)) {
      return;
    }

    this.isRemoving = true;

    this.teamsService.removeTeamMember(this.data.team.id, member.id).subscribe({
      next: () => {
        this.isRemoving = false;
        this.snackBar.open(
          `${member.firstName} ${member.lastName} removed from team successfully!`,
          'Close',
          { duration: 3000 }
        );
        
        // Remove member from the local array
        const index = this.data.team.members.findIndex(m => m.id === member.id);
        if (index > -1) {
          this.data.team.members.splice(index, 1);
        }
        
        // Update member count
        this.data.team.memberCount = this.data.team.members.length;
        
        // Close dialog and refresh
        this.dialogRef.close(true);
      },
      error: (error) => {
        this.isRemoving = false;
        const message = error.error?.message || 'Failed to remove team member';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  onClose(): void {
    this.dialogRef.close();
  }
}