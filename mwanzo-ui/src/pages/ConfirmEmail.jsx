import React, { useEffect, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { confirmEmail } from "../api/auth.api";
import { getApiError } from "../api/client";
import Field from "../components/Field";
import ErrorBox from "../components/ErrorBox";
import { Link, useLocation } from "react-router-dom";

function pick(obj, keys, fallback = null) {
  for (const k of keys) if (obj && obj[k] != null) return obj[k];
  return fallback;
}

export default function ConfirmEmail() {
  const location = useLocation();
  const prefill = location.state || {};

  const [userId, setUserId] = useState("");
  const [token, setToken] = useState("");
  const [msg, setMsg] = useState("");
  const [err, setErr] = useState("");

  useEffect(() => {
    // Accept multiple possible names to match any backend response shape
    const uid = pick(prefill, ["userId", "UserId", "id", "Id"], "");
    const tok = pick(prefill, ["token", "Token", "confirmationToken", "ConfirmationToken"], "");
    if (uid) setUserId(String(uid));
    if (tok) setToken(String(tok));
  }, [prefill]);

  const m = useMutation({
    mutationFn: () => confirmEmail(userId, token),
    onSuccess: (data) => {
      setErr("");
      setMsg(typeof data === "string" ? data : "Email confirmed.");
    },
    onError: (e) => setErr(getApiError(e)),
  });

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white border rounded-2xl shadow-sm p-5">
        <h1 className="text-xl font-semibold mb-1">Confirm Email</h1>
        <p className="text-sm text-slate-600 mb-4">
          Paste userId + token from backend/email — or arrive here from Register popup with auto-filled values.
        </p>

        <ErrorBox message={err} />
        {msg && (
          <div className="p-3 rounded-lg bg-green-50 text-green-800 border border-green-200">{msg}</div>
        )}

        <div className="space-y-3 mt-3">
          <Field label="User ID" value={userId} onChange={(e) => setUserId(e.target.value)} />
          <Field label="Token" value={token} onChange={(e) => setToken(e.target.value)} />

          <button
            onClick={() => m.mutate()}
            disabled={m.isPending || !userId || !token}
            className="w-full px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
          >
            {m.isPending ? "Confirming..." : "Confirm"}
          </button>

          <div className="text-sm text-slate-600">
            Back to{" "}
            <Link to="/login" className="text-slate-900 underline">
              Login
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
