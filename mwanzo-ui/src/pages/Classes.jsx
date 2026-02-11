import React, { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createClass, deleteClass, getClasses, updateClass } from "../api/classes.api";
import { getStudents } from "../api/students.api";
import { getApiError } from "../api/client";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import Field from "../components/Field";
import Spinner from "../components/Spinner";
import ErrorBox from "../components/ErrorBox";

function pick(obj, keys, fallback = null) {
  for (const k of keys) if (obj && obj[k] != null) return obj[k];
  return fallback;
}

export default function Classes() {
  const qc = useQueryClient();
  const [err, setErr] = useState("");

  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");

  // 1) classes
  const classesQ = useQuery({ queryKey: ["classes"], queryFn: getClasses });

  // 2) students (used to compute counts)
  const studentsQ = useQuery({ queryKey: ["students"], queryFn: getStudents });

  const createM = useMutation({
    mutationFn: () => createClass({ name, description }),
    onSuccess: async () => {
      setErr("");
      setOpen(false);
      setName("");
      setDescription("");
      await qc.invalidateQueries({ queryKey: ["classes"] });
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const updateM = useMutation({
    mutationFn: () => updateClass(editing.id, { name, description }),
    onSuccess: async () => {
      setErr("");
      setOpen(false);
      setEditing(null);
      setName("");
      setDescription("");
      await qc.invalidateQueries({ queryKey: ["classes"] });
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const deleteM = useMutation({
    mutationFn: (id) => deleteClass(id),
    onSuccess: async () => {
      setErr("");
      await qc.invalidateQueries({ queryKey: ["classes"] });
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const classes = Array.isArray(classesQ.data) ? classesQ.data : [];
  const students = Array.isArray(studentsQ.data) ? studentsQ.data : [];

  // Build classId -> count map from students list
  const studentsCountMap = useMemo(() => {
    const map = new Map();
    for (const s of students) {
      const cid = pick(s, ["classId", "ClassId"], null);
      if (cid == null) continue;
      map.set(cid, (map.get(cid) ?? 0) + 1);
    }
    return map;
  }, [students]);

  const columns = useMemo(
    () => [
      { key: "id", header: "ID" },
      { key: "name", header: "Name" },
      { key: "description", header: "Description" },
      {
        key: "studentsCount",
        header: "Students",
        render: (c) => c.studentCount ?? c.StudentCount ?? 0
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
                setName(r.name || "");
                setDescription(r.description || "");
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
    [deleteM.isPending, studentsCountMap]
  );

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4 flex items-center justify-between">
        <div>
          <div className="text-lg font-semibold">Classes</div>
          <div className="text-sm text-slate-600">
            Students count is computed from the Students list (reliable even if Classes DTO doesn’t return students[]).
          </div>
        </div>
        <button
          className="px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90"
          onClick={() => {
            setEditing(null);
            setName("");
            setDescription("");
            setOpen(true);
          }}
        >
          + New
        </button>
      </div>

      <ErrorBox message={err} />

      {(classesQ.isLoading || studentsQ.isLoading) && <Spinner />}
      {classesQ.isError && <ErrorBox message={getApiError(classesQ.error)} />}
      {studentsQ.isError && <ErrorBox message={`Students query failed: ${getApiError(studentsQ.error)}`} />}

      <DataTable columns={columns} rows={classes} />

      <Modal open={open} title={editing ? `Edit Class #${editing.id}` : "Create Class"} onClose={() => setOpen(false)}>
        <div className="space-y-3">
          <Field label="Name" value={name} onChange={(e) => setName(e.target.value)} />
          <Field label="Description" value={description} onChange={(e) => setDescription(e.target.value)} />
          <button
            className="w-full px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
            disabled={!name.trim() || createM.isPending || updateM.isPending}
            onClick={() => (editing ? updateM.mutate() : createM.mutate())}
          >
            {editing ? (updateM.isPending ? "Saving..." : "Save") : (createM.isPending ? "Creating..." : "Create")}
          </button>
        </div>
      </Modal>
    </div>
  );
}
