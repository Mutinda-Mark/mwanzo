import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { login } from "../api/auth.api";
import { getApiError } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import Field from "../components/Field";
import ErrorBox from "../components/ErrorBox";

export default function Login() {
  const nav = useNavigate();
  const { loginWithToken } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState("");

  const m = useMutation({
    mutationFn: login,
    onSuccess: (data) => {
      // IMPORTANT: your backend may return token as "token" or "accessToken"
      const token = data?.token || data?.accessToken || data?.access_token;
      if (!token) {
        setErr("Login succeeded but token was not found in response. Check backend response shape.");
        return;
      }
      loginWithToken(token);
      nav("/dashboard");
    },
    onError: (e) => setErr(getApiError(e)),
  });

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white border rounded-2xl shadow-sm p-5">
        <h1 className="text-xl font-semibold mb-1">Login</h1>
        <p className="text-sm text-slate-600 mb-4">Use your Mwanzo credentials.</p>

        <ErrorBox message={err} />

        <div className="space-y-3 mt-3">
          <Field label="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
          <Field
            label="Password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />

          <button
            onClick={() => m.mutate({ email, password })}
            disabled={m.isPending}
            className="w-full px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
          >
            {m.isPending ? "Signing in..." : "Sign in"}
          </button>

          <div className="text-sm text-slate-600">
            No account?{" "}
            <Link to="/register" className="text-slate-900 underline">
              Register
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
