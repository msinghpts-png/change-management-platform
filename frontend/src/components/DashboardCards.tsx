import AssignmentOutlinedIcon from "@mui/icons-material/AssignmentOutlined";
import FlashOnOutlinedIcon from "@mui/icons-material/FlashOnOutlined";
import PendingActionsOutlinedIcon from "@mui/icons-material/PendingActionsOutlined";
import PlayCircleOutlineOutlinedIcon from "@mui/icons-material/PlayCircleOutlineOutlined";
import { Card, CardContent, Grid, Stack, Typography } from "@mui/material";
import type { DashboardStats } from "../types/change";

type Props = {
  stats: DashboardStats;
};

const cards = [
  { label: "Total Changes", key: "totalChanges", icon: <AssignmentOutlinedIcon color="primary" /> },
  { label: "Pending Approval", key: "pendingApprovals", icon: <PendingActionsOutlinedIcon color="warning" /> },
  { label: "In Implementation", key: "inImplementation", icon: <PlayCircleOutlineOutlinedIcon color="secondary" /> },
  { label: "Emergency Changes", key: "emergencyChanges", icon: <FlashOnOutlinedIcon color="error" /> }
] as const;

const DashboardCards = ({ stats }: Props) => {
  return (
    <Grid container spacing={2}>
      {cards.map((card) => (
        <Grid key={card.key} item xs={12} sm={6} lg={3}>
          <Card>
            <CardContent>
              <Stack direction="row" justifyContent="space-between" alignItems="center" mb={1}>
                <Typography variant="body2" color="text.secondary">
                  {card.label}
                </Typography>
                {card.icon}
              </Stack>
              <Typography variant="h5">{stats[card.key]}</Typography>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
};

export default DashboardCards;
