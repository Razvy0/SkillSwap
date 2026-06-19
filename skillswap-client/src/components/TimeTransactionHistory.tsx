import { useMyTimeTransactions } from '@/hooks/useTimeTransactions';
import { Clock, ArrowUpRight, ArrowDownRight, RefreshCw, Lock } from 'lucide-react';

export default function TimeTransactionHistory() {
  const { data: transactions, isLoading } = useMyTimeTransactions();

  if (isLoading) return <p className="text-gray-500">Loading history...</p>;
  if (!transactions || transactions.length === 0) return <p className="text-gray-500">No transactions yet.</p>;

  const getIcon = (type: string) => {
    switch (type) {
      case 'Earned': return <ArrowUpRight className="text-green-500" size={20} />;
      case 'Spent': return <ArrowDownRight className="text-red-500" size={20} />;
      case 'EscrowHold': return <Lock className="text-orange-500" size={20} />;
      case 'Refunded': return <RefreshCw className="text-blue-500" size={20} />;
      default: return <Clock className="text-gray-500" size={20} />;
    }
  };

  const getLabel = (type: string) => {
    switch (type) {
      case 'Earned': return 'Time Earned';
      case 'Spent': return 'Time Spent';
      case 'EscrowHold': return 'Escrow Hold';
      case 'Refunded': return 'Time Refunded';
      default: return type;
    }
  };

  const getAmountColor = (type: string) => {
    switch (type) {
      case 'Earned':
      case 'Refunded': return 'text-green-600';
      case 'EscrowHold':
      case 'Spent': return 'text-red-600';
      default: return 'text-gray-900';
    }
  };

  const formatDate = (dateStr: string) => {
    const d = new Date(dateStr);
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(d);
  };

  return (
    <div className="bg-white border text-left border-gray-200 rounded-xl p-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
        <Clock size={20} /> Transaction History
      </h2>
      <div className="space-y-4">
        {transactions.map(tx => (
          <div key={tx.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-white rounded-full shadow-sm">
                {getIcon(tx.transactionType)}
              </div>
              <div>
                <p className="font-medium text-gray-900">{getLabel(tx.transactionType)}</p>
                <p className="text-xs text-gray-500">
                  {formatDate(tx.createdAt)} 
                  {tx.swapRequestId && ` • Swap #${tx.swapRequestId}`}
                </p>
              </div>
            </div>
            <div className={`font-bold ${getAmountColor(tx.transactionType)}`}>
              {tx.amount > 0 ? '+' : ''}{tx.amount} hr
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
