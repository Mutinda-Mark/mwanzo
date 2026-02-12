import React, { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { createGrade, deleteGrade, getGradeById, getStudentReport, updateGrade } from "../api/grades.api";
import { getApiError } from "../api/client";
import Field from "../components/Field";
import ErrorBox from "../components/ErrorBox";
import Modal from "../components/Modal";
import DataTable from "../components/DataTable";

function pick(obj, keys, fallback = null) {
  for (const k of keys) if (obj && obj[k] != null) return obj[k];
  return fallback;
}

export default function Grades() {
  const [err, setErr] = useState("");

  // Create form
  const [studentId, setStudentId] = useState("");
  const [examId, setExamId] = useState("");
  const [marks, setMarks] = useState("");
  const [comments, setComments] = useState("");

  // Search
  const [gradeId, setGradeId] = useState("");
  const [grade, setGrade] = useState(null);

  // Report
  const [reportStudentId, setReportStudentId] = useState("");
  const [report, setReport] = useState(null);

  // Edit modal
  const [open, setOpen] = useState(false);
  const [editMarks, setEditMarks] = useState("");
  const [editComments, setEditComments] = useState("");

  const createM = useMutation({
    mutationFn: () =>
      createGrade({
        studentId: Number(studentId),
        examId: Number(examId),
        marks: Number(marks),
        comments,
      }),
    onSuccess: (data) => {
      setErr("");
      setGrade(data);

      // ✅ Auto-show created grade in fetch area
      const id = pick(data, ["id", "Id"], null);
      if (id != null) setGradeId(String(id));
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const fetchM = useMutation({
    mutationFn: () => getGradeById(Number(gradeId)),
    onSuccess: (data) => {
      setErr("");
      setGrade(data);
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const reportM = useMutation({
    mutationFn: () => getStudentReport(Number(reportStudentId)),
    onSuccess: (data) => {
      setErr("");
      setReport(data);
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const updateM = useMutation({
    mutationFn: () =>
      updateGrade(pick(grade, ["id", "Id"]), {
        marks: Number(editMarks),
        comments: editComments,
      }),
    onSuccess: (data) => {
      setErr("");
      setOpen(false);
      setGrade(data);
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const deleteM = useMutation({
    mutationFn: (id) => deleteGrade(id),
    onSuccess: () => {
      setErr("");
      setGrade(null);
    },
    onError: (e) => setErr(getApiError(e)),
  });

  const reportRows = Array.isArray(report?.grades) ? report.grades : [];

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4">
        <div className="text-lg font-semibold">Grades</div>
        <div className="text-sm text-slate-600">Create grades, fetch grade by ID, and get student report.</div>
      </div>

      <ErrorBox message={err} />

      {/* Create Grade */}
      <div className="bg-white border rounded-2xl p-4 space-y-3">
        <div className="font-semibold">Create Grade</div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Field label="Student ID" value={studentId} onChange={(e) => setStudentId(e.target.value)} />
          <Field label="Exam ID" value={examId} onChange={(e) => setExamId(e.target.value)} />
          <Field label="Marks" value={marks} onChange={(e) => setMarks(e.target.value)} />
          <Field label="Comments" value={comments} onChange={(e) => setComments(e.target.value)} />
        </div>
        <button
          className="px-4 py-2 rounded-lg bg-slate-900 text-white hover:opacity-90 disabled:opacity-60"
          onClick={() => createM.mutate()}
          disabled={!studentId || !examId || !marks || createM.isPending}
        >
          {createM.isPending ? "Creating..." : "Create"}
        </button>
      </div>

      {/* Fetch Grade */}
      <div className="bg-white border rounded-2xl p-4 space-y-3">
        <div className="font-semibold">Fetch Grade</div>
        <div className="flex flex-col sm:flex-row gap-3 sm:items-end">
          <div className="flex-1">
            <Field label="Grade ID" value={gradeId} onChange={(e) => setGradeId(e.target.value)} />
          </div>
          <button
            className="px-4 py-2 rounded-lg border hover:bg-slate-50"
            onClick={() => fetchM.mutate()}
            disabled={!gradeId || fetchM.isPending}
          >
            {fetchM.isPending ? "Fetching..." : "Fetch"}
          </button>
        </div>

        {grade && (
          <div className="border rounded-xl p-4 bg-slate-50 space-y-3">
            <div className="text-sm text-slate-600">Grade Details</div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <div className="text-xs text-slate-500">Grade ID</div>
                <div className="font-medium">{pick(grade, ["id", "Id"], "—")}</div>
              </div>

              <div>
                <div className="text-xs text-slate-500">Student</div>
                <div className="font-medium">{pick(grade, ["studentName", "StudentName"], "—")}</div>
              </div>

              <div>
                <div className="text-xs text-slate-500">Exam</div>
                <div className="font-medium">{pick(grade, ["examName", "ExamName"], "—")}</div>
              </div>

              <div>
                <div className="text-xs text-slate-500">Marks</div>
                <div className="font-medium">{pick(grade, ["marks", "Marks"], "—")}</div>
              </div>

              <div className="sm:col-span-2">
                <div className="text-xs text-slate-500">Comments</div>
                <div className="font-medium">{pick(grade, ["comments", "Comments"], "—")}</div>
              </div>
            </div>

            <div className="flex gap-2">
              <button
                className="px-4 py-2 rounded-lg border hover:bg-slate-50"
                onClick={() => {
                  setEditMarks(String(pick(grade, ["marks", "Marks"], "")));
                  setEditComments(pick(grade, ["comments", "Comments"], ""));
                  setOpen(true);
                }}
              >
                Edit
              </button>
              <button
                className="px-4 py-2 rounded-lg border border-red-200 text-red-700 hover:bg-red-50"
                onClick={() => deleteM.mutate(pick(grade, ["id", "Id"]))}
                disabled={deleteM.isPending}
              >
                Delete
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Student Report */}
      <div className="bg-white border rounded-2xl p-4 space-y-3">
        <div className="font-semibold">Student Report</div>
        <div className="flex flex-col sm:flex-row gap-3 sm:items-end">
          <div className="flex-1">
            <Field label="Student ID" value={reportStudentId} onChange={(e) => setReportStudentId(e.target.value)} />
          </div>
          <button
            className="px-4 py-2 rounded-lg border hover:bg-slate-50"
            onClick={() => reportM.mutate()}
            disabled={!reportStudentId || reportM.isPending}
          >
            {reportM.isPending ? "Loading..." : "Get Report"}
          </button>
        </div>

        {report && (
          <div className="space-y-2">
            <div className="text-sm text-slate-700">
              Student: <b>{report.studentName}</b> • Average: <b>{Number(report.averageMarks).toFixed(2)}</b>
            </div>

            <DataTable
              keyField="id"
              columns={[
                { key: "id", header: "Grade ID" },
                { key: "examId", header: "Exam ID" },
                { key: "examName", header: "Exam" },
                { key: "marks", header: "Marks" },
                { key: "comments", header: "Comments" },
              ]}
              rows={Array.isArray(reportRows) ? reportRows : []}
            />
          </div>
        )}
      </div>

      <Modal open={open} title={grade ? `Edit Grade #${pick(grade, ["id", "Id"])}` : "Edit Grade"} onClose={() => setOpen(false)}>
        <div className="space-y-3">
          <Field label="Marks" value={editMarks} onChange={(e) => setEditMarks(e.target.value)} />
          <Field label="Comments" value={editComments} onChange={(e) => setEditComments(e.target.value)} />
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
