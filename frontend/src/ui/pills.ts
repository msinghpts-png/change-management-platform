const normalize = (value?: string) => (value ?? "").trim().toLowerCase();

export const getStatusPillClass = (status?: string) => {
  const s = normalize(status);
  if (s.includes("rejected") || s.includes("cancel")) return "pill pill-red";
  if (s.includes("approved") || s.includes("closed") || s.includes("completed")) return "pill pill-green";
  if (s.includes("pending") || s.includes("submitted")) return "pill pill-amber";
  if (s.includes("inimplementation") || s.includes("in progress") || s.includes("inprogress") || s.includes("scheduled")) return "pill pill-cyan";
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
  if (changeTypeId === 3) return "pill pill-red";
  if (changeTypeId === 1) return "pill pill-blue";
  return "pill pill-green";
};

export const getChangeTypeLabel = (changeTypeId?: number) => {
  if (changeTypeId === 1) return "Standard";
  if (changeTypeId === 3) return "Emergency";
  return "Normal";
};
