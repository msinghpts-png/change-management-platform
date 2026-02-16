import { useState } from "react";
import { apiClient } from "../services/apiClient";
import type { Attachment } from "../types/change";

const AttachmentManagementPage = () => {
  const [changeId, setChangeId] = useState("");
  const [items, setItems] = useState<Attachment[]>([]);

  return <div>
    <h1 className="page-title">Attachment Management</h1>
    <div className="card card-pad">
      <input className="input" placeholder="Change Id" value={changeId} onChange={(e) => setChangeId(e.target.value)} />
      <button className="btn" style={{ marginTop: 8 }} onClick={() => apiClient.getAttachments(changeId).then(setItems)}>Load Attachments</button>
      {items.map((a) => <div key={a.changeAttachmentId}>{a.fileName} ({a.contentType})</div>)}
    </div>
  </div>;
};

export default AttachmentManagementPage;
