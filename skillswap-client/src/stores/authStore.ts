import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface AuthState {
  token: string | null;
  userId: string | null;
  email: string | null;
  fullName: string | null;
  role: string | null;
  isAuthenticated: boolean;
  lastSeenSwapsAt: string | null;
  lastSeenSwapsByUser: Record<string, string>;
  setAuth: (data: { token: string; userId: string; email: string; fullName: string; role: string }) => void;
  setLastSeenSwapsAt: (value: string) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      userId: null,
      email: null,
      fullName: null,
      role: null,
      isAuthenticated: false,
      lastSeenSwapsAt: null,
      lastSeenSwapsByUser: {},
      setAuth: (data) => {
        const { lastSeenSwapsByUser, lastSeenSwapsAt: storedLastSeen } = get();
        const hasPerUserMap = Object.keys(lastSeenSwapsByUser).length > 0;
        const lastSeenSwapsAt =
          lastSeenSwapsByUser[data.userId] ?? (hasPerUserMap ? null : storedLastSeen);
        set({
          token: data.token,
          userId: data.userId,
          email: data.email,
          fullName: data.fullName,
          role: data.role,
          isAuthenticated: true,
          lastSeenSwapsAt,
        });
      },
      setLastSeenSwapsAt: (value) => {
        const userId = get().userId;
        if (!userId) {
          set({ lastSeenSwapsAt: value });
          return;
        }
        set((state) => ({
          lastSeenSwapsAt: value,
          lastSeenSwapsByUser: { ...state.lastSeenSwapsByUser, [userId]: value },
        }));
      },
      logout: () =>
        set({
          token: null,
          userId: null,
          email: null,
          fullName: null,
          role: null,
          isAuthenticated: false,
          lastSeenSwapsAt: null,
        }),
    }),
    { name: 'SkillSwap-auth' }
  )
);
