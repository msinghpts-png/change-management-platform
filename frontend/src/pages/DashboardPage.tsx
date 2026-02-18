import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { ChangeRequest, DashboardStats } from "../types/change";
import { labelForChangeType, pillForChangeType } from "../utils/trafficColors";
import { getStatusPillClass } from "../ui/pills";

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

const DashboardPage = () => {
  const nav = useNavigate();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [changes, setChanges] = useState<ChangeRequest[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    Promise.all([apiClient.getDashboardStats(), apiClient.getChanges()])
      .then(([s, c]) => {
        setStats(s);
        setChanges(c ?? []);
      })
      .catch((err: Error) => setError(err.message));
  }, []);

  const derived = useMemo(() => {
    const now = new Date();
    const pending = changes.filter((c) => (c.status ?? "").toLowerCase().includes("pending"));
    const upcoming = changes
      .filter((c) => c.plannedStart && new Date(c.plannedStart) >= now)
      .sort((a, b) => (a.plannedStart ?? "").localeCompare(b.plannedStart ?? ""))
      .slice(0, 3);
    const recent = [...changes]
      .sort((a, b) => (b.createdAt ?? "").localeCompare(a.createdAt ?? ""))
      .slice(0, 5);

    const emergency = changes.filter((c) =>
      (c.priority ?? "").toLowerCase().includes("p1") || (c.priority ?? "").toLowerCase().includes("emergency")
    );

    return { pending, upcoming, recent, emergency };
  }, [changes]);

  return (
    <div>
      <div className="page-head">
        <div>
          <h1 className="page-title">Change Management</h1>
          <p className="page-subtitle">Monitor and manage all change requests</p>
        </div>
      </div>

      {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div> : null}
      {loading ? <div className="card card-pad">Loading…</div> : null}

      <div className="grid grid-4">
        <div className="card card-pad">
          <div className="stat">
            <div>
              <div className="stat-kicker">Total Changes</div>
              <div className="stat-value">{stats?.totalChanges ?? changes.length ?? 0}</div>
              <div className="stat-foot">This month</div>
            </div>
            <div className="pill pill-blue">📄</div>
          </div>
        </div>

        <div className="card card-pad">
          <div className="stat">
            <div>
              <div className="stat-kicker">Pending Approval</div>
              <div className="stat-value">{stats?.pendingApprovals ?? derived.pending.length}</div>
            </div>
            <div className="pill pill-amber">🕒</div>
          </div>
        </div>

        <div className="card card-pad">
          <div className="stat">
            <div>
              <div className="stat-kicker">In Implementation</div>
              <div className="stat-value">
                {changes.filter((c) => (c.status ?? "").toLowerCase().includes("inprogress")).length}
              </div>
            </div>
            <div className="pill pill-cyan">▶</div>
          </div>
        </div>

        <div className="card card-pad">
          <div className="stat">
            <div>
              <div className="stat-kicker">Emergency Changes</div>
              <div className="stat-value">{derived.emergency.length}</div>
            </div>
            <div className="pill pill-red">⚡</div>
          </div>
        </div>
      </div>

      <div style={{ height: 14 }} />

      <div className="grid grid-2">
        <div className="card">
          <div className="card-head">
            <div className="card-title">
              <span className="pill pill-amber">🕒</span> Pending Approval
            </div>
            <button className="btn" onClick={() => nav("/changes")}>
              View All →
            </button>
          </div>
          {derived.pending.length ? (
            <ul className="list">
              {derived.pending.slice(0, 1).map((c) => (
                <li key={c.id} style={{ cursor: "pointer" }} onClick={() => nav(`/changes/${c.id}`)}>
                  <div className="row">
                    <div className="row-left">
                      <div className="mono">{c.changeNumber ?? "CHG-000000"}</div>
                      <div className="h3">{c.title}</div>
                      <div className="small">{c.category ?? "—"}</div>
                    </div>
                    <div className="pill pill-amber">Manager Approval Pending</div>
                  </div>
                </li>
              ))}
            </ul>
          ) : (
            <div className="empty">No pending approvals</div>
          )}
        </div>

        <div className="card">
          <div className="card-head">
            <div className="card-title">
              <span className="pill pill-blue">📅</span> Upcoming Changes
            </div>
            <button className="btn" onClick={() => nav("/calendar")}>
              Calendar →
            </button>
          </div>
          {derived.upcoming.length ? (
            <ul className="list">
              {derived.upcoming.map((c) => (
                <li key={c.id} style={{ cursor: "pointer" }} onClick={() => nav(`/changes/${c.id}`)}>
                  <div className="row">
                    <div className="row-left">
                      <div className="mono">{c.changeNumber ?? "CHG-000000"}</div>
                      <div className="h3">{c.title}</div>
                      <div className="small">{fmtDate(c.plannedStart)}</div>
                    </div>
                    <span className={getStatusPillClass(c.status)}>{c.status ?? "Scheduled"}</span>
                  </div>
                </li>
              ))}
            </ul>
          ) : (
            <div className="empty">No upcoming scheduled changes</div>
          )}
        </div>
      </div>

      <div style={{ height: 14 }} />

      <div className="card">
        <div className="card-head">
          <div className="card-title">
            <span className="pill pill-green">↗</span> Recent Changes
          </div>
          <button className="btn" onClick={() => nav("/changes")}>
            View All →
          </button>
        </div>

        {derived.recent.length ? (
          <ul className="list">
            {derived.recent.map((c) => (
              <li key={c.id} style={{ cursor: "pointer" }} onClick={() => nav(`/changes/${c.id}`)}>
                <div className="row">
                  <div className="row-left">
                    <div style={{ display: "flex", alignItems: "center", gap: 8, flexWrap: "wrap" }}>
                      <span className="mono">{c.changeNumber ?? "CHG-000000"}</span>
                      <span className={pillForPriority(c.priority)}>{c.priority ?? "P3"}</span>
                      <span className={pillForChangeType(c.changeTypeId)}>{labelForChangeType(c.changeTypeId)}</span>
                    </div>
                    <div className="h3">{c.title}</div>
                    <div className="small">
                      {c.category ?? "—"} &nbsp;•&nbsp; {c.requestedBy ?? "admin@example.com"} &nbsp;•&nbsp;{" "}
                      {fmtDate(c.createdAt)}
                    </div>
                  </div>
                  <span className={getStatusPillClass(c.status)}>{c.status ?? "—"}</span>
                </div>
              </li>
            ))}
          </ul>
        ) : (
          <div className="empty">No changes yet</div>
        )}
      </div>
    </div>
  );
};

export default DashboardPage;
