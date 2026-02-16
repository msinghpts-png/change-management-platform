import { Box, Chip, Divider, List, ListItem, ListItemText, Paper, Stack, Tab, Tabs, Typography } from "@mui/material";
import { useState, type ReactNode, type SyntheticEvent } from "react";
import type { Approval, ChangeRequest } from "../types/change";

type Props = {
  change: ChangeRequest;
  approvals: Approval[];
  actions?: ReactNode;
};

const ChangeDetail = ({ change, approvals, actions }: Props) => {
  const [tab, setTab] = useState(0);

  return (
    <Stack spacing={2}>
      <Paper sx={{ p: 3 }}>
        <Stack direction={{ xs: "column", md: "row" }} justifyContent="space-between" spacing={2}>
          <Box>
            <Typography variant="overline">{change.id}</Typography>
            <Typography variant="h5">{change.title}</Typography>
            <Stack direction="row" spacing={1} mt={1} flexWrap="wrap">
              <Chip size="small" label={change.status} color={change.status === "Approved" ? "success" : "default"} />
              {change.priority ? <Chip size="small" label={change.priority} /> : null}
              {change.riskLevel ? <Chip size="small" label={`Risk: ${change.riskLevel}`} color="warning" /> : null}
            </Stack>
          </Box>
          {actions}
        </Stack>
        <Tabs value={tab} onChange={(_event: SyntheticEvent, value: number) => setTab(value)} sx={{ mt: 2 }}>
          <Tab label="Overview" />
          <Tab label={`Approvals (${approvals.length})`} />
          <Tab label="Tasks" />
          <Tab label="Attachments" />
        </Tabs>
        <Divider />
        {tab === 0 ? (
          <Box py={2}>
            <Typography variant="subtitle2" gutterBottom>
              Description
            </Typography>
            <Typography color="text.secondary">{change.description}</Typography>
          </Box>
        ) : null}
        {tab === 1 ? (
          <List>
            {approvals.map((approval) => (
              <ListItem key={approval.id} disableGutters>
                <ListItemText
                  primary={`${approval.approver} • ${approval.status}`}
                  secondary={approval.comment ?? "No comment"}
                />
              </ListItem>
            ))}
          </List>
        ) : null}
        {tab > 1 ? (
          <Box py={2}>
            <Typography color="text.secondary">Placeholder content for this section.</Typography>
          </Box>
        ) : null}
      </Paper>
    </Stack>
  );
};

export default ChangeDetail;
