import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth";

const LoginPage = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [upn, setUpn] = useState("admin@local");
  const [password, setPassword] = useState("Admin123!");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const submit = async () => {
    setLoading(true);
    setError(null);
    try {
      await login(upn, password);
      navigate("/dashboard");
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card card-pad" style={{ maxWidth: 480, margin: "40px auto" }}>
      <h1 className="page-title">Login</h1>
      {error ? <div className="small" style={{ color: "#fca5a5" }}>{error}</div> : null}
      <div className="label">UPN</div>
      <input className="input" value={upn} onChange={(e) => setUpn(e.target.value)} />
      <div className="label" style={{ marginTop: 12 }}>Password</div>
      <input className="input" type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
      <button className="btn btn-primary" disabled={loading} style={{ marginTop: 16 }} onClick={submit}>Sign In</button>
    </div>
  );
};

export default LoginPage;
