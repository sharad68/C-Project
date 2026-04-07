/* eslint-disable react-refresh/only-export-components */
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from "react";
import { api, type AdminIdentity } from "../lib/api";

interface AuthSession {
  token: string;
  user: AdminIdentity;
  expiresAtUtc: string;
}

interface AuthContextValue {
  session: AuthSession | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (userName: string, password: string) => Promise<void>;
  logout: () => void;
}

const STORAGE_KEY = "tornois.admin.session";

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const restoreSession = async () => {
      const stored = window.localStorage.getItem(STORAGE_KEY);
      if (!stored) {
        setIsLoading(false);
        return;
      }

      try {
        const parsed = JSON.parse(stored) as AuthSession;
        if (new Date(parsed.expiresAtUtc).getTime() <= Date.now()) {
          window.localStorage.removeItem(STORAGE_KEY);
          setIsLoading(false);
          return;
        }

        const user = await api.getAdminMe(parsed.token);
        const hydrated = { ...parsed, user };
        setSession(hydrated);
        window.localStorage.setItem(STORAGE_KEY, JSON.stringify(hydrated));
      } catch {
        window.localStorage.removeItem(STORAGE_KEY);
      } finally {
        setIsLoading(false);
      }
    };

    void restoreSession();
  }, []);

  const login = useCallback(async (userName: string, password: string) => {
    const result = await api.login(userName, password);
    const nextSession: AuthSession = {
      token: result.token,
      user: result.user,
      expiresAtUtc: result.expiresAtUtc,
    };

    setSession(nextSession);
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(nextSession));
  }, []);

  const logout = useCallback(() => {
    setSession(null);
    window.localStorage.removeItem(STORAGE_KEY);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      isAuthenticated: session !== null,
      isLoading,
      login,
      logout,
    }),
    [isLoading, login, logout, session],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
}
