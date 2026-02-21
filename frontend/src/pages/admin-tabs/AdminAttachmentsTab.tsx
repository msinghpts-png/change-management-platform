import { useEffect, useState } from "react";
import { apiClient } from "../../services/apiClient";
import type { AdminAttachment } from "../../types/change";

const formatDate = (value?: string) => (value ? new Date(value).toLocaleString() : "—");

const AdminAttachmentsTab = () => {
  const [attachments, setAttachments] = useState<AdminAttachment[]>([]);

  useEffect(() => {
    apiClient.getAllAttachments().then(setAttachments).catch(() => void 0);
  }, []);

  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead><tr><th>ChangeAttachmentId</th><th>ChangeRequestId</th><th>FileName</th><th>FileSizeBytes</th><th>UploadedAt</th><th>UploadedBy</th></tr></thead>
        <tbody>
          {attachments.map((attachment) => (
            <tr key={attachment.changeAttachmentId}>
              <td>{attachment.changeAttachmentId}</td>
              <td>{attachment.changeRequestId}</td>
              <td>{attachment.fileName}</td>
              <td>{attachment.fileSizeBytes}</td>
              <td>{formatDate(attachment.uploadedAt)}</td>
              <td>{attachment.uploadedBy || "—"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default AdminAttachmentsTab;
