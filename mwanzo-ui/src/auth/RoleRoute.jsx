import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

export default function RoleRoute({ allow = [], children }) {
  const { role } = useAuth();
  if (!role) return <Navigate to="/dashboard" replace />;
  if (!allow.includes(role)) return <Navigate to="/dashboard" replace />;
  return children;
}
