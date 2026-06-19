import React, { useState } from 'react';
import { X, AlertTriangle } from 'lucide-react';
import { useCreateDispute } from '@/hooks/useDisputes';

interface ReportModalProps {
  isOpen: boolean;
  onClose: () => void;
  swapRequestId: number;
}

export default function ReportModal({ isOpen, onClose, swapRequestId }: ReportModalProps) {
  const [reason, setReason] = useState('');
  const createDispute = useCreateDispute();

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (reason.length < 10) return;

    createDispute.mutate(
      { swapRequestId, reason },
      {
        onSuccess: () => {
          setReason('');
          onClose();
        },
      }
    );
  };

  return (
    <div className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden flex flex-col max-h-[90vh]">
        <div className="px-6 py-4 border-b flex justify-between items-center bg-red-50">
          <div className="flex items-center gap-2 text-red-700 font-semibold">
            <AlertTriangle size={20} />
            Report Issue
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 transition-colors">
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 flex flex-col gap-4">
          <p className="text-sm text-gray-600">
            Please describe why you are reporting this swap. If the other user didn't show up, or if there was any inappropriate behavior, let us know.
          </p>

          <div>
            <label htmlFor="reason" className="block text-sm font-medium text-gray-700 mb-1">
              Reason
            </label>
            <textarea
              id="reason"
              rows={4}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-red-500 outline-none resize-none transition-all"
              placeholder="Explain the issue in detail (min. 10 characters)..."
              required
              minLength={10}
            />
          </div>

          <div className="flex justify-end gap-3 mt-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-lg transition-colors border border-gray-300"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={reason.length < 10 || createDispute.isPending}
              className="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
            >
              {createDispute.isPending ? 'Submitting...' : 'Submit Report'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}