import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import AppShell from "./components/AppShell";
import ProtectedRoute from "./auth/ProtectedRoute";
import RoleRoute from "./auth/RoleRoute";

import Login from "./pages/Login";
import Register from "./pages/Register";
import ConfirmEmail from "./pages/ConfirmEmail";
import Dashboard from "./pages/Dashboard";

import Subjects from "./pages/Subjects";
import Teachers from "./pages/Teachers";
import Exams from "./pages/Exams";
import Grades from "./pages/Grades";
import Timetable from "./pages/Timetable";
import Students from "./pages/Students";
import Classes from "./pages/Classes";
import Attendance from "./pages/Attendance";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/confirm-email" element={<ConfirmEmail />} />

      <Route
        path="/"
        element={
          <ProtectedRoute>
            <AppShell />
          </ProtectedRoute>
        }
      >
        <Route path="dashboard" element={<Dashboard />} />

        {/* Admin only (typical) */}
        <Route
          path="subjects"
          element={
            <RoleRoute allow={["Admin"]}>
              <Subjects />
            </RoleRoute>
          }
        />
        <Route
          path="classes"
          element={
            <RoleRoute allow={["Admin"]}>
              <Classes />
            </RoleRoute>
          }
        />
        <Route
          path="teachers"
          element={
            <RoleRoute allow={["Admin"]}>
              <Teachers />
            </RoleRoute>
          }
        />

        {/* Teacher + Admin */}
        <Route
          path="students"
          element={
            <RoleRoute allow={["Admin", "Teacher"]}>
              <Students />
            </RoleRoute>
          }
        />
        <Route
          path="attendance"
          element={
            <RoleRoute allow={["Admin", "Teacher"]}>
              <Attendance />
            </RoleRoute>
          }
        />
        <Route
          path="exams"
          element={
            <RoleRoute allow={["Admin", "Teacher"]}>
              <Exams />
            </RoleRoute>
          }
        />
        <Route
          path="grades"
          element={
            <RoleRoute allow={["Admin", "Teacher"]}>
              <Grades />
            </RoleRoute>
          }
        />
        <Route
          path="timetable"
          element={
            <RoleRoute allow={["Admin", "Teacher", "Student"]}>
              <Timetable />
            </RoleRoute>
          }
        />
      </Route>

      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}
