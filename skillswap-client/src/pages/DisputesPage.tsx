import { useDisputes } from '@/hooks/useDisputes';
import { useAuthStore } from '@/stores/authStore';
import { AlertTriangle, Clock, CheckCircle, XCircle } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function DisputesPage() {
  const { data: disputes, isLoading } = useDisputes();
  const userId = useAuthStore(s => s.userId);

  if (isLoading) return <p className="text-gray-500">Loading disputes...</p>;

  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
        <AlertTriangle className="text-orange-500" />
        My Disputes
      </h1>
      
      {(!disputes || disputes.length === 0) ? (
        <div className="bg-white rounded-xl border border-gray-200 p-8 text-center text-gray-500">
          No disputes filed. You're doing great!
        </div>
      ) : (
        <div className="space-y-4">
          {disputes.map((dispute) => {
            const isReporter = dispute.reporterId === userId;
            const otherUserName = isReporter ? dispute.reportedUserName : dispute.reporterName;
            const otherUserId = isReporter ? dispute.reportedUserId : dispute.reporterId;
            
            return (
              <div key={dispute.id} className="bg-white rounded-xl border border-gray-200 p-6 flex flex-col gap-3">
                <div className="flex justify-between items-start">
                  <div>
                    <span className="text-sm font-medium px-2.5 py-1 rounded-full bg-gray-100 text-gray-800 border border-gray-200">
                      Swap #{dispute.swapRequestId}
                    </span>
                    <h3 className="text-lg font-semibold text-gray-900 mt-2">
                      {isReporter ? 'Reported by You' : 'Filed Against You'}
                    </h3>
                    <p className="text-sm text-gray-500">
                      Involving <Link to={`/users/${otherUserId}`} className="text-primary-600 hover:underline">{otherUserName}</Link>
                    </p>
                  </div>
                  <div>
                    {dispute.status === 'Pending' && <span className="flex items-center gap-1 text-sm font-medium px-2.5 py-1 rounded-full bg-orange-100 text-orange-800"><Clock size={16}/> Pending Review</span>}
                    {dispute.status === 'Resolved' && <span className="flex items-center gap-1 text-sm font-medium px-2.5 py-1 rounded-full bg-green-100 text-green-800"><CheckCircle size={16}/> Resolved</span>}
                    {dispute.status === 'Dismissed' && <span className="flex items-center gap-1 text-sm font-medium px-2.5 py-1 rounded-full bg-gray-100 text-gray-800"><XCircle size={16}/> Dismissed</span>}
                  </div>
                </div>

                <div className="bg-red-50 text-red-900 p-4 rounded-lg text-sm mt-2 border border-red-100">
                  <span className="font-semibold block mb-1">Reason:</span>
                  {dispute.reason}
                </div>

                {dispute.adminNotes && (
                  <div className="bg-blue-50 text-blue-900 p-4 rounded-lg text-sm border border-blue-100">
                    <span className="font-semibold block mb-1">Admin Notes:</span>
                    {dispute.adminNotes}
                  </div>
                )}
                
                <p className="text-xs text-gray-400 text-right mt-1">
                  Filed on {new Date(dispute.createdAt).toLocaleDateString()}
                </p>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}