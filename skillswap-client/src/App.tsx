import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import Layout from '@/components/Layout';
import LoginPage from '@/pages/LoginPage';
import RegisterPage from '@/pages/RegisterPage';
import DashboardPage from '@/pages/DashboardPage';
import ExplorePage from '@/pages/ExplorePage';
import ProfilePage from '@/pages/ProfilePage';
import SwapsPage from '@/pages/SwapsPage';
import MessagesPage from '@/pages/MessagesPage';
import UserProfilePage from '@/pages/UserProfilePage';
import DisputesPage from '@/pages/DisputesPage';
import RecommendationsPage from '@/pages/RecommendationsPage';

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route
        path="/"
        element={
          <PrivateRoute>
            <Layout />
          </PrivateRoute>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="recommendations" element={<RecommendationsPage />} />
        <Route path="explore" element={<ExplorePage />} />
        <Route path="profile" element={<ProfilePage />} />
        <Route path="swaps" element={<SwapsPage />} />
        <Route path="disputes" element={<DisputesPage />} />
        <Route path="messages" element={<MessagesPage />} />
        <Route path="users/:id" element={<UserProfilePage />} />
      </Route>
    </Routes>
  );
}
