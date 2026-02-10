CREATE TABLE change_requests (
    id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    change_number NVARCHAR(32) NOT NULL,
    title NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX) NOT NULL,
    status NVARCHAR(50) NOT NULL,
    priority NVARCHAR(50) NOT NULL,
    risk NVARCHAR(50) NOT NULL,
    planned_start DATETIME2 NOT NULL,
    planned_end DATETIME2 NOT NULL,
    created_by NVARCHAR(100) NOT NULL,
    created_at DATETIME2 NOT NULL
);

ALTER TABLE change_requests
ADD CONSTRAINT uq_change_requests_number UNIQUE (change_number);

CREATE INDEX ix_change_requests_status ON change_requests (status);
CREATE INDEX ix_change_requests_planned_start ON change_requests (planned_start);
CREATE INDEX ix_change_requests_created_at ON change_requests (created_at);
