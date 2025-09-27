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
  template: `
    <div class="teams-container">
      <div class="header">
        <h1>My Teams</h1>
        <button mat-raised-button color="primary" (click)="openCreateDialog()">
          <mat-icon>add</mat-icon>
          Create Team
        </button>
      </div>

      <div class="teams-grid" *ngIf="teams.length > 0; else noTeams">
        <mat-card *ngFor="let team of teams" class="team-card">
          <mat-card-header>
            <mat-card-title>{{ team.name }}</mat-card-title>
            <mat-card-subtitle>
              Created by {{ team.creator.firstName }} {{ team.creator.lastName }}
            </mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content>
            <p>{{ team.description }}</p>
            <div class="team-stats">
              <span class="stat">
                <mat-icon>people</mat-icon>
                {{ team.memberCount }} member{{ team.memberCount !== 1 ? 's' : '' }}
              </span>
              <span class="role-badge" [class]="getRoleClass(team.myRole)">
                {{ getRoleName(team.myRole) }}
              </span>
            </div>
            
            <!-- Show team members preview -->
            <div class="members-preview" *ngIf="team.members && team.members.length > 0">
              <h4>Members:</h4>
              <div class="member-chips">
                <span class="member-chip" *ngFor="let member of team.members | slice:0:3">
                  {{ member.firstName }} {{ member.lastName }}
                  <span class="member-role">({{ getRoleName(member.role) }})</span>
                </span>
                <span class="more-members" *ngIf="team.memberCount > 3">
                  +{{ team.memberCount - 3 }} more
                </span>
              </div>
            </div>
          </mat-card-content>
          
          <mat-card-actions>
            <button mat-button (click)="viewTeam(team.id)">
              <mat-icon>visibility</mat-icon>
              View Details
            </button>
            <button 
              mat-button 
              *ngIf="team.myRole === 1" 
              (click)="addMember(team)"
              color="primary">
              <mat-icon>person_add</mat-icon>
              Add Member
            </button>
          </mat-card-actions>
        </mat-card>
      </div>

      <ng-template #noTeams>
        <div class="no-teams">
          <mat-icon class="large-icon">groups</mat-icon>
          <h2>No teams yet</h2>
          <p>Create your first team to start collaborating with others.</p>
          <button mat-raised-button color="primary" (click)="openCreateDialog()">
            Create Your First Team
          </button>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .teams-container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
    }

    .teams-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 20px;
    }

    .team-card {
      height: fit-content;
    }

    .team-stats {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 16px;
    }

    .stat {
      display: flex;
      align-items: center;
      gap: 4px;
      color: #666;
    }

    .role-badge {
      padding: 4px 8px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 500;
    }

    .team-lead {
      background-color: #e3f2fd;
      color: #1976d2;
    }

    .member {
      background-color: #f3e5f5;
      color: #7b1fa2;
    }

    .members-preview {
      margin-top: 16px;
    }

    .members-preview h4 {
      margin: 0 0 8px 0;
      font-size: 14px;
      color: #666;
    }

    .member-chips {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .member-chip {
      background-color: #f5f5f5;
      padding: 4px 8px;
      border-radius: 16px;
      font-size: 12px;
      color: #333;
    }

    .member-role {
      color: #666;
      font-weight: normal;
    }

    .more-members {
      background-color: #e0e0e0;
      padding: 4px 8px;
      border-radius: 16px;
      font-size: 12px;
      color: #666;
    }

    .no-teams {
      text-align: center;
      padding: 60px 20px;
      color: #666;
    }

    .large-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
      color: #ccc;
    }

    mat-card-actions button {
      margin-right: 8px;
    }
  `]
})
export class TeamsListComponent implements OnInit {
  teams: Team[] = [];

  constructor(
    private teamsService: TeamsService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
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
        this.loadTeams(); // Reload teams to show updated member count
      }
    });
  }

  getRoleName(role: number): string {
    return this.teamsService.getRoleName(role);
  }

  getRoleClass(role: number): string {
    return role === 1 ? 'team-lead' : 'member';
  }

  viewTeam(teamId: string): void {
    // TODO: Navigate to team details
    console.log('View team:', teamId);
  }
}