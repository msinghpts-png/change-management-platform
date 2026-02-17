import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";
import type { AppUser } from "../types/change";

const AdminUsersPage = () => {
  const [users, setUsers] = useState<AppUser[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [upn, setUpn] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [role, setRole] = useState("User");
  const [password, setPassword] = useState("Temp123!");

  const load = async () => {
    setLoading(true);
    try {
      setUsers(await apiClient.getUsers());
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load().catch(() => void 0);
  }, []);

  const create = async () => {
    await apiClient.createUser({ upn, displayName, role, password });
    setUpn("");
    setDisplayName("");
    setRole("User");
    await load();
  };

  const toggle = async (u: AppUser) => {
    await apiClient.updateUser(u.id, { role: u.role, isActive: !u.isActive });
    await load();
  };

  const setUserRole = async (u: AppUser, nextRole: string) => {
    await apiClient.updateUser(u.id, { role: nextRole, isActive: u.isActive });
    await load();
  };

  const resetPassword = async (u: AppUser) => {
    const next = window.prompt(`New password for ${u.upn}`);
    if (!next?.trim()) return;
    await apiClient.resetUserPassword(u.id, next.trim());
    alert("Password reset successfully.");
  };

  return (
    <div>
      <div className="page-head"><h1 className="page-title">Admin Users</h1></div>
      {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div> : null}
      <div className="card card-pad">
        <div className="h3">Create User</div>
        <div className="form-grid">
          <input className="input" placeholder="UPN" value={upn} onChange={(e) => setUpn(e.target.value)} />
          <input className="input" placeholder="Display name" value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
          <select className="select" value={role} onChange={(e) => setRole(e.target.value)}><option>User</option><option>CAB</option><option>Admin</option></select>
          <input className="input" type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        <button className="btn btn-primary" onClick={create} style={{ marginTop: 10 }}>Create</button>
      </div>

      <div style={{ height: 12 }} />
      <div className="card card-pad">
        {loading ? <div>Loading users…</div> : (
          <table style={{ width: "100%" }}>
            <thead><tr><th>UPN</th><th>Name</th><th>Role</th><th>Active</th><th /></tr></thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id}>
                  <td>{u.upn}</td>
                  <td>{u.displayName}</td>
                  <td>
                    <select value={u.role} onChange={(e) => setUserRole(u, e.target.value)}>
                      <option>User</option><option>CAB</option><option>Admin</option>
                    </select>
                  </td>
                  <td>{u.isActive ? "Yes" : "No"}</td>
                  <td style={{ display: "flex", gap: 8 }}><button className="btn" onClick={() => toggle(u)}>{u.isActive ? "Deactivate" : "Activate"}</button><button className="btn" onClick={() => resetPassword(u)}>Reset Password</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default AdminUsersPage;
