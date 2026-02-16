import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";

const AdminAttachmentsPage = () => {
  const [items, setItems] = useState<any[]>([]);
  const [changeNumber, setChangeNumber] = useState("");
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    try {
      setItems(await apiClient.getAllAttachments(changeNumber || undefined));
    } catch (e) {
      setError((e as Error).message);
    }
  };

  useEffect(() => { load().catch(() => void 0); }, []);

  const remove = async (id: string) => {
    await apiClient.deleteAdminAttachment(id);
    await load();
  };

  return (
    <div>
      <div className="page-head"><h1 className="page-title">Attachment Management</h1></div>
      {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div> : null}
      <div className="card card-pad" style={{ display: "flex", gap: 8 }}>
        <input className="input" placeholder="Filter by change number (e.g. CHG-001234)" value={changeNumber} onChange={(e) => setChangeNumber(e.target.value)} />
        <button className="btn" onClick={load}>Search</button>
      </div>
      <div style={{ height: 12 }} />
      <div className="card card-pad">
        <table style={{ width: "100%" }}>
          <thead><tr><th>Change</th><th>File</th><th>Path</th><th>Size</th><th /></tr></thead>
          <tbody>
            {items.map((x) => (
              <tr key={x.id}>
                <td>{x.changeNumber}</td>
                <td>{x.fileName}</td>
                <td>{x.filePath}</td>
                <td>{Math.round((x.sizeBytes ?? 0) / 1024)} KB</td>
                <td><button className="btn" onClick={() => remove(x.id)}>Delete</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default AdminAttachmentsPage;
