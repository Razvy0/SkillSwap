import { useState } from 'react';
import { Shield, Clock, CheckCircle, XCircle, Gavel, AlertTriangle } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useAdminDisputes, useResolveDispute, DisputeAction } from '@/hooks/useAdminDisputes';

export default function AdminDisputesPage() {
    const { data: disputes, isLoading } = useAdminDisputes();
    const resolveMutation = useResolveDispute();

    const [selectedDisputeId, setSelectedDisputeId] = useState<number | null>(null);
    const [action, setAction] = useState<DisputeAction>(DisputeAction.Dismiss);
    const [adminNotes, setAdminNotes] = useState('');

    if (isLoading) return <p className="text-gray-500">Loading admin dashboard...</p>;

    const handleResolve = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedDisputeId) return;

        try {
            await resolveMutation.mutateAsync({
                disputeId: selectedDisputeId,
                action: Number(action),
                adminNotes
            });

            setSelectedDisputeId(null);
            setAdminNotes('');
            setAction(DisputeAction.Dismiss);
        } catch (error) {
            console.error('Failed to resolve dispute:', error);
        }
    };

    return (
        <div className="max-w-5xl mx-auto">
            <div className="flex items-center gap-3 mb-8 pb-4 border-b border-gray-200">
                <div className="p-3 bg-purple-100 text-purple-700 rounded-lg">
                    <Shield size={24} />
                </div>
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Admin Panel</h1>
                    <p className="text-sm text-gray-500">Manage and resolve platform disputes</p>
                </div>
            </div>

            {(!disputes || disputes.length === 0) ? (
                <div className="bg-white rounded-xl border border-gray-200 p-8 text-center text-gray-500 flex flex-col items-center gap-2">
                    <CheckCircle className="text-green-500" size={32} />
                    <p>Inbox zero! No disputes require your attention.</p>
                </div>
            ) : (
                <div className="space-y-4">
                    {disputes.map((dispute: any) => (
                        <div key={dispute.id} className="bg-white rounded-xl border border-gray-200 p-6 flex flex-col gap-4 shadow-sm hover:shadow transition-shadow">

                            <div className="flex justify-between items-start">
                                <div className="space-y-1">
                                    <span className="text-xs font-bold uppercase tracking-wider px-2.5 py-1 rounded-md bg-gray-100 text-gray-600 border border-gray-200">
                                        Swap #{dispute.swapRequestId}
                                    </span>
                                    <div className="mt-3 flex items-center gap-2 text-sm text-gray-600">
                                        <span className="font-semibold text-gray-900">Reporter:</span>
                                        <Link to={`/users/${dispute.reporterId}`} className="text-primary-600 hover:underline">
                                            {dispute.reporterName}
                                        </Link>
                                        <span className="mx-2 text-gray-300">|</span>
                                        <span className="font-semibold text-gray-900">Reported:</span>
                                        <Link to={`/users/${dispute.reportedUserId}`} className="text-primary-600 hover:underline">
                                            {dispute.reportedUserName}
                                        </Link>
                                    </div>
                                </div>

                                <div className="flex items-center gap-3">
                                    {dispute.status === 'Pending' && <span className="flex items-center gap-1 text-sm font-medium px-3 py-1 rounded-full bg-orange-100 text-orange-800"><Clock size={16} /> Pending</span>}
                                    {dispute.status === 'Resolved' && <span className="flex items-center gap-1 text-sm font-medium px-3 py-1 rounded-full bg-green-100 text-green-800"><CheckCircle size={16} /> Resolved</span>}
                                    {dispute.status === 'Dismissed' && <span className="flex items-center gap-1 text-sm font-medium px-3 py-1 rounded-full bg-gray-100 text-gray-800"><XCircle size={16} /> Dismissed</span>}

                                    {dispute.status === 'Pending' && (
                                        <button
                                            onClick={() => setSelectedDisputeId(dispute.id)}
                                            className="flex items-center gap-2 bg-purple-600 hover:bg-purple-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors"
                                        >
                                            <Gavel size={16} /> Resolve
                                        </button>
                                    )}
                                </div>
                            </div>

                            <div className="bg-red-50 text-red-900 p-4 rounded-lg text-sm border border-red-100">
                                <span className="font-semibold flex items-center gap-2 mb-1">
                                    <AlertTriangle size={16} /> Dispute Reason:
                                </span>
                                {dispute.reason}
                            </div>

                            {dispute.adminNotes && (
                                <div className="bg-purple-50 text-purple-900 p-4 rounded-lg text-sm border border-purple-100">
                                    <span className="font-semibold block mb-1">Admin Resolution Notes:</span>
                                    {dispute.adminNotes}
                                </div>
                            )}

                            <p className="text-xs text-gray-400 text-right">
                                Filed on {new Date(dispute.createdAt).toLocaleDateString()}
                            </p>
                        </div>
                    ))}
                </div>
            )}

            {selectedDisputeId !== null && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden">
                        <div className="px-6 py-4 border-b border-gray-100 flex justify-between items-center bg-gray-50">
                            <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                                <Gavel className="text-purple-600" size={20} />
                                Resolve Dispute #{selectedDisputeId}
                            </h3>
                        </div>

                        <form onSubmit={handleResolve} className="p-6 space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Action to Take</label>
                                <select
                                    value={action}
                                    onChange={(e) => setAction(Number(e.target.value))}
                                    className="w-full border border-gray-300 rounded-lg p-2.5 text-sm focus:ring-purple-500 focus:border-purple-500 outline-none"
                                >
                                    <option value={DisputeAction.Dismiss}>Dismiss (No Action)</option>
                                    <option value={DisputeAction.DeleteSwap}>Cancel/Delete Swap</option>
                                    <option value={DisputeAction.BanUser}>Ban Reported User</option>
                                </select>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Admin Notes (Visible to users)</label>
                                <textarea
                                    required
                                    rows={4}
                                    value={adminNotes}
                                    onChange={(e) => setAdminNotes(e.target.value)}
                                    placeholder="Explain why this action was taken..."
                                    className="w-full border border-gray-300 rounded-lg p-3 text-sm focus:ring-purple-500 focus:border-purple-500 outline-none resize-none"
                                />
                            </div>

                            <div className="flex gap-3 pt-2 mt-4">
                                <button
                                    type="button"
                                    onClick={() => setSelectedDisputeId(null)}
                                    className="flex-1 px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg text-sm font-medium transition-colors"
                                >
                                    Cancel
                                </button>
                                <button
                                    type="submit"
                                    disabled={resolveMutation.isPending}
                                    className="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 disabled:bg-purple-400 text-white rounded-lg text-sm font-medium transition-colors"
                                >
                                    {resolveMutation.isPending ? 'Resolving...' : 'Confirm Resolution'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}