import AddOutlinedIcon from "@mui/icons-material/AddOutlined";
import CalendarTodayOutlinedIcon from "@mui/icons-material/CalendarTodayOutlined";
import TrendingUpOutlinedIcon from "@mui/icons-material/TrendingUpOutlined";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  List,
  ListItem,
  ListItemText,
  Stack,
  Typography
} from "@mui/material";
import { Link as RouterLink } from "react-router-dom";
import DashboardCards from "../components/DashboardCards";
import { sampleChanges, sampleDashboardStats } from "../data/sampleData";

const DashboardPage = () => {
  const recentChanges = sampleChanges.slice(0, 3);
  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: "column", sm: "row" }} justifyContent="space-between" spacing={2}>
        <Box>
          <Typography variant="h4">Change Management</Typography>
          <Typography variant="subtitle1">Monitor and manage all change requests</Typography>
        </Box>
        <Button component={RouterLink} to="/changes/new" variant="contained" startIcon={<AddOutlinedIcon />}>
          New Change Request
        </Button>
      </Stack>

      <DashboardCards stats={sampleDashboardStats} />

      <Grid container spacing={2}>
        <Grid item xs={12} lg={7}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" justifyContent="space-between" mb={2}>
                <Typography variant="h6">Recent Changes</Typography>
                <Button size="small" component={RouterLink} to="/changes">
                  View all
                </Button>
              </Stack>
              <List>
                {recentChanges.map((change) => (
                  <ListItem key={change.id} disableGutters secondaryAction={<Chip label={change.status} size="small" />}>
                    <ListItemText
                      primary={change.title}
                      secondary={`${change.id} • ${change.priority ?? "P3"} • ${change.riskLevel ?? "Low"}`}
                    />
                  </ListItem>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} lg={5}>
          <Card>
            <CardContent>
              <Stack direction="row" spacing={1} alignItems="center" mb={2}>
                <CalendarTodayOutlinedIcon color="primary" />
                <Typography variant="h6">Calendar Preview</Typography>
              </Stack>
              <Typography color="text.secondary" mb={2}>
                Upcoming scheduled changes this week: {sampleDashboardStats.scheduledThisWeek}
              </Typography>
              <Button component={RouterLink} to="/calendar" startIcon={<TrendingUpOutlinedIcon />}>
                Open calendar
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Stack>
  );
};

export default DashboardPage;
