import api from './api';

export interface Dispute {
  id: number;
  swapRequestId: number;
  reporterId: string;
  reporterName: string;
  reportedUserId: string;
  reportedUserName: string;
  reason: string;
  status: 'Pending' | 'Resolved' | 'Dismissed';
  createdAt: string;
  resolvedAt?: string;
  adminNotes?: string;
}

export interface CreateDisputeDto {
  swapRequestId: number;
  reason: string;
}

const disputeService = {
  createDispute: async (dto: CreateDisputeDto) => {
    const response = await api.post<Dispute>('/disputes', dto);
    return response.data;
  },

  getMyDisputes: async () => {
    const response = await api.get<Dispute[]>('/disputes/my');
    return response.data;
  },
};

export default disputeService;