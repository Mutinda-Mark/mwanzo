import React, { useState } from "react";
import { Link } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { register } from "../api/auth.api";
import { getApiError } from "../api/client";
import Field from "../components/Field";
import ErrorBox from "../components/ErrorBox";

export default function Register() {
  const [form, setForm] = useState({
    email: "admin@agile.com",
    password: "Admin@123",
    firstName: "Admin",
    lastName: "User",
    role: "Admin",
    admissionNumber: null,
  });
  const [msg, setMsg] = useState("");
  const [err, setErr] = useState("");

  const m = useMutation({
    mutationFn: register,
    onSuccess: (data) => {
      setErr("");
      setMsg(
        "Registered. If your backend requires email confirmation, check the email/response and use the Confirm Email screen."
      );
      console.log("register response:", data);
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const set = (k, v) => setForm((p) => ({ ...p, [k]: v }));

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white border rounded-2xl shadow-sm p-5">
        <h1 className="text-xl font-semibold mb-1">Register</h1>
        <p className="text-sm text-slate-600 mb-4">Creates a user account (role-based).</p>

        <ErrorBox message={err} />
        {msg && <div className="p-3 rounded-lg bg-green-50 text-green-800 border border-green-200">{msg}</div>}

        <div className="space-y-3 mt-3">
          <Field label="Email" value={form.email} onChange={(e) => set("email", e.target.value)} />
          <Field label="Password" type="password" value={form.password} onChange={(e) => set("password", e.target.value)} />
          <Field label="First Name" value={form.firstName} onChange={(e) => set("firstName", e.target.value)} />
          <Field label="Last Name" value={form.lastName} onChange={(e) => set("lastName", e.target.value)} />

          <label className="block">
            <div className="text-sm text-slate-700 mb-1">Role</div>
            <select
              value={form.role}
              onChange={(e) => set("role", e.target.value)}
              className="w-full px-3 py-2 border rounded-lg bg-white"
            >
              <option>Admin</option>
              <option>Teacher</option>
              <option>Student</option>
            </select>
          </label>

          {form.role === "Student" && (
            <Field
              label="Admission Number"
              value={form.admissionNumber ?? ""}
              onChange={(e) => set("admissionNumber", e.target.value || null)}
            />
          )}

          <button
            onClick={() => m.mutate(form)}
            disabled={m.isPending}
            className="w-full px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
          >
            {m.isPending ? "Creating..." : "Create account"}
          </button>

          <div className="text-sm text-slate-600">
            Already registered?{" "}
            <Link to="/login" className="text-slate-900 underline">
              Login
            </Link>
          </div>

          <div className="text-sm text-slate-600">
            Need to confirm email?{" "}
            <Link to="/confirm-email" className="text-slate-900 underline">
              Confirm Email
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
