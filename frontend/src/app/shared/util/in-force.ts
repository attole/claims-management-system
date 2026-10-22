import { calendarDateOf, toIsoDateTime } from '../../core/api/api-dates';
import { Policy } from '../../core/api/api.model';

export type InForce = 'unknown' | 'inForce' | 'outside';

export function inForce(policy: Policy | null, lossDate: Date | null): InForce {
  if (!policy || !lossDate) {
    return 'unknown';
  }

  const day = calendarDateOf(toIsoDateTime(lossDate));
  const isInsidePeriod = day >= policy.effectiveDate && day <= policy.expirationDate;

  return policy.status === 'Active' && isInsidePeriod ? 'inForce' : 'outside';
}
