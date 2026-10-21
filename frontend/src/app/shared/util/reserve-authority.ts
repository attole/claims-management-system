export const AUTO_APPROVAL_LIMIT = 10_000;
export const SUPERVISOR_APPROVAL_LIMIT = 100_000;

export type ReserveAuthority = 'none' | 'auto' | 'supervisor' | 'manager';

export function authorityFor(amount: number): ReserveAuthority {
  const size = Math.abs(amount);

  if (size === 0 || Number.isNaN(size)) {
    return 'none';
  }

  if (size <= AUTO_APPROVAL_LIMIT) {
    return 'auto';
  }

  return size <= SUPERVISOR_APPROVAL_LIMIT ? 'supervisor' : 'manager';
}
