import React, { useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import {
  getTeachers,
  createTeacher,
  assignSubjects,
  updateAssignment,
  deleteAssignment,
} from "../api/teachers.api";
import { getSubjects } from "../api/subjects.api";
import { getClasses } from "../api/classes.api";
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

function teacherName(t) {
  const first = pick(t, ["firstName", "FirstName"], "");
  const last = pick(t, ["lastName", "LastName"], "");
  const full = `${first} ${last}`.trim();
  return full || pick(t, ["email", "Email"], "—");
}

export default function Teachers() {
  const [err, setErr] = useState("");

  // create teacher
  const [createUserId, setCreateUserId] = useState("");

  // assign modal
  const [assignOpen, setAssignOpen] = useState(false);
  const [assignRows, setAssignRows] = useState([{ teacherId: "", subjectId: "", classId: "" }]);

  // edit assignment modal
  const [editOpen, setEditOpen] = useState(false);
  const [editing, setEditing] = useState(null); // { assignmentId, teacherName, subjectId, classId }
  const [editSubjectId, setEditSubjectId] = useState("");
  const [editClassId, setEditClassId] = useState("");

  const teachersQ = useQuery({ queryKey: ["teachers"], queryFn: getTeachers });
  const subjectsQ = useQuery({ queryKey: ["subjects"], queryFn: getSubjects });
  const classesQ = useQuery({ queryKey: ["classes"], queryFn: getClasses });

  const teachers = Array.isArray(teachersQ.data) ? teachersQ.data : [];
  const subjects = Array.isArray(subjectsQ.data) ? subjectsQ.data : [];
  const classes = Array.isArray(classesQ.data) ? classesQ.data : [];

  const createM = useMutation({
    mutationFn: () => createTeacher({ userId: createUserId }),
    onSuccess: async () => {
      setErr("");
      setCreateUserId("");
      await teachersQ.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const assignM = useMutation({
    mutationFn: async () => {
      // Backend expects: [{ teacherId: "<guid>", subjectId: 1, classId: 1 }, ...]
      const payload = assignRows
        .filter((r) => r.teacherId && r.subjectId && r.classId)
        .map((r) => ({
          teacherId: r.teacherId,
          subjectId: Number(r.subjectId),
          classId: Number(r.classId),
        }));

      if (payload.length === 0) {
        throw new Error("Add at least one valid assignment row (teacher, subject, class).");
      }

      return assignSubjects(payload);
    },
    onSuccess: async () => {
      setErr("");
      setAssignOpen(false);
      setAssignRows([{ teacherId: "", subjectId: "", classId: "" }]);
      await teachersQ.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const updateAssignM = useMutation({
    mutationFn: async () => {
      if (!editing?.assignmentId) throw new Error("No assignment selected.");
      return updateAssignment(editing.assignmentId, {
        subjectId: Number(editSubjectId),
        classId: Number(editClassId),
      });
    },
    onSuccess: async () => {
      setErr("");
      setEditOpen(false);
      setEditing(null);
      setEditSubjectId("");
      setEditClassId("");
      await teachersQ.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const deleteAssignM = useMutation({
    mutationFn: (assignmentId) => deleteAssignment(assignmentId),
    onSuccess: async () => {
      setErr("");
      await teachersQ.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  // helpers for assign modal
  const addAssignRow = () => setAssignRows((p) => [...p, { teacherId: "", subjectId: "", classId: "" }]);
  const setAssignRow = (i, key, value) =>
    setAssignRows((p) => p.map((r, idx) => (idx === i ? { ...r, [key]: value } : r)));
  const removeAssignRow = (i) => setAssignRows((p) => p.filter((_, idx) => idx !== i));

  const openEditAssignment = (teacher, assignment) => {
    const assignmentId = pick(assignment, ["id", "Id"], null);
    const subjectId = pick(assignment, ["subjectId", "SubjectId"], "");
    const classId = pick(assignment, ["classId", "ClassId"], "");

    if (assignmentId == null) {
      setErr("Assignment ID missing in response. Ensure SubjectAssignmentResponseDto includes Id and mapping returns it.");
      return;
    }

    setEditing({
      assignmentId,
      teacherLabel: teacherName(teacher),
    });
    setEditSubjectId(String(subjectId ?? ""));
    setEditClassId(String(classId ?? ""));
    setEditOpen(true);
  };

  const columns = useMemo(
    () => [
      { key: "id", header: "Teacher ID", render: (t) => pick(t, ["id", "Id"], "—") },
      { key: "userId", header: "UserId (Guid)", render: (t) => pick(t, ["userId", "UserId"], "—") },
      { key: "name", header: "Name", render: (t) => teacherName(t) },
      {
        key: "assignments",
        header: "Assignments",
        render: (t) => {
          const list = pick(t, ["subjectAssignments", "SubjectAssignments"], []);
          if (!Array.isArray(list) || list.length === 0) return "—";

          return (
            <div className="space-y-2">
              {list.map((a, idx) => {
                const assignmentId = pick(a, ["id", "Id"], null);
                const subjectName = pick(a, ["subjectName", "SubjectName"], "Subject");
                const className = pick(a, ["className", "ClassName"], "Class");

                return (
                  <div key={assignmentId ?? idx} className="flex items-center justify-between gap-2 border rounded-xl px-3 py-2 bg-slate-50">
                    <div className="text-xs">
                      <div className="font-medium">{subjectName}</div>
                      <div className="text-slate-600">{className}</div>
                      {assignmentId != null /* && (
                        <div className="text-[11px] text-slate-500">Assignment #{assignmentId}</div>
                      )*/}
                    </div>

                    <div className="flex gap-2">
                      <button
                        className="px-3 py-1 rounded-lg border hover:bg-white"
                        onClick={() => openEditAssignment(t, a)}
                      >
                        Edit
                      </button>

                      <button
                        className="px-3 py-1 rounded-lg border border-red-200 text-red-700 hover:bg-red-50 disabled:opacity-60"
                        onClick={() => {
                          if (assignmentId == null) {
                            setErr("Assignment ID missing — cannot delete.");
                            return;
                          }
                          deleteAssignM.mutate(assignmentId);
                        }}
                        disabled={deleteAssignM.isPending}
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          );
        },
      },
    ],
    [deleteAssignM.isPending]
  );

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4 flex items-center justify-between">
        <div>
          <div className="text-lg font-semibold">Teachers</div>
          <div className="text-sm text-slate-600">
            Manage teachers and subject assignments (create, edit, delete).
          </div>
        </div>

        <button
          className="px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90"
          onClick={() => setAssignOpen(true)}
        >
          Assign Subjects
        </button>
      </div>

      <ErrorBox message={err} />

      {/* Create teacher */}
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
          <button
            className="px-4 py-2 rounded-lg border hover:bg-slate-50"
            onClick={() => createM.mutate()}
            disabled={!createUserId || createM.isPending}
          >
            {createM.isPending ? "Creating..." : "Create"}
          </button>
        </div>
      </div>

      {(teachersQ.isLoading || subjectsQ.isLoading || classesQ.isLoading) && <Spinner />}
      {teachersQ.isError && <ErrorBox message={getApiError(teachersQ.error)} />}

      <DataTable columns={columns} rows={teachers} />

      {/* Assign modal */}
      <Modal open={assignOpen} title="Assign Subjects to Teachers" onClose={() => setAssignOpen(false)}>
        <div className="space-y-3">
          <div className="text-sm text-slate-600">
            This posts to <b>POST /api/Teachers/assign-subject</b> with an array of assignments.
            TeacherId is the <b>GUID UserId</b> (AspNetUsers.Id).
          </div>

          {assignRows.map((r, i) => (
            <div key={i} className="border rounded-xl p-3 space-y-2">
              <label className="block">
                <div className="text-sm text-slate-700 mb-1">Teacher</div>
                <select
                  className="w-full px-3 py-2 border rounded-lg bg-white"
                  value={r.teacherId}
                  onChange={(e) => setAssignRow(i, "teacherId", e.target.value)}
                >
                  <option value="">-- select teacher --</option>
                  {teachers.map((t) => {
                    const userId = pick(t, ["userId", "UserId"], "");
                    return (
                      <option key={userId} value={userId}>
                        {teacherName(t)} ({String(userId).slice(0, 8)}…)
                      </option>
                    );
                  })}
                </select>
              </label>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                <label className="block">
                  <div className="text-sm text-slate-700 mb-1">Subject</div>
                  <select
                    className="w-full px-3 py-2 border rounded-lg bg-white"
                    value={r.subjectId}
                    onChange={(e) => setAssignRow(i, "subjectId", e.target.value)}
                  >
                    <option value="">-- select subject --</option>
                    {subjects.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.name} (#{s.id})
                      </option>
                    ))}
                  </select>
                </label>

                <label className="block">
                  <div className="text-sm text-slate-700 mb-1">Class</div>
                  <select
                    className="w-full px-3 py-2 border rounded-lg bg-white"
                    value={r.classId}
                    onChange={(e) => setAssignRow(i, "classId", e.target.value)}
                  >
                    <option value="">-- select class --</option>
                    {classes.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name} (#{c.id})
                      </option>
                    ))}
                  </select>
                </label>
              </div>

              <div className="flex justify-end">
                <button
                  className="px-3 py-1 rounded-lg border border-red-200 text-red-700 hover:bg-red-50"
                  onClick={() => removeAssignRow(i)}
                  disabled={assignRows.length === 1}
                >
                  Remove
                </button>
              </div>
            </div>
          ))}

          <div className="flex gap-2">
            <button className="px-4 py-2 rounded-lg border hover:bg-slate-50" onClick={addAssignRow}>
              + Add Row
            </button>

            <button
              className="flex-1 px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
              onClick={() => assignM.mutate()}
              disabled={assignM.isPending}
            >
              {assignM.isPending ? "Assigning..." : "Submit Assignments"}
            </button>
          </div>
        </div>
      </Modal>

      {/* Edit assignment modal */}
      <Modal
        open={editOpen}
        title={editing ? `Edit Assignment #${editing.assignmentId}` : "Edit Assignment"}
        onClose={() => setEditOpen(false)}
      >
        <div className="space-y-3">
          <div className="text-sm text-slate-600">
            Teacher: <b>{editing?.teacherLabel ?? "—"}</b>
          </div>

          <label className="block">
            <div className="text-sm text-slate-700 mb-1">Subject</div>
            <select
              className="w-full px-3 py-2 border rounded-lg bg-white"
              value={editSubjectId}
              onChange={(e) => setEditSubjectId(e.target.value)}
            >
              <option value="">-- select subject --</option>
              {subjects.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name} (#{s.id})
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <div className="text-sm text-slate-700 mb-1">Class</div>
            <select
              className="w-full px-3 py-2 border rounded-lg bg-white"
              value={editClassId}
              onChange={(e) => setEditClassId(e.target.value)}
            >
              <option value="">-- select class --</option>
              {classes.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name} (#{c.id})
                </option>
              ))}
            </select>
          </label>

          <button
            className="w-full px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
            onClick={() => updateAssignM.mutate()}
            disabled={!editSubjectId || !editClassId || updateAssignM.isPending}
          >
            {updateAssignM.isPending ? "Saving..." : "Save Changes"}
          </button>
        </div>
      </Modal>
    </div>
  );
}
