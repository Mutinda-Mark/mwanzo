import React from "react";

export default function Field({ label, ...props }) {
  return (
    <label className="block">
      <div className="text-sm text-slate-700 mb-1">{label}</div>
      <input
        {...props}
        className="w-full px-3 py-2 border rounded-lg bg-white outline-none focus:ring-2 focus:ring-slate-300"
      />
    </label>
  );
}
