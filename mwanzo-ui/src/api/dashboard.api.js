import { api } from "./client";
import { endpoints } from "./endpoints";

export async function getDashboard(role) {
  if (role === "Admin") {
    const res = await api.get(endpoints.dashboard.admin);
    return res.data;
  }
  if (role === "Teacher") {
    const res = await api.get(endpoints.dashboard.teacher);
    return res.data;
  }
  if (role === "Student") {
    const res = await api.get(endpoints.dashboard.student);
    return res.data;
  }
  return null;
}
