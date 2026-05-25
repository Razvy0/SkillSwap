import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useUserProfile } from '@/hooks/useUsers';
import { useUserReviews, useReviewableSwaps } from '@/hooks/useReviews';
import { useAuthStore } from '@/stores/authStore';
import { Star, ArrowLeft, MessageSquare } from 'lucide-react';
import ReviewModal from '@/components/ReviewModal';
import { ReviewableSwap } from '@/services/reviewService';

export default function UserProfilePage() {
    const { id } = useParams<{ id: string }>();
    const currentUserId = useAuthStore((s) => s.userId);
    const { data: profile, isLoading } = useUserProfile(id!);
    const { data: reviews, isLoading: reviewsLoading } = useUserReviews(id!);
    const { data: reviewableSwaps } = useReviewableSwaps(id!);
    const [reviewSwap, setReviewSwap] = useState<ReviewableSwap | null>(null);

    if (isLoading) return <p className="text-gray-500">Loading profile...</p>;
    if (!profile) return <p className="text-gray-500">User not found.</p>;

    const isOwnProfile = currentUserId === id;
    const offeringSkills = profile.skills.filter((s) => s.isOffering);
    const seekingSkills = profile.skills.filter((s) => !s.isOffering);

    const getSwapDirectionMeta = (swap: ReviewableSwap) => {
        const lessonType = swap.lessonType ?? 'OneWay';
        if (lessonType === 'Exchange') {
            return {
                badge: 'Skill Exchange',
                direction: 'Both teach each other',
                arrow: '⇄',
            };
        }

        const teacherLabel = swap.teacherId
            ? swap.teacherId === currentUserId
                ? 'You'
                : profile.fullName
            : 'Teacher';
        const learnerLabel = swap.learnerId
            ? swap.learnerId === currentUserId
                ? 'You'
                : profile.fullName
            : 'Learner';

        return {
            badge: 'One-Way Lesson',
            direction: `${teacherLabel} → ${learnerLabel}`,
            arrow: '→',
        };
    };

    return (
        <div className="max-w-3xl mx-auto">
            <Link to="/explore" className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 mb-6">
                <ArrowLeft size={16} /> Back to Explore
            </Link>

            {/* Profile header */}
            <div className="bg-white border border-gray-200 rounded-xl p-6 mb-6">
                <div className="flex justify-between items-start">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">{profile.fullName}</h1>
                        {profile.bio && <p className="text-gray-600 mt-2">{profile.bio}</p>}
                    </div>
                    <div className="text-right">
                        <div className="flex items-center gap-1 text-2xl text-yellow-500 font-bold">
                            {profile.rating > 0 ? profile.rating.toFixed(1) : '—'}
                            <Star size={24} fill="currentColor" />
                        </div>
                        <p className="text-sm text-gray-400 mt-1">
                            {reviews?.length ?? 0} review{reviews?.length !== 1 ? 's' : ''}
                        </p>
                    </div>
                </div>

                {!isOwnProfile && (
                    <div className="mt-4 pt-4 border-t border-gray-100 flex flex-wrap gap-3">
                        <Link
                            to={`/messages?user=${id}`}
                            className="inline-flex items-center gap-2 px-4 py-2 bg-primary-600 text-white text-sm font-medium rounded-lg hover:bg-primary-700"
                        >
                            <MessageSquare size={16} /> Send Message
                        </Link>
                        {reviewableSwaps && reviewableSwaps.length === 1 && (
                            <button
                                onClick={() => setReviewSwap(reviewableSwaps[0])}
                                className="inline-flex items-center gap-2 px-4 py-2 bg-yellow-500 text-white text-sm font-medium rounded-lg hover:bg-yellow-600"
                            >
                                <Star size={16} /> Leave Review
                            </button>
                        )}
                        {reviewableSwaps && reviewableSwaps.length > 1 && (
                            <div className="relative group">
                                <button className="inline-flex items-center gap-2 px-4 py-2 bg-yellow-500 text-white text-sm font-medium rounded-lg hover:bg-yellow-600">
                                    <Star size={16} /> Leave Review ({reviewableSwaps.length})
                                </button>
                                <div className="absolute left-0 top-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg z-10 hidden group-hover:block min-w-[260px]">
                                    {reviewableSwaps.map((s) => {
                                        const meta = getSwapDirectionMeta(s);
                                        const isExchange = (s.lessonType ?? 'OneWay') === 'Exchange';
                                        return (
                                            <button
                                                key={s.swapId}
                                                onClick={() => setReviewSwap(s)}
                                                className="w-full text-left px-4 py-2.5 text-sm hover:bg-gray-50 border-b border-gray-100 last:border-0"
                                            >
                                                <div className="flex items-center gap-1.5">
                                                    {isExchange ? (
                                                        <>
                                                            <span className="font-medium text-gray-900">{s.offeredSkillTitle}</span>
                                                            <span className="text-gray-400">{meta.arrow}</span>
                                                            <span className="font-medium text-gray-900">{s.requestedSkillTitle}</span>
                                                        </>
                                                    ) : (
                                                        <>
                                                            <span className="font-medium text-gray-900">Lesson: {s.requestedSkillTitle}</span>
                                                        </>
                                                    )}
                                                </div>
                                                <div className="mt-1 flex items-center gap-2 text-xs text-gray-500">
                                                    <span
                                                        className={
                                                            meta.badge === 'Skill Exchange'
                                                                ? 'rounded-full bg-indigo-100 px-2 py-0.5 text-indigo-700'
                                                                : 'rounded-full bg-emerald-100 px-2 py-0.5 text-emerald-700'
                                                        }
                                                    >
                                                        {meta.badge}
                                                    </span>
                                                    <span>{meta.direction}</span>
                                                </div>
                                            </button>
                                        );
                                    })}
                                </div>
                            </div>
                        )}
                    </div>
                )}
            </div>

            {/* Skills */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                {offeringSkills.length > 0 && (
                    <div className="bg-white border border-gray-200 rounded-xl p-5">
                        <h2 className="text-lg font-semibold text-gray-900 mb-3">Skills Offered</h2>
                        <div className="space-y-2">
                            {offeringSkills.map((s) => (
                                <div key={s.id} className="flex items-center justify-between">
                                    <span className="text-sm font-medium text-gray-800">{s.title}</span>
                                    <span className="text-xs px-2 py-0.5 rounded-full bg-green-100 text-green-700">{s.categoryName}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {seekingSkills.length > 0 && (
                    <div className="bg-white border border-gray-200 rounded-xl p-5">
                        <h2 className="text-lg font-semibold text-gray-900 mb-3">Skills Seeking</h2>
                        <div className="space-y-2">
                            {seekingSkills.map((s) => (
                                <div key={s.id} className="flex items-center justify-between">
                                    <span className="text-sm font-medium text-gray-800">{s.title}</span>
                                    <span className="text-xs px-2 py-0.5 rounded-full bg-blue-100 text-blue-700">{s.categoryName}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </div>

            {/* Reviews */}
            <div className="bg-white border border-gray-200 rounded-xl p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Reviews</h2>
                {reviewsLoading ? (
                    <p className="text-gray-500 text-sm">Loading reviews...</p>
                ) : reviews && reviews.length > 0 ? (
                    <div className="space-y-4">
                        {reviews.map((review) => (
                            <div key={review.id} className="border-b border-gray-100 pb-4 last:border-0 last:pb-0">
                                <div className="flex justify-between items-start mb-1">
                                    <div>
                                        <Link
                                            to={`/users/${review.reviewerId}`}
                                            className="text-sm font-semibold text-gray-900 hover:text-primary-600"
                                        >
                                            {review.reviewerName}
                                        </Link>
                                        <span className="text-xs text-gray-400 ml-2">
                                            {new Date(review.createdAt).toLocaleDateString(undefined, {
                                                month: 'short',
                                                day: 'numeric',
                                                year: 'numeric',
                                            })}
                                        </span>
                                    </div>
                                    <div className="flex items-center gap-0.5">
                                        {Array.from({ length: 5 }).map((_, i) => (
                                            <Star
                                                key={i}
                                                size={14}
                                                className={i < review.score ? 'text-yellow-400' : 'text-gray-200'}
                                                fill={i < review.score ? 'currentColor' : 'none'}
                                            />
                                        ))}
                                    </div>
                                </div>
                                {review.comment && (
                                    <p className="text-sm text-gray-600">{review.comment}</p>
                                )}
                            </div>
                        ))}
                    </div>
                ) : (
                    <p className="text-gray-500 text-sm">No reviews yet.</p>
                )}
            </div>

            {reviewSwap && (
                <ReviewModal
                    swapRequestId={reviewSwap.swapId}
                    otherUserName={profile.fullName}
                    onClose={() => setReviewSwap(null)}
                />
            )}
        </div>
    );
}
