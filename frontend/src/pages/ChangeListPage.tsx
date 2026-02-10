import AddOutlinedIcon from "@mui/icons-material/AddOutlined";
import SearchOutlinedIcon from "@mui/icons-material/SearchOutlined";
import { Box, Button, Stack, TextField, Typography } from "@mui/material";
import { useEffect, useMemo, useState, type ChangeEvent } from "react";
import { Link as RouterLink } from "react-router-dom";
import ChangeList from "../components/ChangeList";
import { sampleChanges } from "../data/sampleData";
import { apiClient } from "../services/apiClient";
import type { ChangeRequest } from "../types/change";

const ChangeListPage = () => {
  const [changes, setChanges] = useState<ChangeRequest[]>(sampleChanges);
  const [query, setQuery] = useState("");

  useEffect(() => {
    apiClient
      .getChanges()
      .then((data) => setChanges(data))
      .catch(() => {
        // Keep sample data fallback for polished default UI.
      });
  }, []);

  const filtered = useMemo(
    () =>
      changes.filter(
        (change) =>
          change.title.toLowerCase().includes(query.toLowerCase()) ||
          change.description.toLowerCase().includes(query.toLowerCase()) ||
          change.id.toLowerCase().includes(query.toLowerCase())
      ),
    [changes, query]
  );

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: "column", sm: "row" }} justifyContent="space-between" spacing={2}>
        <Box>
          <Typography variant="h4">Change Requests</Typography>
          <Typography color="text.secondary">{filtered.length} of {changes.length} changes</Typography>
        </Box>
        <Button component={RouterLink} to="/changes/new" variant="contained" startIcon={<AddOutlinedIcon />}>
          New Change
        </Button>
      </Stack>
      <TextField
        placeholder="Search by title, description, or change number"
        value={query}
        onChange={(event: ChangeEvent<HTMLInputElement>) => setQuery(event.target.value)}
        InputProps={{ startAdornment: <SearchOutlinedIcon sx={{ mr: 1, color: "text.secondary" }} /> }}
      />
      <ChangeList changes={filtered} />
    </Stack>
  );
};

export default ChangeListPage;
