import { ReserveComponentType, ReserveTransactionType } from '../../core/api/api.model';

export const RESERVE_COMPONENTS: readonly ReserveComponentType[] = [
  'Indemnity',
  'Expense',
  'Alae',
  'SubrogationRecoverable',
];

export const RESERVE_TRANSACTIONS: readonly ReserveTransactionType[] = ['Add', 'Adjust', 'Reverse'];
