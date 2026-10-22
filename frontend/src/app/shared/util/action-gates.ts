import { ClaimStatus, Issue, Party, UserRole } from '../../core/api/api.model';

export const FROZEN_STATUSES: readonly ClaimStatus[] = ['Closed', 'Withdrawn'];
export const FIX_ONLY_ISSUE_CODES: readonly string[] = ['S:V-LossDate-Required', 'S:BR-C-05', 'S:BR-C-03'];

const SUPERVISING_ROLES: readonly UserRole[] = ['Supervisor', 'Manager'];
const SUPERVISED_STATUSES: readonly ClaimStatus[] = ['Reopened'];

export function isClaimEditable(status: ClaimStatus): boolean {
  return !FROZEN_STATUSES.includes(status);
}

export function canAcknowledge(issue: Issue): boolean {
  return issue.state === 'Raised' && issue.severity === 'Warning';
}

export function canWaive(issue: Issue, role: UserRole): boolean {
  const isWaivableIssue = issue.state === 'Raised' && issue.severity === 'Critical';
  const isFixOnly = FIX_ONLY_ISSUE_CODES.includes(issue.code);

  return isWaivableIssue && !isFixOnly && SUPERVISING_ROLES.includes(role);
}

export function allowedNextStatuses(nextStatuses: readonly ClaimStatus[], role: UserRole): ClaimStatus[] {
  const isSupervising = SUPERVISING_ROLES.includes(role);

  return nextStatuses.filter((status) => isSupervising || !SUPERVISED_STATUSES.includes(status));
}

export function isLastActiveClaimant(party: Party, parties: readonly Party[]): boolean {
  const activeClaimants = parties.filter((candidate) => candidate.isActive && candidate.role === 'Claimant');

  return party.isActive && party.role === 'Claimant' && activeClaimants.length === 1;
}
