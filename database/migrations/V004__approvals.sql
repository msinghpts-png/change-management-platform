CREATE TABLE change_approvals (
    id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    change_request_id UNIQUEIDENTIFIER NOT NULL,
    approver NVARCHAR(100) NOT NULL,
    status NVARCHAR(50) NOT NULL,
    comment NVARCHAR(MAX) NULL,
    decision_at DATETIME2 NULL
);

ALTER TABLE change_approvals
ADD CONSTRAINT fk_change_approvals_change_request
FOREIGN KEY (change_request_id) REFERENCES change_requests (id);

CREATE INDEX ix_change_approvals_change_request_id ON change_approvals (change_request_id);
CREATE INDEX ix_change_approvals_status ON change_approvals (status);
