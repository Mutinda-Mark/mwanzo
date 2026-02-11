import React, { useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { deleteAttendance, getAttendanceByStudent, markAttendance, updateAttendance } from "../api/attendance.api";
import { getApiError } from "../api/client";
import Field from "../components/Field";
import ErrorBox from "../components/ErrorBox";
import Spinner from "../components/Spinner";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";

export default function Attendance() {
  const [err, setErr] = useState("");

  // Mark form
  const [studentId, setStudentId] = useState("");
  const [date, setDate] = useState("");
  const [isPresent, setIsPresent] = useState(true);
  const [notes, setNotes] = useState("");

  // List controls
  const [viewStudentId, setViewStudentId] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  // Edit modal
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [editPresent, setEditPresent] = useState(true);
  const [editNotes, setEditNotes] = useState("");

  const q = useQuery({
    queryKey: ["attendanceByStudent", viewStudentId, startDate, endDate],
    queryFn: () =>
      getAttendanceByStudent(Number(viewStudentId), {
        startDate: startDate || undefined,
        endDate: endDate || undefined,
      }),
    enabled: !!viewStudentId,
  });

  const markM = useMutation({
    mutationFn: () =>
      markAttendance({
        studentId: Number(studentId),
        date,
        isPresent,
        notes,
      }),
    onSuccess: async () => {
      setErr("");
      if (viewStudentId === studentId) {
        q.refetch();
      }
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const updateM = useMutation({
    mutationFn: () => updateAttendance(editing.id, { isPresent: editPresent, notes: editNotes }),
    onSuccess: async () => {
      setErr("");
      setOpen(false);
      setEditing(null);
      q.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const deleteM = useMutation({
    mutationFn: (id) => deleteAttendance(id),
    onSuccess: async () => {
      setErr("");
      q.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const rows = Array.isArray(q.data) ? q.data : [];

  const columns = useMemo(
    () => [
      { key: "id", header: "ID" },
      { key: "studentId", header: "Student ID" },
      { key: "date", header: "Date", render: (r) => (r.date ? String(r.date).slice(0, 10) : "") },
      { key: "isPresent", header: "Present", render: (r) => (r.isPresent ? "Yes" : "No") },
      { key: "notes", header: "Notes" },
      {
        key: "actions",
        header: "Actions",
        render: (r) => (
          <div className="flex gap-2">
            <button
              className="px-3 py-1 rounded-lg border hover:bg-slate-50"
              onClick={() => {
                setEditing(r);
                setEditPresent(!!r.isPresent);
                setEditNotes(r.notes || "");
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

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4">
        <div className="text-lg font-semibold">Attendance</div>
        <div className="text-sm text-slate-600">Mark and view attendance by student.</div>
      </div>

      <ErrorBox message={err} />

      {/* Mark Attendance */}
      <div className="bg-white border rounded-2xl p-4 space-y-3">
        <div className="font-semibold">Mark Attendance</div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Field label="Student ID" value={studentId} onChange={(e) => setStudentId(e.target.value)} />
          <Field label="Date (YYYY-MM-DD)" value={date} onChange={(e) => setDate(e.target.value)} />

          <label className="block">
            <div className="text-sm text-slate-700 mb-1">Present?</div>
            <select
              value={isPresent ? "true" : "false"}
              onChange={(e) => setIsPresent(e.target.value === "true")}
              className="w-full px-3 py-2 border rounded-lg bg-white"
            >
              <option value="true">Yes</option>
              <option value="false">No</option>
            </select>
          </label>

          <Field label="Notes" value={notes} onChange={(e) => setNotes(e.target.value)} />
        </div>

        <button
          className="px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
          onClick={() => markM.mutate()}
          disabled={!studentId || !date || markM.isPending}
        >
          {markM.isPending ? "Marking..." : "Mark"}
        </button>
      </div>

      {/* View Attendance */}
      <div className="bg-white border rounded-2xl p-4 space-y-3">
        <div className="font-semibold">View Attendance (by student)</div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <Field
            label="Student ID"
            value={viewStudentId}
            onChange={(e) => setViewStudentId(e.target.value)}
          />
          <Field label="Start Date (optional)" value={startDate} onChange={(e) => setStartDate(e.target.value)} />
          <Field label="End Date (optional)" value={endDate} onChange={(e) => setEndDate(e.target.value)} />
        </div>

        {q.isLoading && <Spinner />}
        {q.isError && <ErrorBox message={getApiError(q.error)} />}

        <DataTable columns={columns} rows={rows} />
      </div>

      <Modal open={open} title={editing ? `Edit Attendance #${editing.id}` : "Edit Attendance"} onClose={() => setOpen(false)}>
        <div className="space-y-3">
          <label className="block">
            <div className="text-sm text-slate-700 mb-1">Present?</div>
            <select
              value={editPresent ? "true" : "false"}
              onChange={(e) => setEditPresent(e.target.value === "true")}
              className="w-full px-3 py-2 border rounded-lg bg-white"
            >
              <option value="true">Yes</option>
              <option value="false">No</option>
            </select>
          </label>
          <Field label="Notes" value={editNotes} onChange={(e) => setEditNotes(e.target.value)} />
          <button
            className="w-full px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
            onClick={() => updateM.mutate()}
            disabled={updateM.isPending}
          >
            {updateM.isPending ? "Saving..." : "Save"}
          </button>
        </div>
      </Modal>
    </div>
  );
}
