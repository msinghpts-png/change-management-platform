import { useEffect, useState } from "react";
import { apiClient } from "../../services/apiClient";
import type { AppUser } from "../../types/change";

const formatDate = (value?: string) => (value ? new Date(value).toLocaleString() : "—");

const AdminUsersTab = () => {
  const [users, setUsers] = useState<AppUser[]>([]);

  useEffect(() => {
    apiClient.getUsers().then(setUsers).catch(() => void 0);
  }, []);

  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead><tr><th>UserId</th><th>Upn</th><th>DisplayName</th><th>Role</th><th>IsActive</th><th>CreatedAt</th></tr></thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id}>
              <td>{user.id}</td>
              <td>{user.upn}</td>
              <td>{user.displayName}</td>
              <td>{user.role}</td>
              <td>{user.isActive ? "Yes" : "No"}</td>
              <td>{formatDate(user.createdAt)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default AdminUsersTab;
