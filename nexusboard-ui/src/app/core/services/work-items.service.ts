import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';  // Add this line

export interface WorkItem {
  id: string;
  title: string;
  description: string;
  status: number;
  priority: number;
  dueDate: string | null;
  createdAt: string;
  completedAt: string | null;
  assignee: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
  } | null;
  creator: {
    id: string;
    firstName: string;
    lastName: string;
  };
  fileCount: number;
}

export interface CreateWorkItemRequest {
  title: string;
  description: string;
  projectId: string;
  assigneeId: string | null;
  status: number;
  priority: number;
  dueDate: string | null;
}

export interface UpdateWorkItemRequest {
  title: string;
  description: string;
  status: number;
  priority: number;
  assigneeId: string | null;
  dueDate: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class WorkItemsService {
  private readonly API_URL = environment.apiUrl;  // Change this line

  constructor(private http: HttpClient) { }

  getProjectWorkItems(projectId: string): Observable<WorkItem[]> {
    return this.http.get<WorkItem[]>(`${this.API_URL}/workitems/project/${projectId}`);
  }

  createWorkItem(request: CreateWorkItemRequest): Observable<WorkItem> {
    return this.http.post<WorkItem>(`${this.API_URL}/workitems`, request);
  }

  updateWorkItem(workItemId: string, request: UpdateWorkItemRequest): Observable<any> {
    return this.http.put(`${this.API_URL}/workitems/${workItemId}`, request);
  }

  deleteWorkItem(workItemId: string): Observable<any> {
    return this.http.delete(`${this.API_URL}/workitems/${workItemId}`);
  }

  getStatusName(status: number): string {
    const statuses = {
      1: 'Todo',
      2: 'In Progress', 
      3: 'Review',
      4: 'Done'
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

  getPriorityColor(priority: number): string {
    const colors = {
      1: '#4caf50',
      2: '#2196f3',
      3: '#ff9800',
      4: '#f44336'
    };
    return colors[priority as keyof typeof colors] || '#666';
  }

  getStatusColumns() {
    return [
      { id: 1, name: 'Todo', color: '#2196f3' },
      { id: 2, name: 'In Progress', color: '#ff9800' },
      { id: 3, name: 'Review', color: '#9c27b0' },
      { id: 4, name: 'Done', color: '#4caf50' }
    ];
  }
}