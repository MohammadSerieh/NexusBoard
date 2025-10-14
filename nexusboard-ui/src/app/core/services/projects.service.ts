import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';  // Add this line

export interface Project {
  id: string;
  name: string;
  description: string;
  status: number;
  priority: number;
  startDate: string | null;
  endDate: string | null;
  createdAt: string;
  team: {
    id: string;
    name: string;
  };
  creator: {
    id: string;
    firstName: string;
    lastName: string;
  };
  taskCounts: {
    total: number;
    todo: number;
    inProgress: number;
    review: number;
    done: number;
  };
}

export interface CreateProjectRequest {
  name: string;
  description: string;
  teamId: string;
  status: number;
  priority: number;
  startDate: string | null;
  endDate: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectsService {
  private readonly API_URL = environment.apiUrl;  // Change this line

  constructor(private http: HttpClient) { }

  getMyProjects(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.API_URL}/projects`);
  }

  createProject(request: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(`${this.API_URL}/projects`, request);
  }

  getStatusName(status: number): string {
    const statuses = {
      1: 'Planning',
      2: 'Active', 
      3: 'On Hold',
      4: 'Completed',
      5: 'Cancelled'
    };
    return statuses[status as keyof typeof statuses] || 'Unknown';
  }

  getPriorityName(priority: number): string {
    const priorities = {
      1: 'Low',
      2: 'Medium',
      3: 'High', 
      4: 'Critical'
    };
    return priorities[priority as keyof typeof priorities] || 'Unknown';
  }

  getStatusColor(status: number): string {
    const colors = {
      1: '#2196f3',
      2: '#4caf50',
      3: '#ff9800',
      4: '#9c27b0',
      5: '#f44336'
    };
    return colors[status as keyof typeof colors] || '#666';
  }

  getPriorityColor(priority: number): string {
    const colors = {
      1: '#4caf50',
      2: '#2196f3',
      3: '#ff9800',
      4: '#f44336'
    };
    return colors[priority as keyof typeof colors] || '#666';
  }
}