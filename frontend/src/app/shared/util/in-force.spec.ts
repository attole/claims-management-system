import { Policy } from '../../core/api/api.model';
import { inForce } from './in-force';

describe('inForce (S:UI-Intake-Policy)', () => {
  const policy: Policy = {
    id: 'a1000000-0000-4000-8000-000000000010',
    policyNumber: 'POL-48111',
    clientName: 'Harbor Logistics',
    effectiveDate: '2026-01-01',
    expirationDate: '2026-12-31',
    status: 'Active',
    coverageTypes: [],
  };

  it('is green only for an active policy covering the UTC day of the loss', () => {
    const results = [
      inForce(policy, new Date(Date.UTC(2026, 6, 22))),
      inForce(policy, new Date(Date.UTC(2027, 0, 2))),
      inForce({ ...policy, status: 'Expired' }, new Date(Date.UTC(2026, 6, 22))),
      inForce(null, new Date()),
    ];

    expect(results).toEqual(['inForce', 'outside', 'outside', 'unknown']);
  });
});
