import ArrowForwardIosRoundedIcon from "@mui/icons-material/ArrowForwardIosRounded";
import { Chip, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography } from "@mui/material";
import { Link as RouterLink } from "react-router-dom";
import type { ChangeRequest } from "../types/change";

type Props = {
  changes: ChangeRequest[];
};

const ChangeList = ({ changes }: Props) => (
  <Paper sx={{ overflowX: "auto" }}>
    <Table aria-label="Change requests">
      <TableHead>
        <TableRow>
          <TableCell>Change</TableCell>
          <TableCell>Status</TableCell>
          <TableCell>Priority</TableCell>
          <TableCell>Risk</TableCell>
          <TableCell>Planned Window</TableCell>
          <TableCell align="right">Open</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {changes.map((change) => (
          <TableRow key={change.id} hover>
            <TableCell>
              <Typography variant="subtitle2">{change.title}</Typography>
              <Typography variant="caption" color="text.secondary">
                {change.id}
              </Typography>
            </TableCell>
            <TableCell>
              <Chip label={change.status} size="small" color={change.status === "Approved" ? "success" : "default"} />
            </TableCell>
            <TableCell>{change.priority ?? "-"}</TableCell>
            <TableCell>{change.riskLevel ?? "-"}</TableCell>
            <TableCell>
              <Stack>
                <Typography variant="body2">{change.plannedStart ? new Date(change.plannedStart).toLocaleString() : "TBD"}</Typography>
                <Typography variant="caption" color="text.secondary">
                  {change.plannedEnd ? new Date(change.plannedEnd).toLocaleString() : "TBD"}
                </Typography>
              </Stack>
            </TableCell>
            <TableCell align="right">
              <RouterLink to={`/changes/${change.id}`} aria-label={`Open ${change.title}`}>
                <ArrowForwardIosRoundedIcon fontSize="small" />
              </RouterLink>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  </Paper>
);

export default ChangeList;
