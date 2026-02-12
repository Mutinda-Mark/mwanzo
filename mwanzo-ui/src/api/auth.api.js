import { api } from "./client";
import { endpoints } from "./endpoints";

export async function register(payload) {
  console.log("auth.api register payload:", payload);
  const res = await api.post(endpoints.auth.register, payload);
  return res.data;
}

export async function login(payload) {
  const res = await api.post(endpoints.auth.login, payload);
  return res.data;
}

export async function confirmEmail(userId, token) {
  const res = await api.get(endpoints.auth.confirmEmail, { params: { userId, token } });
  return res.data;
}
