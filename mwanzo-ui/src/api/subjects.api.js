import { api } from "./client";
import { endpoints } from "./endpoints";

export async function getSubjects() {
  const res = await api.get(endpoints.subjects);
  return res.data;
}

export async function createSubject(payload) {
  const res = await api.post(endpoints.subjects, payload);
  return res.data;
}

export async function updateSubject(id, payload) {
  const res = await api.put(`${endpoints.subjects}/${id}`, payload);
  return res.data;
}

export async function deleteSubject(id) {
  const res = await api.delete(`${endpoints.subjects}/${id}`);
  return res.data;
}
