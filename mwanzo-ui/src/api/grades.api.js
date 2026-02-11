import { api } from "./client";
import { endpoints } from "./endpoints";

export async function createGrade(payload) {
  const res = await api.post(endpoints.grades, payload);
  return res.data;
}

export async function getGradeById(id) {
  const res = await api.get(`${endpoints.grades}/${id}`);
  return res.data;
}

export async function updateGrade(id, payload) {
  const res = await api.put(`${endpoints.grades}/${id}`, payload);
  return res.data;
}

export async function deleteGrade(id) {
  const res = await api.delete(`${endpoints.grades}/${id}`);
  return res.data;
}

export async function getStudentReport(studentId) {
  const res = await api.get(`${endpoints.grades}/report/${studentId}`);
  return res.data;
}
