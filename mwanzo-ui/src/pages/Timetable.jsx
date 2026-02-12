import React, { useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import {
  createTimetableEntry,
  deleteTimetableEntry,
  getAllTimetables,
  getTimetableByClass,
  updateTimetableEntry,
} from "../api/timetable.api";
import { getApiError } from "../api/client";
import Field from "../components/Field";
import ErrorBox from "../components/ErrorBox";
import Spinner from "../components/Spinner";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";

const dayNames = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

function pick(obj, keys, fallback = null) {
  for (const k of keys) if (obj && obj[k] != null) return obj[k];
  return fallback;
}

export default function Timetable() {
  const [err, setErr] = useState("");

  // filter by class (optional)
  const [filterClassId, setFilterClassId] = useState("");

  // modal
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState(null);

  // modal fields (✅ includes classId now)
  const [classId, setClassId] = useState("");
  const [day, setDay] = useState("1");
  const [startTime, setStartTime] = useState("");
  const [endTime, setEndTime] = useState("");
  const [subjectId, setSubjectId] = useState("");

  const allQ = useQuery({ queryKey: ["timetableAll"], queryFn: getAllTimetables });

  const classQ = useQuery({
    queryKey: ["timetableByClass", filterClassId],
    queryFn: () => getTimetableByClass(Number(filterClassId)),
    enabled: !!filterClassId,
  });

  const allRows = Array.isArray(allQ.data) ? allQ.data : [];
  const classRows = Array.isArray(classQ.data) ? classQ.data : [];

  const createM = useMutation({
    mutationFn: () =>
      createTimetableEntry({
        classId: Number(classId),
        day: Number(day),
        startTime,
        endTime,
        subjectId: Number(subjectId),
      }),
    onSuccess: async () => {
      setErr("");
      setOpen(false);
      await allQ.refetch();
      if (filterClassId) await classQ.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const updateM = useMutation({
    mutationFn: () =>
      updateTimetableEntry(editing.id, {
        classId: Number(classId),
        day: Number(day),
        startTime,
        endTime,
        subjectId: Number(subjectId),
      }),
    onSuccess: async () => {
      setErr("");
      setOpen(false);
      setEditing(null);
      await allQ.refetch();
      if (filterClassId) await classQ.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const deleteM = useMutation({
    mutationFn: (id) => deleteTimetableEntry(id),
    onSuccess: async () => {
      setErr("");
      await allQ.refetch();
      if (filterClassId) await classQ.refetch();
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const openCreate = () => {
    setEditing(null);
    setClassId(""); // ✅ not tied to filter
    setDay("1");
    setStartTime("");
    setEndTime("");
    setSubjectId("");
    setOpen(true);
  };

  const allColumns = useMemo(
    () => [
      { key: "id", header: "ID" },
      { key: "classId", header: "Class ID" },
      { key: "className", header: "Class", render: (r) => pick(r, ["className", "ClassName"], "—") },
      { key: "subjectId", header: "Subject ID" },
      { key: "subjectName", header: "Subject", render: (r) => pick(r, ["subjectName", "SubjectName"], "—") },
      {
        key: "day",
        header: "Day",
        render: (r) => dayNames[pick(r, ["day", "Day"], 0)] ?? String(pick(r, ["day", "Day"], "")),
      },
      { key: "startTime", header: "Start" },
      { key: "endTime", header: "End" },
    ],
    []
  );

  const classColumns = useMemo(
    () => [
      //{ key: "id", header: "ID" },
      { key: "subjectId", header: "Subject ID" },
      { key: "subjectName", header: "Subject", render: (r) => pick(r, ["subjectName", "SubjectName"], "—") },
      {
        key: "day",
        header: "Day",
        render: (r) => dayNames[pick(r, ["day", "Day"], 0)] ?? String(pick(r, ["day", "Day"], "")),
      },
      { key: "startTime", header: "Start" },
      { key: "endTime", header: "End" },
      {
        key: "actions",
        header: "Actions",
        render: (r) => (
          <div className="flex gap-2">
            <button
              className="px-3 py-1 rounded-lg border hover:bg-slate-50"
              onClick={() => {
                setEditing(r);
                setClassId(String(pick(r, ["classId", "ClassId"], "")));
                setDay(String(pick(r, ["day", "Day"], 1)));
                setStartTime(pick(r, ["startTime", "StartTime"], ""));
                setEndTime(pick(r, ["endTime", "EndTime"], ""));
                setSubjectId(String(pick(r, ["subjectId", "SubjectId"], "")));
                setOpen(true);
              }}
            >
              Edit
            </button>
            <button
              className="px-3 py-1 rounded-lg border border-red-200 text-red-700 hover:bg-red-50"
              onClick={() => deleteM.mutate(pick(r, ["id", "Id"]))}
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
          <div className="text-lg font-semibold">Timetable</div>
          <div className="text-sm text-slate-600">Create/edit entries from the New button (Class ID is entered in the modal).</div>
        </div>
        <button className="px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90" onClick={openCreate}>
          + New
        </button>
      </div>

      <ErrorBox message={err} />

      <div className="bg-white border rounded-2xl p-4 space-y-2">
        <div className="font-semibold">All Timetable Entries</div>
        {allQ.isLoading && <Spinner />}
        {allQ.isError && <ErrorBox message={getApiError(allQ.error)} />}
        <DataTable columns={allColumns} rows={allRows} />
      </div>

      <div className="bg-white border rounded-2xl p-4 space-y-3">
        <div className="font-semibold">Timetable by Class</div>
        <Field label="Class ID (filter)" value={filterClassId} onChange={(e) => setFilterClassId(e.target.value)} />
        {classQ.isLoading && !!filterClassId && <Spinner />}
        {classQ.isError && <ErrorBox message={getApiError(classQ.error)} />}
        <DataTable columns={classColumns} rows={classRows} />
      </div>

      <Modal open={open} title={editing ? `Edit Entry #${pick(editing, ["id", "Id"])}` : "Create Timetable Entry"} onClose={() => setOpen(false)}>
        <div className="space-y-3">
          <Field label="Class ID" value={classId} onChange={(e) => setClassId(e.target.value)} />
          <Field label="Subject ID" value={subjectId} onChange={(e) => setSubjectId(e.target.value)} />

          <label className="block">
            <div className="text-sm text-slate-700 mb-1">Day</div>
            <select className="w-full px-3 py-2 border rounded-lg bg-white" value={day} onChange={(e) => setDay(e.target.value)}>
              {dayNames.map((d, idx) => (
                <option key={idx} value={idx}>
                  {idx} - {d}
                </option>
              ))}
            </select>
          </label>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
            <Field label="Start Time (HH:mm or HH:mm:ss)" value={startTime} onChange={(e) => setStartTime(e.target.value)} />
            <Field label="End Time (HH:mm or HH:mm:ss)" value={endTime} onChange={(e) => setEndTime(e.target.value)} />
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
