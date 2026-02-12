import React from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const linkBase = "px-3 py-2 rounded-lg text-sm";
const active = "bg-slate-900 text-white";
const idle = "text-slate-700 hover:bg-slate-200";

export default function Nav() {
  const { user, role, logout } = useAuth();
  const nav = useNavigate();

  const doLogout = () => {
    logout();
    nav("/login");
  };

  const links = [
    { to: "/dashboard", label: "Dashboard", roles: ["Admin", "Teacher", "Student"] },
    { to: "/admin-users", label: "Users", roles: ["Admin"] },
    { to: "/subjects", label: "Subjects", roles: ["Admin"] },
    { to: "/classes", label: "Classes", roles: ["Admin"] },
    { to: "/teachers", label: "Teachers", roles: ["Admin"] },
    { to: "/students", label: "Students", roles: ["Admin", "Teacher"] },
    { to: "/attendance", label: "Attendance", roles: ["Admin", "Teacher"] },
    { to: "/exams", label: "Exams", roles: ["Admin", "Teacher"] },
    { to: "/grades", label: "Grades", roles: ["Admin", "Teacher"] },
    { to: "/timetable", label: "Timetable", roles: ["Admin", "Teacher", "Student"] },
  ];

  return (
    <header className="border-b bg-white">
      <div className="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <div className="font-semibold">Mwanzo</div>
          <div className="text-xs text-slate-500">Role: {role || "Unknown"}</div> 
        </div>

        <nav className="flex-1 overflow-x-auto">
            <div className="flex gap-2 whitespace-nowrap">
                {links
                .filter((l) => l.roles.includes(role))
                .map((l) => (
                    <NavLink
                    key={l.to}
                    to={l.to}
                    className={({ isActive }) =>
                        `${linkBase} ${isActive ? active : idle}`
                    }
                    >
                    {l.label}
                    </NavLink>
                ))}
            </div>
        </nav>


        <div className="flex items-center gap-3">
          <div className="text-xs text-slate-600 hidden sm:block">{user?.email || ""}</div>
          <button
            onClick={doLogout}
            className="px-3 py-2 rounded-lg bg-slate-900 text-white text-sm hover:opacity-90"
          >
            Logout
          </button>
        </div>
      </div>
    </header>
  );
}
