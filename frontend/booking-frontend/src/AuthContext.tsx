import { createContext, useContext, useState, type ReactNode } from 'react';
import { isAuthenticated, getUserInfo, logout as authLogout, loginAsDemo as authLoginAsDemo, type UserInfo } from './auth';

interface AuthContextValue {
  authed: boolean;
  user: UserInfo | null;
  login: () => void;        // called after token is stored
  logout: () => void;
  loginAsDemo: () => void;
}

const AuthContext = createContext<AuthContextValue>({
  authed: false,
  user: null,
  login: () => {},
  logout: () => {},
  loginAsDemo: () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [authed, setAuthed] = useState(isAuthenticated);
  const [user, setUser]     = useState<UserInfo | null>(getUserInfo);

  function login() {
    setAuthed(isAuthenticated());
    setUser(getUserInfo());
  }

  function logout() {
    authLogout();
    setAuthed(false);
    setUser(null);
  }

  function loginAsDemo() {
    authLoginAsDemo();
    setAuthed(true);
    setUser(getUserInfo());
  }

  return (
    <AuthContext.Provider value={{ authed, user, login, logout, loginAsDemo }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
