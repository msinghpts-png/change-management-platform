import "./TemplatesPage.css";

const templates = [
  {
    id: "tpl-1",
    title: "Windows Security Patch",
    description: "Deploy monthly patch updates with tested rollback and verification steps."
  },
  {
    id: "tpl-2",
    title: "Network Firewall Rule Change",
    description: "Apply controlled rule updates for inbound and outbound application traffic."
  },
  {
    id: "tpl-3",
    title: "Database Maintenance",
    description: "Run index, statistics, and health check routines during maintenance window."
  }
];

const TemplatesPage = () => {
  return (
    <section>
      <div className="page-header">
        <div>
          <h2 className="page-title">Templates</h2>
          <p className="page-subtitle">Reusable patterns for common change requests</p>
        </div>
        <button type="button" className="btn btn-primary">
          Create Template
        </button>
      </div>

      <div className="templates-grid">
        {templates.map((template) => (
          <article key={template.id} className="card template-card">
            <h3 className="template-title">{template.title}</h3>
            <p className="template-description">{template.description}</p>
            <div className="template-actions">
              <button type="button" className="btn btn-secondary">
                Edit
              </button>
              <button type="button" className="btn btn-secondary">
                Duplicate
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
};

export default TemplatesPage;
