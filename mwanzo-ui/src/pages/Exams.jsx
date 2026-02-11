import React, { useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { createExam, deleteExam, getExams, updateExam } from "../api/exams.api";
import { getApiError } from "../api/client";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import Field from "../components/Field";
import ErrorBox from "../components/ErrorBox";
import Spinner from "../components/Spinner";

export default function Exams() {
  const [err, setErr] = useState("");
  const [classFilter, setClassFilter] = useState("");

  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState(null);

  const [name, setName] = useState("");
  const [examDate, setExamDate] = useState("");
  const [classId, setClassId] = useState("");
  const [subjectId, setSubjectId] = useState("");

  const q = useQuery({
    queryKey: ["exams", classFilter],
    queryFn: () => getExams(classFilter ? { classId: Number(classFilter) } : {}),
  });

  const createM = useMutation({
    mutationFn: () =>
      createExam({
        name,
        examDate,
        classId: Number(classId),
        subjectId: Number(subjectId),
      }),
    onSuccess: () => {
      setErr("");
      setOpen(false);
      q.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const updateM = useMutation({
    mutationFn: () =>
      updateExam(editing.id, {
        name,
        examDate,
        classId: Number(classId),
        subjectId: Number(subjectId),
      }),
    onSuccess: () => {
      setErr("");
      setOpen(false);
      setEditing(null);
      q.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const deleteM = useMutation({
    mutationFn: (id) => deleteExam(id),
    onSuccess: () => {
      setErr("");
      q.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const rows = Array.isArray(q.data) ? q.data : [];

  const columns = useMemo(
    () => [
      { key: "id", header: "ID" },
      { key: "name", header: "Name" },
      { key: "examDate", header: "Date", render: (r) => (r.examDate ? String(r.examDate).slice(0, 10) : "") },
      { key: "classId", header: "Class ID" },
      { key: "subjectId", header: "Subject ID" },
      {
        key: "actions",
        header: "Actions",
        render: (r) => (
          <div className="flex gap-2">
            <button
              className="px-3 py-1 rounded-lg border hover:bg-slate-50"
              onClick={() => {
                setEditing(r);
                setName(r.name || "");
                setExamDate((r.examDate || "").slice(0, 10));
                setClassId(String(r.classId ?? ""));
                setSubjectId(String(r.subjectId ?? ""));
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
    setName("");
    setExamDate("");
    setClassId("");
    setSubjectId("");
    setOpen(true);
  };

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4 flex items-center justify-between">
        <div>
          <div className="text-lg font-semibold">Exams</div>
          <div className="text-sm text-slate-600">Manage exams, optionally filter by class.</div>
        </div>
        <button className="px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90" onClick={openCreate}>
          + New
        </button>
      </div>

      <ErrorBox message={err} />

      <div className="bg-white border rounded-2xl p-4 flex flex-col sm:flex-row gap-3 sm:items-end">
        <div className="flex-1">
          <Field label="Filter by Class ID (optional)" value={classFilter} onChange={(e) => setClassFilter(e.target.value)} />
        </div>
        <button className="px-4 py-2 rounded-lg border hover:bg-slate-50" onClick={() => q.refetch()}>
          Apply
        </button>
      </div>

      {q.isLoading && <Spinner />}
      {q.isError && <ErrorBox message={getApiError(q.error)} />}
      <DataTable columns={columns} rows={rows} />

      <Modal open={open} title={editing ? `Edit Exam #${editing.id}` : "Create Exam"} onClose={() => setOpen(false)}>
        <div className="space-y-3">
          <Field label="Name" value={name} onChange={(e) => setName(e.target.value)} />
          <Field label="Exam Date (YYYY-MM-DD)" value={examDate} onChange={(e) => setExamDate(e.target.value)} />
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
            <Field label="Class ID" value={classId} onChange={(e) => setClassId(e.target.value)} />
            <Field label="Subject ID" value={subjectId} onChange={(e) => setSubjectId(e.target.value)} />
          </div>
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
