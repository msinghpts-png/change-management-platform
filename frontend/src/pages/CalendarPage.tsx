import ChevronLeftOutlinedIcon from "@mui/icons-material/ChevronLeftOutlined";
import ChevronRightOutlinedIcon from "@mui/icons-material/ChevronRightOutlined";
import EventOutlinedIcon from "@mui/icons-material/EventOutlined";
import { Box, Card, CardContent, Grid, IconButton, Stack, Typography } from "@mui/material";

const days = Array.from({ length: 31 }, (_, index) => index + 1);

const CalendarPage = () => {
  return (
    <Stack spacing={2}>
      <Box>
        <Typography variant="h4">Change Calendar</Typography>
        <Typography color="text.secondary">View scheduled and in-progress changes</Typography>
      </Box>
      <Grid container spacing={2}>
        <Grid item xs={12} lg={8}>
          <Card>
            <CardContent>
              <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
                <Stack direction="row" alignItems="center" spacing={1}>
                  <IconButton aria-label="Previous month">
                    <ChevronLeftOutlinedIcon />
                  </IconButton>
                  <Typography variant="h6">January 2026</Typography>
                  <IconButton aria-label="Next month">
                    <ChevronRightOutlinedIcon />
                  </IconButton>
                </Stack>
              </Stack>
              <Box
                sx={{
                  display: "grid",
                  gridTemplateColumns: "repeat(7, minmax(0, 1fr))",
                  gap: 1
                }}
              >
                {days.map((day) => (
                  <Box
                    key={day}
                    sx={{
                      border: "1px solid #e2e8f0",
                      borderRadius: 1,
                      minHeight: 72,
                      p: 1,
                      bgcolor: day === 28 ? "rgba(37,99,235,0.08)" : "background.paper"
                    }}
                  >
                    <Typography variant="caption">{day}</Typography>
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} lg={4}>
          <Card sx={{ height: "100%" }}>
            <CardContent>
              <Stack direction="row" spacing={1} alignItems="center" mb={2}>
                <EventOutlinedIcon color="primary" />
                <Typography variant="h6">Select a date</Typography>
              </Stack>
              <Typography color="text.secondary">Click on a date to see scheduled changes.</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Stack>
  );
};

export default CalendarPage;
