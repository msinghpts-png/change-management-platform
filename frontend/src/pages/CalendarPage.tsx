import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { ChangeRequest } from "../types/change";

const startOfMonth = (d: Date) => new Date(d.getFullYear(), d.getMonth(), 1);
const endOfMonth = (d: Date) => new Date(d.getFullYear(), d.getMonth() + 1, 0);
const sameDay = (a: Date, b: Date) =>
  a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();

const CalendarPage = () => {
  const nav = useNavigate();
  const [items, setItems] = useState<ChangeRequest[]>([]);
  const [error, setError] = useState<string | null>(null);

  const [month, setMonth] = useState<Date>(() => new Date());
  const [selected, setSelected] = useState<Date | null>(null);

  useEffect(() => {
    apiClient
      .getChanges()
      .then((data) => setItems(data ?? []))
      .catch((err: Error) => setError(err.message));
  }, []);

  const grid = useMemo(() => {
    const first = startOfMonth(month);
    const last = endOfMonth(month);

    const start = new Date(first);
    start.setDate(first.getDate() - ((first.getDay() + 6) % 7)); // Monday start

    const days: Date[] = [];
    for (let i = 0; i < 42; i++) {
      const d = new Date(start);
      d.setDate(start.getDate() + i);
      days.push(d);
    }

    const byDate = new Map<string, ChangeRequest[]>();
    for (const c of items) {
      if (!c.plannedStart) continue;
      const d = new Date(c.plannedStart);
      if (Number.isNaN(d.getTime())) continue;
      const key = d.toISOString().slice(0, 10);
      byDate.set(key, [...(byDate.get(key) ?? []), c]);
    }

    return { first, last, days, byDate };
  }, [month, items]);

  const selectedKey = selected ? selected.toISOString().slice(0, 10) : null;
  const selectedChanges = selectedKey ? grid.byDate.get(selectedKey) ?? [] : [];

  return (
    <div>
      <div className="page-head">
        <div>
          <h1 className="page-title">Change Calendar</h1>
          <p className="page-subtitle">View scheduled and in-progress changes</p>
        </div>
        <button className="btn btn-primary" onClick={() => nav("/changes/new")}>
          Schedule New Change
        </button>
      </div>

      {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div> : null}

      <div className="two-col">
        <div className="card">
          <div className="card-head">
            <div className="card-title">
              <button className="btn" onClick={() => setMonth(new Date(month.getFullYear(), month.getMonth() - 1, 1))}>‹</button>
              <span style={{ fontSize: 14, fontWeight: 900 }}>
                {month.toLocaleDateString(undefined, { month: "long", year: "numeric" })}
              </span>
              <button className="btn" onClick={() => setMonth(new Date(month.getFullYear(), month.getMonth() + 1, 1))}>›</button>
            </div>
            <button className="btn" onClick={() => setMonth(new Date())}>Today</button>
          </div>

          <div className="card-body" style={{ paddingTop: 10 }}>
            <div style={{ display: "grid", gridTemplateColumns: "repeat(7, 1fr)", gap: 8, marginBottom: 8, color: "#64748b", fontSize: 12, fontWeight: 800 }}>
              {["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map((d) => (
                <div key={d} style={{ textAlign: "center" }}>{d}</div>
              ))}
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "repeat(7, 1fr)", gap: 8 }}>
              {grid.days.map((d) => {
                const inMonth = d.getMonth() === month.getMonth();
                const key = d.toISOString().slice(0, 10);
                const count = (grid.byDate.get(key) ?? []).length;
                const isSel = selected && sameDay(d, selected);

                return (
                  <button
                    key={key}
                    className="card"
                    onClick={() => setSelected(d)}
                    style={{
                      height: 74,
                      padding: 10,
                      textAlign: "left",
                      cursor: "pointer",
                      borderColor: isSel ? "rgba(37,99,235,.35)" : "var(--border)",
                      background: inMonth ? "var(--surface)" : "rgba(255,255,255,.55)"
                    }}
                  >
                    <div style={{ fontWeight: 900, color: inMonth ? "var(--text)" : "var(--muted)" }}>{d.getDate()}</div>
                    {count ? <div className="pill pill-blue" style={{ marginTop: 10, width: "fit-content" }}>{count} change{count > 1 ? "s" : ""}</div> : null}
                  </button>
                );
              })}
            </div>

            <div style={{ marginTop: 12, display: "flex", gap: 10, flexWrap: "wrap" }}>
              <span className="small">Legend:</span>
              <span className="pill pill-red">P1 / Emergency</span>
              <span className="pill pill-amber">P2 / High Risk</span>
              <span className="pill pill-blue">P3</span>
              <span className="pill">P4</span>
            </div>
          </div>
        </div>

        <div className="card">
          <div className="card-head">
            <div className="card-title">📅 Select a date</div>
          </div>

          {!selected ? (
            <div className="empty">
              <div style={{ fontSize: 28, marginBottom: 8 }}>🗓️</div>
              Click on a date to see scheduled changes
            </div>
          ) : (
            <div className="card-body">
              <div className="h3" style={{ marginBottom: 8 }}>
                {selected.toLocaleDateString(undefined, { weekday: "long", year: "numeric", month: "short", day: "numeric" })}
              </div>

              {selectedChanges.length ? (
                <ul className="list" style={{ border: "1px solid var(--border)", borderRadius: 12, overflow: "hidden" }}>
                  {selectedChanges.map((c) => (
                    <li key={c.id} style={{ cursor: "pointer" }} onClick={() => nav(`/changes/${c.id}`)}>
                      <div className="row">
                        <div className="row-left">
                          <div className="mono">{c.changeNumber ?? "CHG-000000"}</div>
                          <div className="h3">{c.title}</div>
                        </div>
                        <span className={"pill " + ((c.priority ?? "").toLowerCase().includes("p1") ? "pill-red" : "pill-blue")}>
                          {c.priority ?? "P3"}
                        </span>
                      </div>
                    </li>
                  ))}
                </ul>
              ) : (
                <div className="empty">No scheduled changes</div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default CalendarPage;
