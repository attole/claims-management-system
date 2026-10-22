import { EMPTY_ID } from '../api/api-ids';
import { DEV_USERS, handlerName, userName } from './dev-users';

describe('dev users (S:UI-Audit, S:UI-Detail-Header)', () => {
  it('names a known user, the system for the empty id, and anyone else as unknown', () => {
    const names = [userName(DEV_USERS[1].id), userName(EMPTY_ID), userName('b2000000-0000-4000-8000-000000000009')];

    expect(names).toEqual(['Sam Supervisor', 'System', 'Unknown user']);
  });

  it('names a handler, and an empty id as Unassigned', () => {
    const names = [handlerName(DEV_USERS[0].id), handlerName(EMPTY_ID)];

    expect(names).toEqual(['Hana Handler', 'Unassigned']);
  });
});
