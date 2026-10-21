import { UserRole } from '../api/api.model';

export interface DevUser {
  id: string;
  name: string;
  role: UserRole;
}

export const DEV_USERS: readonly DevUser[] = [
  { id: 'a1000000-0000-4000-8000-000000000001', name: 'Hana Handler', role: 'Handler' },
  { id: 'a1000000-0000-4000-8000-000000000002', name: 'Sam Supervisor', role: 'Supervisor' },
  { id: 'a1000000-0000-4000-8000-000000000003', name: 'Mia Manager', role: 'Manager' },
];
