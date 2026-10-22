import { Issue, Party } from '../../core/api/api.model';
import { allowedNextStatuses, canAcknowledge, canWaive, isClaimEditable, isLastActiveClaimant } from './action-gates';

describe('action gates (S:BR-ST-02, S:ST-Closed-Reopened, S:PARTY-LastClaimant)', () => {
  const issue = (change: Partial<Issue>): Issue => ({
    id: 'i1',
    claimId: 'c1',
    code: 'S:BR-C-02',
    message: '',
    field: '',
    severity: 'Warning',
    state: 'Raised',
    resolvedAt: '',
    resolvedBy: '',
    ...change,
  });
  const party = (change: Partial<Party>): Party => ({
    id: 'p1',
    claimId: 'c1',
    role: 'Claimant',
    type: 'Person',
    firstName: 'Ada',
    lastName: '',
    companyName: '',
    email: '',
    phone: '',
    notes: '',
    isActive: true,
    ...change,
  });

  it('a closed or withdrawn claim is not editable, every other status is', () => {
    const results = (['Draft', 'Open', 'Reopened', 'Closed', 'Withdrawn'] as const).map(isClaimEditable);

    expect(results).toEqual([true, true, true, false, false]);
  });

  it('only a raised warning can be acknowledged', () => {
    const results = [
      canAcknowledge(issue({})),
      canAcknowledge(issue({ severity: 'Critical' })),
      canAcknowledge(issue({ state: 'Acknowledged' })),
    ];

    expect(results).toEqual([true, false, false]);
  });

  it('a supervisor waives a raised critical issue unless it must be fixed', () => {
    const critical = issue({ severity: 'Critical', code: 'S:BR-ST-02' });
    const results = [
      canWaive(critical, 'Supervisor'),
      canWaive(critical, 'Handler'),
      canWaive(issue({ severity: 'Critical', code: 'S:BR-C-05' }), 'Manager'),
      canWaive(issue({}), 'Manager'),
    ];

    expect(results).toEqual([true, false, false, false]);
  });

  it('offers Reopened only to a supervisor or manager', () => {
    const results = [allowedNextStatuses(['Reopened'], 'Handler'), allowedNextStatuses(['Reopened'], 'Supervisor')];

    expect(results).toEqual([[], ['Reopened']]);
  });

  it('marks the last active claimant, ignoring inactive claimants and other roles', () => {
    const only = party({});
    const parties = [only, party({ id: 'p2', isActive: false }), party({ id: 'p3', role: 'Witness' })];

    const result = isLastActiveClaimant(only, parties);

    expect(result).toBe(true);
  });

  it('does not mark a claimant while another active claimant remains', () => {
    const first = party({});
    const parties = [first, party({ id: 'p4' })];

    const result = isLastActiveClaimant(first, parties);

    expect(result).toBe(false);
  });
});
