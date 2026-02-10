import AddOutlinedIcon from "@mui/icons-material/AddOutlined";
import ContentCopyOutlinedIcon from "@mui/icons-material/ContentCopyOutlined";
import DeleteOutlineOutlinedIcon from "@mui/icons-material/DeleteOutlineOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import { Box, Button, Card, CardContent, Chip, Grid, IconButton, Stack, Typography } from "@mui/material";

const templates = [
  {
    id: "tpl-1",
    name: "Windows Security Patch",
    description: "Deploy monthly Windows security patches as part of regular cycle.",
    tags: ["Standard", "Server", "Risk: Low"]
  },
  {
    id: "tpl-2",
    name: "Network Firewall Rule Change",
    description: "Modify firewall rules to allow/deny traffic for business services.",
    tags: ["Normal", "Network", "Risk: Medium"]
  },
  {
    id: "tpl-3",
    name: "Database Maintenance",
    description: "Perform scheduled database maintenance including index and stats updates.",
    tags: ["Standard", "Database", "Risk: Low"]
  }
];

const TemplatesPage = () => {
  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: "column", sm: "row" }} justifyContent="space-between" spacing={2}>
        <Box>
          <Typography variant="h4">Change Templates</Typography>
          <Typography color="text.secondary">Pre-defined templates for common change types</Typography>
        </Box>
        <Button variant="contained" startIcon={<AddOutlinedIcon />}>
          New Template
        </Button>
      </Stack>
      <Grid container spacing={2}>
        {templates.map((template) => (
          <Grid item key={template.id} xs={12} md={6}>
            <Card>
              <CardContent>
                <Stack direction="row" justifyContent="space-between" spacing={1}>
                  <Typography variant="h6">{template.name}</Typography>
                  <Stack direction="row" spacing={0.5}>
                    <IconButton aria-label={`Duplicate ${template.name}`} size="small">
                      <ContentCopyOutlinedIcon fontSize="small" />
                    </IconButton>
                    <IconButton aria-label={`Edit ${template.name}`} size="small">
                      <EditOutlinedIcon fontSize="small" />
                    </IconButton>
                    <IconButton aria-label={`Delete ${template.name}`} size="small" color="error">
                      <DeleteOutlineOutlinedIcon fontSize="small" />
                    </IconButton>
                  </Stack>
                </Stack>
                <Stack direction="row" spacing={1} my={1} flexWrap="wrap">
                  {template.tags.map((tag) => (
                    <Chip key={tag} label={tag} size="small" />
                  ))}
                </Stack>
                <Typography color="text.secondary">{template.description}</Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Stack>
  );
};

export default TemplatesPage;
