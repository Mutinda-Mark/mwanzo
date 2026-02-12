import React, { useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import {
  getAdminUsers,
  updateAdminUser,
  deleteAdminUser,
} from "../api/admin.api";
import { useAuth } from "../auth/AuthContext";
import { getApiError } from "../api/client";
import Spinner from "../components/Spinner";
import ErrorBox from "../components/ErrorBox";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import Field from "../components/Field";

function pick(obj, keys, fallback = null) {
  for (const k of keys) if (obj && obj[k] != null) return obj[k];
  return fallback;
}

const ROLE_OPTIONS = ["Admin", "Teacher", "Student", "Parent"];

export default function Admin() {
  const { role } = useAuth();
  const [err, setErr] = useState("");

  // search
  const [q, setQ] = useState("");

  // edit modal
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState(null);

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [admissionNumber, setAdmissionNumber] = useState("");
  const [userName, setUserName] = useState("");
  const [userRole, setUserRole] = useState("");

  const usersQ = useQuery({
    queryKey: ["adminUsers", q],
    queryFn: () => getAdminUsers(q),
    enabled: role === "Admin",
  });

  const updateM = useMutation({
    mutationFn: async () => {
      if (!editing?.id) throw new Error("No user selected.");

      // send only what your API expects
      const payload = {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        admissionNumber: admissionNumber.trim() ? admissionNumber.trim() : null,
        userName: userName.trim() ? userName.trim() : null,
        role: userRole ? userRole : null,
      };

      return updateAdminUser(editing.id, payload);
    },
    onSuccess: async () => {
      setErr("");
      setOpen(false);
      setEditing(null);
      await usersQ.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const deleteM = useMutation({
    mutationFn: (id) => deleteAdminUser(id),
    onSuccess: async () => {
      setErr("");
      await usersQ.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const users = Array.isArray(usersQ.data) ? usersQ.data : [];

  const columns = useMemo(
    () => [
      { key: "id", header: "User ID", render: (u) => pick(u, ["id", "Id"], "—") },
      { key: "firstName", header: "First Name", render: (u) => pick(u, ["firstName", "FirstName"], "—") },
      { key: "lastName", header: "Last Name", render: (u) => pick(u, ["lastName", "LastName"], "—") },
      { key: "admissionNumber", header: "Admission #", render: (u) => pick(u, ["admissionNumber", "AdmissionNumber"], "—") || "—" },
      { key: "userName", header: "UserName", render: (u) => pick(u, ["userName", "UserName"], "—") },
      { key: "role", header: "Role", render: (u) => pick(u, ["role", "Role"], "—") },
      {
        key: "actions",
        header: "Action",
        render: (u) => {
          const id = pick(u, ["id", "Id"], "");
          return (
            <div className="flex gap-2">
              <button
                className="px-3 py-1 rounded-lg border hover:bg-slate-50"
                onClick={() => {
                  setErr("");
                  const dto = {
                    id,
                    firstName: pick(u, ["firstName", "FirstName"], ""),
                    lastName: pick(u, ["lastName", "LastName"], ""),
                    admissionNumber: pick(u, ["admissionNumber", "AdmissionNumber"], ""),
                    userName: pick(u, ["userName", "UserName"], ""),
                    role: pick(u, ["role", "Role"], ""),
                  };
                  setEditing(dto);

                  setFirstName(dto.firstName || "");
                  setLastName(dto.lastName || "");
                  setAdmissionNumber(dto.admissionNumber || "");
                  setUserName(dto.userName || "");
                  setUserRole(dto.role || "");

                  setOpen(true);
                }}
              >
                Edit
              </button>

              <button
                className="px-3 py-1 rounded-lg border border-red-200 text-red-700 hover:bg-red-50 disabled:opacity-60"
                onClick={() => {
                  if (!id) return;
                  if (!confirm("Delete this user? This may also remove linked Teacher/Student records.")) return;
                  deleteM.mutate(id);
                }}
                disabled={deleteM.isPending}
              >
                Delete
              </button>
            </div>
          );
        },
      },
    ],
    [deleteM.isPending]
  );

  if (role !== "Admin") {
    return (
      <div className="bg-white border rounded-2xl p-4">
        <div className="text-lg font-semibold">System Users</div>
        <div className="text-sm text-slate-600 mt-1">You must be an Admin to view this page.</div>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4">
        <div className="flex flex-col sm:flex-row sm:items-end gap-3 justify-between">
          <div>
            <div className="text-lg font-semibold">System Users</div>
            <div className="text-sm text-slate-600">
              View, search, edit, and delete registered users.
            </div>
          </div>

          <div className="w-full sm:w-96">
            <Field
              label="Search"
              value={q}
              onChange={(e) => setQ(e.target.value)}
              placeholder="name, username, admission, role..."
            />
          </div>
        </div>
      </div>

      <ErrorBox message={err} />

      {usersQ.isLoading && <Spinner />}
      {usersQ.isError && <ErrorBox message={getApiError(usersQ.error)} />}

      <DataTable rows={users} columns={columns} />

      <Modal
        open={open}
        title={editing ? `Edit User` : "Edit User"}
        onClose={() => setOpen(false)}
      >
        <div className="space-y-3">
          <div className="text-xs text-slate-500">
            User ID: <span className="font-mono">{editing?.id ?? "—"}</span>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <Field label="First Name" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
            <Field label="Last Name" value={lastName} onChange={(e) => setLastName(e.target.value)} />
            <Field label="Admission Number" value={admissionNumber} onChange={(e) => setAdmissionNumber(e.target.value)} placeholder="optional" />
            <Field label="UserName" value={userName} onChange={(e) => setUserName(e.target.value)} placeholder="optional" />
          </div>

          <label className="block">
            <div className="text-sm text-slate-700 mb-1">Role</div>
            <select
              className="w-full px-3 py-2 border rounded-lg bg-white"
              value={userRole}
              onChange={(e) => setUserRole(e.target.value)}
            >
              <option value="">-- keep current --</option>
              {ROLE_OPTIONS.map((r) => (
                <option key={r} value={r}>
                  {r}
                </option>
              ))}
            </select>
          </label>

          <button
            className="w-full px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
            onClick={() => updateM.mutate()}
            disabled={!firstName.trim() || !lastName.trim() || updateM.isPending}
          >
            {updateM.isPending ? "Saving..." : "Save Changes"}
          </button>
        </div>
      </Modal>
    </div>
  );
}
