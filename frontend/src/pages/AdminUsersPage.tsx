import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";
import type { AppUser } from "../types/change";

const AdminUsersPage = () => {
  const [users, setUsers] = useState<AppUser[]>([]);
  const [upn, setUpn] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [role, setRole] = useState("User");
  const [password, setPassword] = useState("Temp123!");

  const load = () => apiClient.getUsers().then(setUsers);
  useEffect(() => { load().catch(() => setUsers([])); }, []);

  return <div>
    <h1 className="page-title">User Maintenance</h1>
    <div className="card card-pad">
      <div className="form-grid">
        <input className="input" placeholder="UPN" value={upn} onChange={(e) => setUpn(e.target.value)} />
        <input className="input" placeholder="Display Name" value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
        <select className="select" value={role} onChange={(e) => setRole(e.target.value)}><option>User</option><option>CAB</option><option>Admin</option></select>
        <input className="input" value={password} onChange={(e) => setPassword(e.target.value)} />
      </div>
      <button className="btn btn-primary" onClick={() => apiClient.createUser({ upn, displayName, role, password }).then(load)}>Create User</button>
    </div>
    <div className="card card-pad" style={{ marginTop: 12 }}>
      {users.map((u) => <div key={u.id} className="row" style={{ marginBottom: 8 }}>
        <span>{u.upn} ({u.role})</span>
        <div style={{ display: "flex", gap: 8 }}>
          <button className="btn" onClick={() => apiClient.updateUser(u.id, { role: u.role, isActive: !u.isActive }).then(load)}>{u.isActive ? "Deactivate" : "Activate"}</button>
          <button className="btn" onClick={() => apiClient.resetPassword(u.id, "Temp123!")}>Reset Password</button>
        </div>
      </div>)}
    </div>
  </div>;
};

export default AdminUsersPage;
