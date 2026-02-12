import React from "react";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../auth/AuthContext";
import { getDashboard } from "../api/dashboard.api";
import Spinner from "../components/Spinner";
import ErrorBox from "../components/ErrorBox";
import { getApiError } from "../api/client";

function StatCard({ label, value }) {
  return (
    <div className="bg-white border rounded-2xl p-4">
      <div className="text-xs text-slate-500">{label}</div>
      <div className="text-2xl font-semibold mt-1">{value}</div>
    </div>
  );
}

export default function Dashboard() {
  const { role } = useAuth();

  const q = useQuery({
    queryKey: ["dashboard", role],
    queryFn: () => getDashboard(role),
    enabled: !!role,
  });

  const data = q.data;

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4">
        <div className="text-lg font-semibold">Dashboard</div>
        <div className="text-sm text-slate-600">Role-based overview.</div>
      </div>

      {q.isLoading && <Spinner />}
      {q.isError && <ErrorBox message={getApiError(q.error)} />}

      {data && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
          {role === "Admin" && (
            <>
              <StatCard label="Total Students" value={data.totalStudents ?? 0} />
              <StatCard label="Total Teachers" value={data.totalTeachers ?? 0} />
              <StatCard label="Total Classes" value={data.totalClasses ?? 0} />
              <StatCard label="Total Exams" value={data.totalExams ?? 0} />
            </>
          )}

          {role === "Teacher" && (
            <>
              <StatCard label="Total Classes" value={data.totalClasses ?? 0} />
              <StatCard label="Total Students" value={data.totalStudents ?? 0} />
              <StatCard label="Total Exams" value={data.totalExams ?? 0} />
            </>
          )}

          {role === "Student" && (
            <>
              <StatCard label="Class" value={data.className || "—"} />
              <StatCard label="Total Exams" value={data.totalExams ?? 0} />
              <StatCard
                label="Average Grade"
                value={Number(data.averageGrade ?? 0).toFixed(2)}
              />
            </>
          )}
        </div>
      )}

      {!data && !q.isLoading && !q.isError && (
        <div className="bg-white border rounded-2xl p-4 text-slate-600">
          No dashboard endpoint configured for role: <b>{role}</b>.
          <div className="text-sm mt-1">
            Update <code>src/api/endpoints.js</code> if your backend uses different dashboard routes.
          </div>
        </div>
      )}
    </div>
  );
}
