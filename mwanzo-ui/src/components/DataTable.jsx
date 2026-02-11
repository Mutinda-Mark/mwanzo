import React from "react";

export default function DataTable({ columns = [], rows = [], keyField = "id" }) {
  return (
    <div className="overflow-x-auto border rounded-lg bg-white">
      <table className="min-w-full text-sm">
        <thead className="bg-slate-100 text-slate-700">
          <tr>
            {columns.map((c) => (
              <th key={c.key} className="text-left p-3 border-b">
                {c.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r[keyField]} className="hover:bg-slate-50">
              {columns.map((c) => (
                <td key={c.key} className="p-3 border-b text-slate-800">
                  {c.render ? c.render(r) : String(r[c.key] ?? "")}
                </td>
              ))}
            </tr>
          ))}
          {rows.length === 0 && (
            <tr>
              <td className="p-4 text-slate-500" colSpan={columns.length}>
                No data
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
