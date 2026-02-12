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

// POST /api/Teachers/assign-subject
// payloadArray: [{ teacherId: "<USER_GUID>", subjectId: 1, classId: 2 }, ...]
export async function assignSubjects(payloadArray) {
  const res = await api.post(endpoints.teacherAssignSubject, payloadArray);
  return res.data;
}

// PUT /api/Teachers/assign-subject/{assignmentId}
// payload: { subjectId: number, classId: number }
export async function updateAssignment(assignmentId, payload) {
  const res = await api.put(`/api/Teachers/assign-subject/${assignmentId}`, payload);
  return res.data;
}

// DELETE /api/Teachers/assign-subject/{assignmentId}
export async function deleteAssignment(assignmentId) {
  const res = await api.delete(`/api/Teachers/assign-subject/${assignmentId}`);
  return res.data;
}
