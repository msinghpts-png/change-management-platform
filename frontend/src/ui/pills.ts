const normalize = (value?: string) => (value ?? "").trim().toLowerCase();

const CHANGE_TYPE_STANDARD = 1;
const CHANGE_TYPE_NORMAL = 2;
const CHANGE_TYPE_EMERGENCY = 3;

export const getStatusPillClass = (status?: string) => {
  const s = normalize(status);
  if (s.includes("cancel")) return "pill pill-red";
  if (s.includes("rejected")) return "pill pill-red";
  if (s.includes("approved") || s.includes("closed") || s.includes("completed")) return "pill pill-green";
  if (s.includes("pendingapproval") || s.includes("pending") || s.includes("submitted")) return "pill pill-amber";
  if (s.includes("scheduled") || s.includes("inimplementation") || s.includes("in progress") || s.includes("inprogress")) return "pill pill-cyan";
  return "pill";
};

export const getRiskPillClass = (risk?: string) => {
  const r = normalize(risk);
  if (r.includes("critical") || r.includes("high")) return "pill pill-red";
  if (r.includes("low")) return "pill pill-green";
  return "pill pill-amber";
};

export const getImpactPillClass = (impact?: string) => {
  const i = normalize(impact);
  if (i.includes("high")) return "pill pill-red";
  if (i.includes("low")) return "pill pill-green";
  return "pill pill-amber";
};

export const getChangeTypePillClass = (changeTypeId?: number) => {
  if (changeTypeId === CHANGE_TYPE_EMERGENCY) return "pill pill-red";
  if (changeTypeId === CHANGE_TYPE_STANDARD) return "pill pill-blue";
  if (changeTypeId === CHANGE_TYPE_NORMAL) return "pill pill-green";
  return "pill";
};

export const getChangeTypeLabel = (changeTypeId?: number) => {
  if (changeTypeId === CHANGE_TYPE_STANDARD) return "Standard";
  if (changeTypeId === CHANGE_TYPE_EMERGENCY) return "Emergency";
  if (changeTypeId === CHANGE_TYPE_NORMAL) return "Normal";
  return "Normal";
};
