import { useState } from 'react';
import { useCreateReview } from '@/hooks/useReviews';
import { Star, X } from 'lucide-react';

interface ReviewModalProps {
    swapRequestId: number;
    otherUserName: string;
    onClose: () => void;
}

export default function ReviewModal({ swapRequestId, otherUserName, onClose }: ReviewModalProps) {
    const [score, setScore] = useState(0);
    const [hoveredStar, setHoveredStar] = useState(0);
    const [comment, setComment] = useState('');
    const createReview = useCreateReview();

    const handleSubmit = () => {
        if (score < 1) return;
        createReview.mutate(
            { swapRequestId, score, comment: comment.trim() || undefined },
            { onSuccess: onClose }
        );
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
            <div className="bg-white rounded-xl shadow-lg w-full max-w-md p-6 relative">
                <button onClick={onClose} className="absolute top-3 right-3 text-gray-400 hover:text-gray-600">
                    <X size={20} />
                </button>

                <h2 className="text-lg font-semibold text-gray-900 mb-1">Leave a Review</h2>
                <p className="text-sm text-gray-500 mb-5">Rate your experience with {otherUserName}</p>

                {/* Star rating */}
                <div className="flex items-center gap-1 mb-4">
                    {Array.from({ length: 5 }).map((_, i) => {
                        const starValue = i + 1;
                        return (
                            <button
                                key={i}
                                type="button"
                                onClick={() => setScore(starValue)}
                                onMouseEnter={() => setHoveredStar(starValue)}
                                onMouseLeave={() => setHoveredStar(0)}
                                className="p-0.5 transition-transform hover:scale-110"
                            >
                                <Star
                                    size={28}
                                    className={
                                        starValue <= (hoveredStar || score)
                                            ? 'text-yellow-400'
                                            : 'text-gray-200'
                                    }
                                    fill={starValue <= (hoveredStar || score) ? 'currentColor' : 'none'}
                                />
                            </button>
                        );
                    })}
                    {score > 0 && (
                        <span className="ml-2 text-sm text-gray-500">{score}/5</span>
                    )}
                </div>

                {/* Comment */}
                <textarea
                    value={comment}
                    onChange={(e) => setComment(e.target.value)}
                    placeholder="Share your experience (optional)..."
                    rows={4}
                    maxLength={1000}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500 outline-none resize-none"
                />

                <div className="flex justify-end gap-2 mt-4">
                    <button
                        onClick={onClose}
                        className="px-4 py-2 text-sm text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={handleSubmit}
                        disabled={score < 1 || createReview.isPending}
                        className="px-4 py-2 text-sm bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
                    >
                        {createReview.isPending ? 'Submitting...' : 'Submit Review'}
                    </button>
                </div>

                {createReview.isError && (
                    <p className="text-sm text-red-600 mt-2">
                        {(createReview.error as any)?.response?.data?.message || 'Failed to submit review.'}
                    </p>
                )}
            </div>
        </div>
    );
}
