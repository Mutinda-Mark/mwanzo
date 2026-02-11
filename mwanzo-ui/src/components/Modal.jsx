import React from "react";

export default function Modal({ open, title, children, onClose }) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 bg-black/30 flex items-center justify-center p-4">
      <div className="w-full max-w-lg bg-white rounded-2xl shadow-lg border">
        <div className="p-4 border-b flex items-center justify-between">
          <div className="font-semibold">{title}</div>
          <button onClick={onClose} className="px-2 py-1 rounded-lg hover:bg-slate-100">
            ✕
          </button>
        </div>
        <div className="p-4">{children}</div>
      </div>
    </div>
  );
}
