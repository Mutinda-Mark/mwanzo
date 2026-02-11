import React, { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createStudent, deleteStudent, getStudentById, getStudents, updateStudent } from "../api/students.api";
import { getApiError } from "../api/client";
import Field from "../components/Field";
import ErrorBox from "../components/ErrorBox";
import Spinner from "../components/Spinner";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";

function pick(obj, keys, fallback = null) {
  for (const k of keys) {
    if (obj && obj[k] != null) return obj[k];
  }
  return fallback;
}

export default function Students() {
  const qc = useQueryClient();
  const [err, setErr] = useState("");

  // fetch by id (old behavior)
  const [studentId, setStudentId] = useState("");
  const [fetchedStudent, setFetchedStudent] = useState(null);

  // modal state
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState(null);

  // form fields
  const [userId, setUserId] = useState("");
  const [classId, setClassId] = useState("");
  const [enrollmentDate, setEnrollmentDate] = useState("");

  // ALL students
  const allQ = useQuery({
    queryKey: ["students"],
    queryFn: getStudents,
  });

  const fetchByIdM = useMutation({
    mutationFn: () => getStudentById(Number(studentId)),
    onSuccess: (data) => {
      setErr("");
      setFetchedStudent(data);
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const createM = useMutation({
    mutationFn: () =>
      createStudent({
        userId,
        classId: classId ? Number(classId) : null,
        enrollmentDate,
      }),
    onSuccess: async () => {
      setErr("");
      setOpen(false);
      setUserId("");
      setClassId("");
      setEnrollmentDate("");
      await qc.invalidateQueries({ queryKey: ["students"] });
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const updateM = useMutation({
    mutationFn: () =>
      updateStudent(editing.id, {
        classId: classId ? Number(classId) : null,
        enrollmentDate,
      }),
    onSuccess: async () => {
      setErr("");
      setOpen(false);
      setEditing(null);
      setUserId("");
      setClassId("");
      setEnrollmentDate("");
      await qc.invalidateQueries({ queryKey: ["students"] });
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const deleteM = useMutation({
    mutationFn: (id) => deleteStudent(id),
    onSuccess: async () => {
      setErr("");
      await qc.invalidateQueries({ queryKey: ["students"] });
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const allRows = Array.isArray(allQ.data) ? allQ.data : [];

  const columns = useMemo(
    () => [
      { key: "id", header: "ID" },
      {
        key: "name",
        header: "Student",
        render: (r) => {
          const first = pick(r, ["firstName", "FirstName"]);
          const last = pick(r, ["lastName", "LastName"]);
          const full = `${first ?? ""} ${last ?? ""}`.trim();
          return full || pick(r, ["studentName", "StudentName"], "—");
        },
      },
      //{ key: "email", header: "Email", render: (r) => pick(r, ["email", "Email"], "—") },
      //{ key: "classId", header: "Class ID", render: (r) => pick(r, ["classId", "ClassId"], "—") },
      { key: "className", header: "Class", render: (r) => pick(r, ["className", "ClassName"], "—") },
      {
        key: "enrolled",
        header: "Enrollment Date",
        render: (r) => {
          const d = pick(r, ["enrollmentDate", "EnrollmentDate"]);
          return d ? String(d).slice(0, 10) : "";
        },
      },
      {
        key: "actions",
        header: "Actions",
        render: (r) => (
          <div className="flex gap-2">
            <button
              className="px-3 py-1 rounded-lg border hover:bg-slate-50"
              onClick={() => {
                setEditing(r);
                setUserId(pick(r, ["userId", "UserId"], ""));
                setClassId(pick(r, ["classId", "ClassId"], "") ? String(pick(r, ["classId", "ClassId"])) : "");
                const d = pick(r, ["enrollmentDate", "EnrollmentDate"], "");
                setEnrollmentDate(d ? String(d).slice(0, 10) : "");
                setOpen(true);
              }}
            >
              Edit
            </button>

            <button
              className="px-3 py-1 rounded-lg border border-red-200 text-red-700 hover:bg-red-50"
              onClick={() => deleteM.mutate(r.id)}
              disabled={deleteM.isPending}
            >
              Delete
            </button>
          </div>
        ),
      },
    ],
    [deleteM.isPending]
  );

  const openCreate = () => {
    setEditing(null);
    setUserId("");
    setClassId("");
    setEnrollmentDate("");
    setOpen(true);
  };

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4 flex items-center justify-between">
        <div>
          <div className="text-lg font-semibold">Students</div>
          <div className="text-sm text-slate-600">Fetch by ID + view all students.</div>
        </div>
        <button className="px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90" onClick={openCreate}>
          + New
        </button>
      </div>

      <ErrorBox message={err} />

      {/* Fetch by ID (old behavior) */}
      <div className="bg-white border rounded-2xl p-4 space-y-3">
        <div className="font-semibold">Fetch Student by ID</div>
        <div className="flex flex-col sm:flex-row gap-3 sm:items-end">
          <div className="flex-1">
            <Field label="Student ID" value={studentId} onChange={(e) => setStudentId(e.target.value)} />
          </div>
          <button
            className="px-4 py-2 rounded-lg border hover:bg-slate-50"
            onClick={() => fetchByIdM.mutate()}
            disabled={!studentId || fetchByIdM.isPending}
          >
            {fetchByIdM.isPending ? "Fetching..." : "Fetch"}
          </button>
        </div>

        {fetchedStudent && (
          <pre className="text-xs overflow-auto bg-slate-50 border rounded-lg p-3">
            {JSON.stringify(fetchedStudent, null, 2)}
          </pre>
        )}
      </div>

      {/* All students */}
      <div className="bg-white border rounded-2xl p-4 space-y-2">
        <div className="font-semibold">All Students</div>
        {allQ.isLoading && <Spinner />}
        {allQ.isError && <ErrorBox message={getApiError(allQ.error)} />}
        <DataTable columns={columns} rows={allRows} />
      </div>

      <Modal open={open} title={editing ? `Edit Student #${editing.id}` : "Create Student"} onClose={() => setOpen(false)}>
        <div className="space-y-3">
          {!editing && (
            <Field
              label="User ID (AspNetUsers.Id)"
              value={userId}
              onChange={(e) => setUserId(e.target.value)}
              placeholder="GUID string"
            />
          )}

          <Field label="Class ID (optional)" value={classId} onChange={(e) => setClassId(e.target.value)} />
          <Field label="Enrollment Date (YYYY-MM-DD)" value={enrollmentDate} onChange={(e) => setEnrollmentDate(e.target.value)} />

          <button
            className="w-full px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
            disabled={editing ? updateM.isPending : createM.isPending}
            onClick={() => (editing ? updateM.mutate() : createM.mutate())}
          >
            {editing ? (updateM.isPending ? "Saving..." : "Save") : (createM.isPending ? "Creating..." : "Create")}
          </button>
        </div>
      </Modal>
    </div>
  );
}
