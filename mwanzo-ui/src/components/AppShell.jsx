import React from "react";
import { Outlet } from "react-router-dom";
import Nav from "./Nav";

export default function AppShell() {
  return (
    <div className="min-h-screen bg-slate-50">
      <Nav />
      <main className="max-w-6xl mx-auto p-4">
        <Outlet />
      </main>
    </div>
  );
}
