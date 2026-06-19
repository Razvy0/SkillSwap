import api from './api';

export interface TimeTransactionDto {
  id: string;
  userId: string;
  amount: number;
  transactionType: 'EscrowHold' | 'Earned' | 'Spent' | 'Refunded';
  swapRequestId?: number;
  createdAt: string;
}

export const getMyTimeTransactions = async () => {
  const { data } = await api.get<TimeTransactionDto[]>('/TimeTransactions/my');
  return data;
};
