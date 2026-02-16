import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { ChangeRequest } from "../types/change";

type FilterKey = "All" | "Draft" | "Pending" | "Approved" | "In Progress" | "Closed";

const pillForStatus = (status?: string) => {
  const s = (status ?? "").toLowerCase();
  if (s.includes("approved")) return "pill pill-green";
  if (s.includes("closed") || s.includes("complete")) return "pill pill-green";
  if (s.includes("pending")) return "pill pill-amber";
  if (s.includes("inprogress") || s.includes("in progress") || s.includes("implementation"))
    return "pill pill-cyan";
  if (s.includes("rejected")) return "pill pill-red";
  return "pill";
};

const pillForPriority = (priority?: string) => {
  const p = (priority ?? "").toLowerCase();
  if (p.includes("p1") || p.includes("emergency")) return "pill pill-red";
  if (p.includes("p2")) return "pill pill-amber";
  if (p.includes("p3")) return "pill pill-blue";
  return "pill";
};

const fmtDate = (value?: string) => {
  if (!value) return "—";
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return "—";
  return d.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
};

const matchesFilter = (c: ChangeRequest, filter: FilterKey) => {
  const s = (c.status ?? "").toLowerCase();
  switch (filter) {
    case "Draft":
      return s.includes("draft");
    case "Pending":
      return s.includes("pending");
    case "Approved":
      return s.includes("approved");
    case "In Progress":
      return s.includes("inprogress") || s.includes("in progress") || s.includes("implementation");
    case "Closed":
      return s.includes("closed") || s.includes("complete");
    default:
      return true;
  }
};

const ChangeListPage = () => {
  const nav = useNavigate();
  const [items, setItems] = useState<ChangeRequest[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const [query, setQuery] = useState("");
  const [filter, setFilter] = useState<FilterKey>("All");
  const [myOnly, setMyOnly] = useState(false);

  useEffect(() => {
    setLoading(true);
    apiClient
      .getChanges()
      .then((data) => setItems(data ?? []))
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    return items
      .filter((c) => matchesFilter(c, filter))
      .filter((c) => (!myOnly ? true : (c.requestedBy ?? "").toLowerCase().includes("admin")))
      .filter((c) => {
        if (!q) return true;
        const hay = [
          c.changeNumber,
          c.title,
          c.description,
          c.category,
          c.environment,
          c.status,
          c.requestedBy
        ]
          .filter(Boolean)
          .join(" ")
          .toLowerCase();
        return hay.includes(q);
      });
  }, [items, query, filter, myOnly]);

  const chips: FilterKey[] = ["All", "Draft", "Pending", "Approved", "In Progress", "Closed"];

  return (
    <div>
      <div className="page-head">
        <div>
          <h1 className="page-title">Change Requests</h1>
          <p className="page-subtitle">{filtered.length} of {items.length} changes</p>
        </div>

        <button className="btn btn-primary" onClick={() => nav("/changes/new")}>
          + New Change
        </button>
      </div>

      {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div> : null}
      {loading ? <div className="card card-pad">Loading…</div> : null}

      <div className="card card-pad">
        <div className="searchbar">
          <input
            className="input"
            placeholder="Search by title, description, or change number..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />
          <button className={"btn" + (myOnly ? " btn-primary" : "")} onClick={() => setMyOnly((v) => !v)}>
            My Changes
          </button>
          <button className="btn" type="button" onClick={() => alert("Filters UI can be wired next (risk, impact, date).")}>
            Filters
          </button>
        </div>

        <div className="chips">
          {chips.map((c) => (
            <button
              key={c}
              className={"chip " + (filter === c ? "chip-active" : "")}
              onClick={() => setFilter(c)}
            >
              {c}
            </button>
          ))}
        </div>
      </div>

      <div style={{ height: 12 }} />

      <div className="grid" style={{ gap: 12 }}>
        {filtered.map((c) => (
          <div key={c.id} className="card" style={{ cursor: "pointer" }} onClick={() => nav(`/changes/${c.id}`)}>
            <div className="card-pad">
              <div className="row">
                <div className="row-left">
                  <div style={{ display: "flex", alignItems: "center", gap: 8, flexWrap: "wrap" }}>
                    <span className="mono">{c.changeNumber ?? "CHG-000000"}</span>
                    <span className={pillForPriority(c.priority)}>{c.priority ?? "P3"}</span>
                    <span className="pill pill-blue">Normal</span>
                  </div>
                  <div className="h3" style={{ marginTop: 4 }}>{c.title}</div>
                  <div className="small" style={{ marginTop: 4 }}>
                    {c.description ?? "—"}
                  </div>
                  <div className="small" style={{ marginTop: 10 }}>
                    {c.category ?? "—"} &nbsp;•&nbsp; {fmtDate(c.plannedStart)} &nbsp;•&nbsp; {c.requestedBy ?? "admin@example.com"}
                  </div>
                </div>

                <div style={{ display: "flex", flexDirection: "column", alignItems: "flex-end", gap: 8 }}>
                  <span className={pillForStatus(c.status)}>{c.status ?? "—"}</span>
                  <div style={{ display: "flex", gap: 8, flexWrap: "wrap", justifyContent: "flex-end" }}>
                    {c.riskLevel ? <span className="pill pill-amber">Risk: {c.riskLevel}</span> : null}
                    {c.impactLevel ? <span className="pill pill-amber">Impact: {c.impactLevel}</span> : null}
                  </div>
                </div>
              </div>
            </div>
          </div>
        ))}

        {!filtered.length ? <div className="card"><div className="empty">No matching changes</div></div> : null}
      </div>
    </div>
  );
};

export default ChangeListPage;
