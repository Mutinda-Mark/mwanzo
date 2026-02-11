import { api } from "./client";
import { endpoints } from "./endpoints";

export async function getStudents() {
  const res = await api.get(endpoints.students); // GET /api/Students
  return res.data;
}

export async function getStudentById(id) {
  const res = await api.get(`${endpoints.students}/${id}`);
  return res.data;
}

export async function createStudent(payload) {
  const res = await api.post(endpoints.students, payload);
  return res.data;
}

export async function updateStudent(id, payload) {
  const res = await api.put(`${endpoints.students}/${id}`, payload);
  return res.data;
}

export async function deleteStudent(id) {
  const res = await api.delete(`${endpoints.students}/${id}`);
  return res.data;
}
