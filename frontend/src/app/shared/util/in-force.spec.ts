import { Policy } from '../../core/api/api.model';
import { inForce } from './in-force';

describe('inForce (S:UI-Intake-Policy, S:BR-C-02)', () => {
  const policy: Policy = {
    id: 'a1000000-0000-4000-8000-000000000010',
    policyNumber: 'POL-48111',
    clientName: 'Harbor Logistics',
    effectiveDate: '2026-01-01',
    expirationDate: '2026-12-31',
    effectiveEndDate: '2026-12-31',
    status: 'Active',
    coverageTypes: [],
  };

  it('compares the UTC day of the loss with the start and the effective end, whatever the status is today', () => {
    const results = [
      inForce(policy, new Date(Date.UTC(2026, 6, 22))),
      inForce(policy, new Date(Date.UTC(2027, 0, 2))),
      inForce({ ...policy, status: 'Expired' }, new Date(Date.UTC(2026, 6, 22))),
      inForce({ ...policy, status: 'Cancelled', effectiveEndDate: '2026-03-31' }, new Date(Date.UTC(2026, 6, 22))),
      inForce(null, new Date()),
    ];

    expect(results).toEqual(['inForce', 'outside', 'inForce', 'outside', 'unknown']);
  });
});
