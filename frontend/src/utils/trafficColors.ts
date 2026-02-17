export const pillForChangeType = (changeTypeId?: number) => {
  if (changeTypeId === 1) return "pill pill-blue"; // Standard
  if (changeTypeId === 3) return "pill pill-red"; // Emergency
  return "pill"; // Normal/unknown => grey default
};

export const labelForChangeType = (changeTypeId?: number) => {
  if (changeTypeId === 1) return "Standard";
  if (changeTypeId === 3) return "Emergency";
  return "Normal";
};

export const pillForRiskLevel = (risk?: string) => {
  const v = (risk ?? "").toLowerCase();
  if (v.includes("low")) return "pill pill-green";
  if (v.includes("high") || v.includes("critical")) return "pill pill-red";
  return "pill pill-amber";
};

export const pillForImpactLevel = (impact?: string) => {
  const v = (impact ?? "").toLowerCase();
  if (v.includes("low")) return "pill pill-green";
  if (v.includes("high")) return "pill pill-red";
  return "pill pill-amber";
};
