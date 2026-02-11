import React, { createContext, useContext, useMemo, useState, useEffect } from "react";
import { tokenStore, safeDecodeJwt, getRole, isExpired } from "../utils/token";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => tokenStore.get() || null);

  useEffect(() => {
    if (token && isExpired(token)) {
      tokenStore.clear();
      setToken(null);
    }
  }, [token]);

  const decoded = useMemo(() => (token ? safeDecodeJwt(token) : null), [token]);
  const role = useMemo(() => getRole(decoded), [decoded]);

  const user = useMemo(() => {
    if (!decoded) return null;
    return {
      email: decoded.email || decoded.unique_name || decoded.sub || null,
      role,
      claims: decoded,
    };
  }, [decoded, role]);

  const loginWithToken = (jwt) => {
    tokenStore.set(jwt);
    setToken(jwt);
  };

  const logout = () => {
    tokenStore.clear();
    setToken(null);
  };

  const value = { token, user, role, isAuthed: !!token && !isExpired(token), loginWithToken, logout };
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
