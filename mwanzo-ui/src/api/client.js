import axios from "axios";
import { tokenStore, isExpired } from "../utils/token";

const baseURL = import.meta.env.VITE_API_BASE_URL;

export const api = axios.create({
  baseURL,
  headers: { "Content-Type": "application/json" },
});

// Attach token automatically
api.interceptors.request.use((config) => {
  const token = tokenStore.get();
  if (token && !isExpired(token)) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Nice error message helper
export function getApiError(err) {
  if (!err) return "Unknown error";
  if (err.response?.data) {
    if (typeof err.response.data === "string") return err.response.data;
    if (err.response.data?.message) return err.response.data.message;
    return JSON.stringify(err.response.data);
  }
  return err.message || "Request failed";
}
