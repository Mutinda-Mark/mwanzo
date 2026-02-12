import React, { useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { register } from "../api/auth.api";
import { getApiError } from "../api/client";
import Field from "../components/Field";
import ErrorBox from "../components/ErrorBox";
import Modal from "../components/Modal";

function pick(obj, keys, fallback = null) {
  for (const k of keys) if (obj && obj[k] != null) return obj[k];
  return fallback;
}

function isUrl(s) {
  if (!s) return false;
  try {
    new URL(String(s));
    return true;
  } catch {
    return false;
  }
}

export default function Register() {
  const nav = useNavigate();

  const [form, setForm] = useState({
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    role: "Teacher", // ✅ default so it never submits empty role
    admissionNumber: "",
  });

  const [err, setErr] = useState("");

  // popup
  const [open, setOpen] = useState(false);
  const [regData, setRegData] = useState(null);

  const set = (k, v) => setForm((p) => ({ ...p, [k]: v }));

  const extracted = useMemo(() => {
    const d = regData || {};
    const userId = pick(d, ["userId", "UserId", "id", "Id"], "");
    const token = pick(d, ["token", "Token", "confirmationToken", "ConfirmationToken"], "");

    // supports confirmLink + typo confrimLink
    const link =
      pick(
        d,
        [
          "confirmLink",
          "ConfirmLink",
          "confirmationLink",
          "ConfirmationLink",
          "confirmEmailLink",
          "ConfirmEmailLink",
          "confrimLink",
          "ConfrimLink",
          "link",
          "Link",
        ],
        ""
      ) || (isUrl(d) ? d : "");

    return { userId, token, link };
  }, [regData]);

  const m = useMutation({
    mutationFn: register,
    onSuccess: (data) => {
      setErr("");
      setRegData(data);
      setOpen(true);
      console.log("register response:", data);
    },
    onError: (e) => setErr(getApiError(e)),
  });

  // ✅ NEW: confirm in new tab, then redirect this tab to login
  const confirmAndGoLogin = () => {
    if (!extracted.link) return;

    window.open(extracted.link, "_blank", "noopener,noreferrer");
    setOpen(false);
    nav("/login");
  };

  // ✅ IMPORTANT: send payload exactly like Swagger expects
  const submit = () => {
    const payload = {
      email: String(form.email || "").trim(),
      password: String(form.password || ""),
      firstName: String(form.firstName || "").trim(),
      lastName: String(form.lastName || "").trim(),
      role: String(form.role || "Student").trim(), // "Teacher" or "Student"
      admissionNumber:
        String(form.role || "").trim() === "Student"
          ? (String(form.admissionNumber || "").trim() || null)
          : null,
    };

    console.log("REGISTER payload (sending):", payload);
    m.mutate(payload);
  };

  const canSubmit =
    !!String(form.email || "").trim() &&
    !!String(form.password || "") &&
    !!String(form.firstName || "").trim() &&
    !!String(form.lastName || "").trim() &&
    !!String(form.role || "").trim() &&
    (form.role !== "Student" || !!String(form.admissionNumber || "").trim());

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white border rounded-2xl shadow-sm p-5">
        <h1 className="text-xl font-semibold mb-1">Register</h1>
        <p className="text-sm text-slate-600 mb-4">Creates a user account (role-based).</p>

        <ErrorBox message={err} />

        <div className="space-y-3 mt-3">
          <Field label="Email" value={form.email} onChange={(e) => set("email", e.target.value)} />
          <Field
            label="Password"
            type="password"
            value={form.password}
            onChange={(e) => set("password", e.target.value)}
          />
          <Field label="First Name" value={form.firstName} onChange={(e) => set("firstName", e.target.value)} />
          <Field label="Last Name" value={form.lastName} onChange={(e) => set("lastName", e.target.value)} />

          <label className="block">
            <div className="text-sm text-slate-700 mb-1">Role</div>
            <select
              value={form.role}
              onChange={(e) => set("role", e.target.value)}
              className="w-full px-3 py-2 border rounded-lg bg-white"
            >
              <option value="Teacher">Teacher</option>
              <option value="Student">Student</option>
            </select>
          </label>

          {form.role === "Student" && (
            <Field
              label="Admission Number"
              value={form.admissionNumber}
              onChange={(e) => set("admissionNumber", e.target.value)}
              placeholder="e.g. STU001"
            />
          )}

          <button
            onClick={submit}
            disabled={!canSubmit || m.isPending}
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
        </div>
      </div>

      {/* ✅ Success Popup */}
      <Modal open={open} title="Registration Successful" onClose={() => setOpen(false)}>
        <div className="space-y-3">
          <div className="text-sm text-slate-700">User registered successfully! Press confirm to proceed.</div>

          {extracted.link ? (
            <div className="border rounded-xl p-3 bg-slate-50 space-y-2">
              <div className="text-sm font-semibold">Email Confirmation</div>

              <div className="text-xs text-slate-700">
                Confirmation link is ready.{" "}
                <span className="text-slate-500">
                  (Preview: <span className="font-mono">{String(extracted.link).slice(0, 55)}...</span>)
                </span>
              </div>

              <button
                type="button"
                onClick={confirmAndGoLogin}
                className="w-full px-3 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90"
              >
                Confirm Email
              </button>
            </div>
          ) : (
            <div className="border rounded-xl p-3 bg-yellow-50 border-yellow-200 text-yellow-900 text-sm">
              No confirmation link returned by backend. Check console log for `register response`.
            </div>
          )}

          <div className="flex gap-2">
            <button
              className="flex-1 px-4 py-2 rounded-lg border hover:bg-slate-50"
              onClick={() => setOpen(false)}
            >
              Close
            </button>

            <Link
              to="/login"
              className="flex-1 text-center px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90"
            >
              Go to Login
            </Link>
          </div>
        </div>
      </Modal>
    </div>
  );
}
