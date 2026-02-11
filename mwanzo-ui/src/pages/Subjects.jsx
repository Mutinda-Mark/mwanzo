import React, { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createSubject, deleteSubject, getSubjects, updateSubject } from "../api/subjects.api";
import { getApiError } from "../api/client";
import Spinner from "../components/Spinner";
import ErrorBox from "../components/ErrorBox";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import Field from "../components/Field";

export default function Subjects() {
  const qc = useQueryClient();
  const [err, setErr] = useState("");

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [name, setName] = useState("");

  const q = useQuery({ queryKey: ["subjects"], queryFn: getSubjects });

  const createM = useMutation({
    mutationFn: () => createSubject({ name }),
    onSuccess: async () => {
      setErr("");
      setModalOpen(false);
      setName("");
      await qc.invalidateQueries({ queryKey: ["subjects"] });
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const updateM = useMutation({
    mutationFn: () => updateSubject(editing.id, { name }),
    onSuccess: async () => {
      setErr("");
      setModalOpen(false);
      setEditing(null);
      setName("");
      await qc.invalidateQueries({ queryKey: ["subjects"] });
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const deleteM = useMutation({
    mutationFn: (id) => deleteSubject(id),
    onSuccess: async () => {
      setErr("");
      await qc.invalidateQueries({ queryKey: ["subjects"] });
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const rows = Array.isArray(q.data) ? q.data : [];

  const columns = useMemo(
    () => [
      { key: "id", header: "ID" },
      { key: "name", header: "Name" },
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
                setModalOpen(true);
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
      <div className="bg-white border rounded-2xl p-4 flex items-center justify-between">
        <div>
          <div className="text-lg font-semibold">Subjects</div>
          <div className="text-sm text-slate-600">ID, Name, Actions only.</div>
        </div>
        <button
          className="px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90"
          onClick={() => {
            setEditing(null);
            setName("");
            setModalOpen(true);
          }}
        >
          + New
        </button>
      </div>

      <ErrorBox message={err} />
      {q.isLoading && <Spinner />}
      {q.isError && <ErrorBox message={getApiError(q.error)} />}

      <DataTable columns={columns} rows={rows} />

      <Modal
        open={modalOpen}
        title={editing ? `Edit Subject #${editing.id}` : "Create Subject"}
        onClose={() => setModalOpen(false)}
      >
        <div className="space-y-3">
          <Field label="Name" value={name} onChange={(e) => setName(e.target.value)} />
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
