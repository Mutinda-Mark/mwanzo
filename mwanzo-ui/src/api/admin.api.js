import { api } from "./client";

export async function getAdminUsers(q) {
  const params = {};
  if (q && String(q).trim()) params.q = String(q).trim();
  const res = await api.get("/api/Admin", { params });
  return res.data;
}

export async function getAdminUserById(id) {
  const res = await api.get(`/api/Admin/${id}`);
  return res.data;
}

// payload: { firstName, lastName, admissionNumber?, userName?, role? }
// role should be string like "Admin" | "Teacher" | "Student" | "Parent"
export async function updateAdminUser(id, payload) {
  const res = await api.put(`/api/Admin/${id}`, payload);
  return res.data;
}

export async function deleteAdminUser(id) {
  const res = await api.delete(`/api/Admin/${id}`);
  return res.data; // usually empty for 204
}
