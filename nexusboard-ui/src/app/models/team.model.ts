export interface Team {
  id: string;
  name: string;
  description: string;
  createdAt: Date;
  updatedAt: Date;
  memberCount?: number;
}

export interface TeamMember {
  id: string;
  userId: string;
  teamId: string;
  role: TeamRole;
  joinedAt: Date;
  user: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
  };
}

export interface CreateTeamRequest {
  name: string;
  description: string;
}

export interface InviteTeamMemberRequest {
  email: string;
  role: TeamRole;
}

export enum TeamRole {
  Owner = 'Owner',
  Admin = 'Admin', 
  Member = 'Member',
  Viewer = 'Viewer'
}