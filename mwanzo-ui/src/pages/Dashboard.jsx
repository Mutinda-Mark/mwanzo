import React from "react";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../auth/AuthContext";
import { getDashboard } from "../api/dashboard.api";
import Spinner from "../components/Spinner";
import ErrorBox from "../components/ErrorBox";
import { getApiError } from "../api/client";

export default function Dashboard() {
  const { role } = useAuth();

  const q = useQuery({
    queryKey: ["dashboard", role],
    queryFn: () => getDashboard(role),
    enabled: !!role,
  });

  return (
    <div className="space-y-3">
      <div className="bg-white border rounded-2xl p-4">
        <div className="text-lg font-semibold">Dashboard</div>
        <div className="text-sm text-slate-600">Role-based overview.</div>
      </div>

      {q.isLoading && <Spinner />}
      {q.isError && <ErrorBox message={getApiError(q.error)} />}

      {q.data && (
        <pre className="bg-white border rounded-2xl p-4 overflow-auto text-xs">
          {JSON.stringify(q.data, null, 2)}
        </pre>
      )}

      {!q.data && !q.isLoading && !q.isError && (
        <div className="bg-white border rounded-2xl p-4 text-slate-600">
          No dashboard endpoint configured for role: <b>{role}</b>.
          <div className="text-sm mt-1">
            If your backend uses different teacher/student dashboard routes, update <code>src/api/endpoints.js</code>.
          </div>
        </div>
      )}
    </div>
  );
}
