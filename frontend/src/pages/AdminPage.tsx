import { useMemo, useState } from "react";
import AdminAttachmentsTab from "./admin-tabs/AdminAttachmentsTab";
import AdminAuditsTab from "./admin-tabs/AdminAuditsTab";
import AdminDatabaseTab from "./admin-tabs/AdminDatabaseTab";
import AdminTemplatesTab from "./admin-tabs/AdminTemplatesTab";
import AdminUsersTab from "./admin-tabs/AdminUsersTab";

const tabs = ["Users", "Database", "Templates", "Attachments", "Audits"] as const;
type AdminTab = typeof tabs[number];

const AdminPage = () => {
  const [activeTab, setActiveTab] = useState<AdminTab>("Users");

  const activeContent = useMemo(() => {
    if (activeTab === "Users") return <AdminUsersTab />;
    if (activeTab === "Database") return <AdminDatabaseTab />;
    if (activeTab === "Templates") return <AdminTemplatesTab />;
    if (activeTab === "Attachments") return <AdminAttachmentsTab />;
    return <AdminAuditsTab />;
  }, [activeTab]);

  return (
    <div>
      <div className="page-head">
        <h1 className="page-title">Admin</h1>
      </div>

      <div className="tabs" role="tablist" aria-label="Admin tabs">
        {tabs.map((tab) => (
          <button
            key={tab}
            type="button"
            className={`tab ${activeTab === tab ? "tab-active" : ""}`}
            role="tab"
            aria-selected={activeTab === tab}
            onClick={() => setActiveTab(tab)}
          >
            {tab}
          </button>
        ))}
      </div>

      <div style={{ height: 12 }} />
      <div className="card card-pad">{activeContent}</div>
    </div>
  );
};

export default AdminPage;
