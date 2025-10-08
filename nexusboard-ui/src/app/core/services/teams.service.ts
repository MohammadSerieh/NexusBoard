import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Team {
  id: string;
  name: string;
  description: string;
  createdAt: string;
  creator: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
  };
  myRole: string;
  memberCount: number;
  members: TeamMember[];
}

export interface TeamMember {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface CreateTeamRequest {
  name: string;
  description: string;
}

export interface AddMemberRequest {
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class TeamsService {
  private readonly API_URL = 'http://localhost:5058/api';

  constructor(private http: HttpClient) { }

  getMyTeams(): Observable<Team[]> {
    return this.http.get<Team[]>(`${this.API_URL}/teams`);
  }

  createTeam(request: CreateTeamRequest): Observable<Team> {
    return this.http.post<Team>(`${this.API_URL}/teams`, request);
  }

  addTeamMember(teamId: string, request: AddMemberRequest): Observable<TeamMember> {
    return this.http.post<TeamMember>(`${this.API_URL}/teams/${teamId}/members`, request);
  }

  removeTeamMember(teamId: string, memberId: string): Observable<any> {
    return this.http.delete(`${this.API_URL}/teams/${teamId}/members/${memberId}`);
  }

  getRoleName(role: string): string {
    if (role === 'TeamLead') {
      return 'Team Lead';
    }
    if (role === 'Member') {
      return 'Member';
    }
    return 'Unknown';
  }

  isTeamLead(role: string): boolean {
    return role === 'TeamLead';
  }
}