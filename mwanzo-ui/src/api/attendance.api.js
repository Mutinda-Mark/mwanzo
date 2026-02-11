import { api } from "./client";
import { endpoints } from "./endpoints";

export async function markAttendance(payload) {
  const res = await api.post(endpoints.attendanceMark, payload);
  return res.data;
}

export async function getAttendanceByStudent(studentId, params = {}) {
  const res = await api.get(endpoints.attendanceByStudent(studentId), { params });
  return res.data;
}

export async function updateAttendance(id, payload) {
  const res = await api.put(`${endpoints.attendance}/${id}`, payload);
  return res.data;
}

export async function deleteAttendance(id) {
  const res = await api.delete(`${endpoints.attendance}/${id}`);
  return res.data;
}
