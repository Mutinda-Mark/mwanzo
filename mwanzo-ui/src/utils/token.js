import { jwtDecode } from "jwt-decode";

const TOKEN_KEY = "mwanzo_token";

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
};

export function safeDecodeJwt(token) {
  try {
    return jwtDecode(token);
  } catch {
    return null;
  }
}

export function isExpired(token) {
  const decoded = safeDecodeJwt(token);
  if (!decoded?.exp) return true;
  const now = Math.floor(Date.now() / 1000);
  return decoded.exp <= now;
}

// Tries to extract a role claim from common JWT formats
export function getRole(decoded) {
  if (!decoded) return null;

  // common: "role" or "roles"
  if (decoded.role) return decoded.role;
  if (Array.isArray(decoded.roles)) return decoded.roles[0];

  // ASP.NET sometimes uses this claim:
  const msRole = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
  if (Array.isArray(msRole)) return msRole[0];
  if (typeof msRole === "string") return msRole;

  return null;
}
