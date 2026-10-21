import { EMPTY_ID } from '../api/api-ids';
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

export const SYSTEM_USER_NAME = 'System';
export const UNKNOWN_USER_NAME = 'Unknown user';

export function userName(userId: string): string {
  const user = DEV_USERS.find((candidate) => candidate.id === userId);
  const fallback = userId === EMPTY_ID ? SYSTEM_USER_NAME : UNKNOWN_USER_NAME;

  return user?.name ?? fallback;
}
