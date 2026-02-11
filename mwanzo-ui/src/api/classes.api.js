import { api } from "./client";
import { endpoints } from "./endpoints";

export async function getClasses() {
  const res = await api.get(endpoints.classes);
  return res.data;
}

export async function getClassById(id) {
  const res = await api.get(`${endpoints.classes}/${id}`);
  return res.data;
}

export async function createClass(payload) {
  const res = await api.post(endpoints.classes, payload);
  return res.data;
}

export async function updateClass(id, payload) {
  const res = await api.put(`${endpoints.classes}/${id}`, payload);
  return res.data;
}

export async function deleteClass(id) {
  const res = await api.delete(`${endpoints.classes}/${id}`);
  return res.data;
}
