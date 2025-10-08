import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { TeamsService, Team } from '../../../core/services/teams.service';
import { CreateTeamDialogComponent } from '../create-team-dialog/create-team-dialog.component';
import { AddMemberDialogComponent } from '../add-member-dialog/add-member-dialog.component';
import { TeamDetailsDialogComponent } from '../team-details-dialog/team-details-dialog.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-teams-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule
  ],
  templateUrl: './teams-list.component.html',
  styleUrl: './teams-list.component.scss'
})
export class TeamsListComponent implements OnInit {
  teams: Team[] = [];

  constructor(
    private teamsService: TeamsService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadTeams();
  }

  loadTeams(): void {
    this.teamsService.getMyTeams().subscribe({
      next: (teams) => {
        this.teams = teams;
      },
      error: (error) => {
        this.snackBar.open('Failed to load teams', 'Close', { duration: 3000 });
      }
    });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateTeamDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTeams();
      }
    });
  }

  addMember(team: Team): void {
    const dialogRef = this.dialog.open(AddMemberDialogComponent, {
      width: '500px',
      data: { team }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTeams();
      }
    });
  }

  getRoleName(role: string): string {
    return this.teamsService.getRoleName(role);
  }

  getRoleClass(role: string): string {
    return this.teamsService.isTeamLead(role) ? 'team-lead' : 'member';
  }

  viewTeam(teamId: string): void {
    const team = this.teams.find(t => t.id === teamId);
    
    if (!team) {
      this.snackBar.open('Team not found', 'Close', { duration: 3000 });
      return;
    }

    const currentUser = this.authService.getCurrentUser();
    
    if (!currentUser) {
      this.snackBar.open('Please log in again', 'Close', { duration: 3000 });
      return;
    }

    const dialogRef = this.dialog.open(TeamDetailsDialogComponent, {
      width: '700px',
      data: { 
        team: team,
        currentUserId: currentUser.id
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTeams();
      }
    });
  }

  deleteTeam(team: Team): void {
    const confirmMessage = `Are you sure you want to delete "${team.name}"? This will remove all members and cannot be undone.`;
    
    if (!confirm(confirmMessage)) {
      return;
    }

    this.teamsService.deleteTeam(team.id).subscribe({
      next: () => {
        this.snackBar.open(`Team "${team.name}" deleted successfully!`, 'Close', { duration: 3000 });
        this.loadTeams();
      },
      error: (error) => {
        const message = error.error?.message || 'Failed to delete team';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }
}