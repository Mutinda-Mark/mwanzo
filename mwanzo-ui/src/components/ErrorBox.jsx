import React from "react";

export default function ErrorBox({ message }) {
  if (!message) return null;
  return (
    <div className="p-3 rounded-lg bg-red-50 text-red-700 border border-red-200">
      {message}
    </div>
  );
}
