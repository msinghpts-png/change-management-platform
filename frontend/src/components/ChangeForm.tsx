import { Box, Button, Grid, MenuItem, Paper, Stack, TextField, Typography } from "@mui/material";
import type { ChangeEvent } from "react";
import type { ChangeCreateDto } from "../types/change";

type Props = {
  value: ChangeCreateDto;
  onChange: (field: keyof ChangeCreateDto, value: string) => void;
  onSubmit: () => void;
  submitLabel: string;
  disabled?: boolean;
};

const ChangeForm = ({ value, onChange, onSubmit, submitLabel, disabled }: Props) => {
  return (
    <Paper sx={{ p: 3 }} component="section">
      <Typography variant="h6" gutterBottom>
        Basic Information
      </Typography>
      <Grid container spacing={2}>
        <Grid item xs={12}>
          <TextField
            label="Title"
            fullWidth
            value={value.title}
            onChange={(event: ChangeEvent<HTMLInputElement>) => onChange("title", event.target.value)}
            required
          />
        </Grid>
        <Grid item xs={12}>
          <TextField
            label="Description"
            fullWidth
            multiline
            minRows={4}
            value={value.description}
            onChange={(event: ChangeEvent<HTMLInputElement>) => onChange("description", event.target.value)}
            required
          />
        </Grid>
        <Grid item xs={12} md={6}>
          <TextField
            select
            label="Priority"
            fullWidth
            value={value.priority ?? ""}
            onChange={(event: ChangeEvent<HTMLInputElement>) => onChange("priority", event.target.value)}
          >
            {["P1", "P2", "P3", "P4"].map((item) => (
              <MenuItem key={item} value={item}>
                {item}
              </MenuItem>
            ))}
          </TextField>
        </Grid>
        <Grid item xs={12} md={6}>
          <TextField
            select
            label="Risk Level"
            fullWidth
            value={value.riskLevel ?? ""}
            onChange={(event: ChangeEvent<HTMLInputElement>) => onChange("riskLevel", event.target.value)}
          >
            {["Low", "Medium", "High"].map((item) => (
              <MenuItem key={item} value={item}>
                {item}
              </MenuItem>
            ))}
          </TextField>
        </Grid>
        <Grid item xs={12} md={6}>
          <TextField
            label="Planned Start"
            type="datetime-local"
            fullWidth
            InputLabelProps={{ shrink: true }}
            value={value.plannedStart ?? ""}
            onChange={(event: ChangeEvent<HTMLInputElement>) => onChange("plannedStart", event.target.value)}
          />
        </Grid>
        <Grid item xs={12} md={6}>
          <TextField
            label="Planned End"
            type="datetime-local"
            fullWidth
            InputLabelProps={{ shrink: true }}
            value={value.plannedEnd ?? ""}
            onChange={(event: ChangeEvent<HTMLInputElement>) => onChange("plannedEnd", event.target.value)}
          />
        </Grid>
      </Grid>
      <Box mt={3}>
        <Stack direction="row" justifyContent="flex-end">
          <Button variant="contained" onClick={onSubmit} disabled={disabled}>
            {submitLabel}
          </Button>
        </Stack>
      </Box>
    </Paper>
  );
};

export default ChangeForm;
