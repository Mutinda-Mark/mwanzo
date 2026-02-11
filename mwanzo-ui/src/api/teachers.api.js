import { api } from "./client";
import { endpoints } from "./endpoints";

export async function getTeachers() {
  const res = await api.get(endpoints.teachers);
  return res.data;
}

export async function createTeacher(payload) {
  const res = await api.post(endpoints.teachers, payload);
  return res.data;
}

// expects an array: [{ teacherId: "<USER_GUID>", subjectId: 1, classId: 2 }, ...]
export async function assignSubjects(payloadArray) {
  const res = await api.post(endpoints.teacherAssignSubject, payloadArray);
  return res.data;
}

export async function assignSubject(payloadArray) {
  const { data } = await client.post("/api/Teachers/assign-subject", payloadArray);
  return data;
}