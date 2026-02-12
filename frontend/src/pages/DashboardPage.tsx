import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";
import type { DashboardStats } from "../types/change";
import "./dashboard.css";

const DashboardPage = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);

  useEffect(() => {
    apiClient.getDashboardStats().then(setStats);
  }, []);

  if (!stats) return <p>Loading dashboard...</p>;

  return (
    <div>
      <h2 className="page-title">Dashboard Overview</h2>

      <div className="card-grid">
        <div className="stat-card">
          <h3>Total Changes</h3>
          <p>{stats.totalChanges}</p>
        </div>

        <div className="stat-card">
          <h3>Pending Approvals</h3>
          <p>{stats.pendingApprovals}</p>
        </div>

        <div className="stat-card">
          <h3>Scheduled This Week</h3>
          <p>{stats.scheduledThisWeek}</p>
        </div>

        <div className="stat-card">
          <h3>Completed This Month</h3>
          <p>{stats.completedThisMonth}</p>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
