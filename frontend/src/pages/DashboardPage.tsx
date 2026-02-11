import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import StatCard from "../components/StatCard";
import StatusBadge from "../components/StatusBadge";
import { sampleChanges, sampleDashboardStats } from "../data/sampleData";
import { apiClient } from "../services/apiClient";
import type { DashboardStats } from "../types/change";
import "./DashboardPage.css";

const DashboardPage = () => {
  const [stats, setStats] = useState<DashboardStats>(sampleDashboardStats);

  useEffect(() => {
    apiClient.getDashboardStats().then(setStats).catch(() => setStats(sampleDashboardStats));
  }, []);

  const recentChanges = sampleChanges.slice(0, 4);

  return (
    <section>
      <div className="page-header">
        <div>
          <h2 className="page-title">Dashboard</h2>
          <p className="page-subtitle">Overview of changes, approvals, and schedules</p>
        </div>
        <Link className="btn btn-primary" to="/changes/new">
          New Change Request
        </Link>
      </div>

      <div className="dashboard-stats-grid">
        <StatCard title="Total Changes" value={stats.totalChanges} description="Changes tracked this month" />
        <StatCard title="Pending Approvals" value={stats.pendingApprovals} description="Waiting for decision" />
        <StatCard title="In Implementation" value={stats.inImplementation} description="Actively being executed" />
        <StatCard title="Emergency Changes" value={stats.emergencyChanges} description="High-priority incidents" />
      </div>

      <div className="grid-2 dashboard-sections">
        <section className="card">
          <h3 className="dashboard-section-title">Recent Changes</h3>
          <ul className="dashboard-recent-list">
            {recentChanges.map((change) => (
              <li key={change.id}>
                <Link className="dashboard-change-link" to={`/changes/${change.id}`}>
                  <div>
                    <p className="dashboard-change-title">{change.title}</p>
                    <p className="dashboard-change-meta">{change.id}</p>
                  </div>
                  <StatusBadge status={change.status} />
                </Link>
              </li>
            ))}
          </ul>
        </section>

        <section className="card">
          <h3 className="dashboard-section-title">Upcoming Changes</h3>
          <p className="dashboard-upcoming-text">Scheduled this week: {stats.scheduledThisWeek}</p>
          <Link className="btn btn-secondary dashboard-calendar-link" to="/calendar">
            View Calendar
          </Link>
        </section>
      </div>
    </section>
  );
};

export default DashboardPage;
