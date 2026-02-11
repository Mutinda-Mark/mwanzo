import React, { useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { assignSubjects, createTeacher, getTeachers } from "../api/teachers.api";
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

function teacherDisplayName(t) {
  // nested user variant
  const user = pick(t, ["user", "User"], null);

  const first =
    pick(user, ["firstName", "FirstName"], null) ??
    pick(t, ["firstName", "FirstName"], null);

  const last =
    pick(user, ["lastName", "LastName"], null) ??
    pick(t, ["lastName", "LastName"], null);

  const full = `${first ?? ""} ${last ?? ""}`.trim();
  if (full) return full;

  // fallback: email if present
  const email =
    pick(user, ["email", "Email"], null) ??
    pick(t, ["email", "Email"], null);

  return email || "—";
}

export default function Teachers() {
  const [err, setErr] = useState("");

  const [createUserId, setCreateUserId] = useState("");

  const [assignOpen, setAssignOpen] = useState(false);
  const [rows, setRows] = useState([{ teacherUserId: "", subjectId: "", classId: "" }]);

  const q = useQuery({ queryKey: ["teachers"], queryFn: getTeachers });

  const createM = useMutation({
    mutationFn: () => createTeacher({ userId: createUserId }),
    onSuccess: () => {
      setErr("");
      setCreateUserId("");
      q.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const assignM = useMutation({
    mutationFn: () =>
      assignSubjects(
        rows
          .filter((r) => r.teacherUserId && r.subjectId && r.classId)
          .map((r) => ({
            teacherId: r.teacherUserId, // controller expects TeacherId = teacher.UserId
            subjectId: Number(r.subjectId),
            classId: Number(r.classId),
          }))
      ),
    onSuccess: () => {
      setErr("");
      setAssignOpen(false);
      q.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const teachers = Array.isArray(q.data) ? q.data : [];

  const columns = useMemo(
    () => [
      { key: "id", header: "Teacher ID", render: (t) => pick(t, ["id", "Id"], "—") },
      { key: "userId", header: "UserId (Guid)", render: (t) => pick(t, ["userId", "UserId"], "—") },
      { key: "name", header: "Name", render: (t) => teacherDisplayName(t) },
      {
        key: "assignments",
        header: "Assignments",
        render: (t) => {
          const list = pick(t, ["subjectAssignments", "SubjectAssignments"], []);
          const items = Array.isArray(list) ? list : [];
          if (!items.length) return "—";

          return (
            <div className="space-y-1">
              {items.map((a, idx) => (
                <div key={idx} className="text-xs">
                  Class: {pick(a, ["className", "ClassName"], pick(pick(a, ["class", "Class"], {}), ["name", "Name"], pick(a, ["classId", "ClassId"], "")))}{" "}
                  • Subject: {pick(a, ["subjectName", "SubjectName"], pick(pick(a, ["subject", "Subject"], {}), ["name", "Name"], pick(a, ["subjectId", "SubjectId"], "")))}
                </div>
              ))}
            </div>
          );
        },
      },
    ],
    []
  );

  const addRow = () => setRows((p) => [...p, { teacherUserId: "", subjectId: "", classId: "" }]);
  const setRow = (i, k, v) => setRows((p) => p.map((r, idx) => (idx === i ? { ...r, [k]: v } : r)));
  const removeRow = (i) => setRows((p) => p.filter((_, idx) => idx !== i));

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4 flex items-center justify-between">
        <div>
          <div className="text-lg font-semibold">Teachers</div>
          <div className="text-sm text-slate-600">
            If Name was empty before, it was a DTO-shape mismatch (nested vs flattened). Fixed here.
          </div>
        </div>
        <button className="px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90" onClick={() => setAssignOpen(true)}>
          Assign Subjects
        </button>
      </div>

      <ErrorBox message={err} />

      <div className="bg-white border rounded-2xl p-4 space-y-3">
        <div className="font-semibold">Create Teacher</div>
        <div className="flex flex-col sm:flex-row gap-3 sm:items-end">
          <div className="flex-1">
            <Field
              label="UserId (AspNetUsers.Id)"
              value={createUserId}
              onChange={(e) => setCreateUserId(e.target.value)}
              placeholder="GUID string"
            />
          </div>
          <button className="px-4 py-2 rounded-lg border hover:bg-slate-50" onClick={() => createM.mutate()} disabled={!createUserId || createM.isPending}>
            {createM.isPending ? "Creating..." : "Create"}
          </button>
        </div>
      </div>

      {q.isLoading && <Spinner />}
      {q.isError && <ErrorBox message={getApiError(q.error)} />}
      <DataTable columns={columns} rows={teachers} />

      <Modal open={assignOpen} title="Assign Subjects (bulk)" onClose={() => setAssignOpen(false)}>
        <div className="space-y-3">
          <div className="text-sm text-slate-600">
            Controller expects <b>teacherId = teacher.UserId (AspNetUsers.Id)</b>.
          </div>

          {rows.map((r, i) => (
            <div key={i} className="border rounded-xl p-3 space-y-2">
              <Field label="Teacher UserId (Guid)" value={r.teacherUserId} onChange={(e) => setRow(i, "teacherUserId", e.target.value)} />
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                <Field label="Subject ID" value={r.subjectId} onChange={(e) => setRow(i, "subjectId", e.target.value)} />
                <Field label="Class ID" value={r.classId} onChange={(e) => setRow(i, "classId", e.target.value)} />
              </div>
              <div className="flex justify-end">
                <button className="px-3 py-1 rounded-lg border border-red-200 text-red-700 hover:bg-red-50" onClick={() => removeRow(i)} disabled={rows.length === 1}>
                  Remove
                </button>
              </div>
            </div>
          ))}

          <div className="flex gap-2">
            <button className="px-4 py-2 rounded-lg border hover:bg-slate-50" onClick={addRow}>
              + Add Row
            </button>
            <button className="flex-1 px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60" onClick={() => assignM.mutate()} disabled={assignM.isPending}>
              {assignM.isPending ? "Assigning..." : "Submit Assignments"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
