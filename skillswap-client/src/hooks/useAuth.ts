import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { authService, LoginDto, RegisterDto } from '@/services/authService';
import { useAuthStore } from '@/stores/authStore';

export function useLogin() {
  const setAuth = useAuthStore((s) => s.setAuth);
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (dto: LoginDto) => authService.login(dto),
    onSuccess: ({ data }) => {
      setAuth(data);
      navigate('/');
    },
  });
}

export function useRegister() {
  const setAuth = useAuthStore((s) => s.setAuth);
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (dto: RegisterDto) => authService.register(dto),
    onSuccess: ({ data }) => {
      setAuth(data);
      navigate('/');
    },
  });
}
