import { api } from "./client";
import { endpoints } from "./endpoints";

export async function getTimetableByClass(classId) {
  const res = await api.get(`${endpoints.timetable}/${classId}`);
  return res.data;
}

export async function createTimetableEntry(payload) {
  const res = await api.post(endpoints.timetable, payload);
  return res.data;
}

export async function updateTimetableEntry(id, payload) {
  const res = await api.put(`${endpoints.timetable}/${id}`, payload);
  return res.data;
}

export async function getAllTimetables() {
  const res = await api.get(endpoints.timetable); // GET /api/Timetable
  return res.data;
}

export async function deleteTimetableEntry(id) {
  const res = await api.delete(`${endpoints.timetable}/${id}`);
  return res.data;
}
