import { api } from "./client";
import { endpoints } from "./endpoints";

export async function getExams(params = {}) {
  const res = await api.get(endpoints.exams, { params }); // supports ?classId=
  return res.data;
}

export async function createExam(payload) {
  const res = await api.post(endpoints.exams, payload);
  return res.data;
}

export async function updateExam(id, payload) {
  const res = await api.put(`${endpoints.exams}/${id}`, payload);
  return res.data;
}

export async function deleteExam(id) {
  const res = await api.delete(`${endpoints.exams}/${id}`);
  return res.data;
}
