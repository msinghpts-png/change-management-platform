import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";
import type { DashboardStats } from "../types/change";

const DashboardPage = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    apiClient
      .getDashboardStats()
      .then((data) => setStats(data))
      .catch((err: Error) => setError(err.message));
  }, []);

  return (
    <section>
      <h2>Dashboard</h2>
      {error ? <p>{error}</p> : null}
      {stats ? (
        <ul>
          <li>Total changes: {stats.totalChanges}</li>
          <li>Pending approvals: {stats.pendingApprovals}</li>
          <li>Scheduled this week: {stats.scheduledThisWeek}</li>
          <li>Completed this month: {stats.completedThisMonth}</li>
        </ul>
      ) : (
        <p>Loading dashboard stats...</p>
      )}
    </section>
  );
};

export default DashboardPage;
