import ArrowBackOutlinedIcon from "@mui/icons-material/ArrowBackOutlined";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import CloseOutlinedIcon from "@mui/icons-material/CloseOutlined";
import SendOutlinedIcon from "@mui/icons-material/SendOutlined";
import {
  Alert,
  Box,
  Button,
  Chip,
  Stack,
  TextField,
  Typography
} from "@mui/material";
import { useEffect, useMemo, useState, type ChangeEvent } from "react";
import { Link as RouterLink, useParams } from "react-router-dom";
import ChangeDetail from "../components/ChangeDetail";
import ChangeForm from "../components/ChangeForm";
import { sampleApprovals, sampleChanges } from "../data/sampleData";
import { apiClient } from "../services/apiClient";
import type { Approval, ApprovalStatus, ChangeCreateDto, ChangeRequest, ChangeUpdateDto } from "../types/change";

const defaultForm: ChangeCreateDto = {
  title: "",
  description: "",
  priority: "P3",
  riskLevel: "Low",
  plannedStart: "",
  plannedEnd: ""
};

const ChangeDetailPage = () => {
  const { id } = useParams();
  const isNew = id === "new" || !id;
  const [change, setChange] = useState<ChangeRequest | null>(isNew ? null : sampleChanges[0]);
  const [approvals, setApprovals] = useState<Approval[]>(sampleApprovals.filter((item) => item.changeRequestId === id));
  const [formData, setFormData] = useState<ChangeCreateDto>(defaultForm);
  const [decisionComment, setDecisionComment] = useState("");
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    if (isNew || !id) {
      return;
    }

    apiClient
      .getChangeById(id)
      .then((data) => {
        setChange(data);
        setFormData({
          title: data.title,
          description: data.description,
          priority: data.priority,
          riskLevel: data.riskLevel,
          plannedStart: data.plannedStart,
          plannedEnd: data.plannedEnd
        });
      })
      .catch(() => {
        const fallback = sampleChanges.find((item) => item.id === id) ?? sampleChanges[0];
        setChange(fallback);
        setFormData({
          title: fallback.title,
          description: fallback.description,
          priority: fallback.priority,
          riskLevel: fallback.riskLevel,
          plannedStart: fallback.plannedStart,
          plannedEnd: fallback.plannedEnd
        });
      });

    apiClient
      .getApprovals(id)
      .then((data) => setApprovals(data))
      .catch(() => setApprovals(sampleApprovals.filter((item) => item.changeRequestId === id)));
  }, [id, isNew]);

  const canTakeDecision = useMemo(() => change?.status === "PendingApproval", [change?.status]);

  const handleFormChange = (field: keyof ChangeCreateDto, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    try {
      if (isNew) {
        const created = await apiClient.createChange(formData);
        console.log("Created change", created);
        setMessage("Change request created successfully.");
      } else if (id && change) {
        const payload: ChangeUpdateDto = {
          ...formData,
          status: change.status
        };
        const updated = await apiClient.updateChange(id, payload);
        console.log("Updated change", updated);
        setChange(updated);
        setMessage("Change request updated successfully.");
      }
    } catch {
      setMessage("Action completed in demo mode with sample data.");
    }
  };

  const handleSubmitForApproval = async () => {
    if (!id) {
      return;
    }
    try {
      const updated = await apiClient.submitChange(id);
      console.log("Submitted for approval", updated);
      setChange(updated);
    } catch {
      setChange((prev) => (prev ? { ...prev, status: "PendingApproval" } : prev));
    }
  };

  const handleDecision = async (approvalId: string, status: ApprovalStatus) => {
    if (!id) {
      return;
    }

    try {
      const updated = await apiClient.decideApproval(id, approvalId, { status, comment: decisionComment });
      console.log("Approval decision", updated);
      setApprovals((prev) => prev.map((item) => (item.id === approvalId ? updated : item)));
    } catch {
      setApprovals((prev) =>
        prev.map((item) =>
          item.id === approvalId ? { ...item, status, comment: decisionComment, decisionAt: new Date().toISOString() } : item
        )
      );
    }
  };

  return (
    <Stack spacing={2}>
      <Button component={RouterLink} to="/changes" startIcon={<ArrowBackOutlinedIcon />} sx={{ alignSelf: "flex-start" }}>
        Back to list
      </Button>
      <Typography variant="h4">{isNew ? "New Change Request" : "Change Detail"}</Typography>
      {message ? <Alert severity="success">{message}</Alert> : null}

      <ChangeForm value={formData} onChange={handleFormChange} onSubmit={handleSave} submitLabel={isNew ? "Submit for Approval" : "Save Changes"} />

      {!isNew && change ? (
        <>
          {change.status === "Draft" ? (
            <Button variant="contained" startIcon={<SendOutlinedIcon />} onClick={handleSubmitForApproval} sx={{ alignSelf: "flex-start" }}>
              Submit for approval
            </Button>
          ) : null}

          <ChangeDetail
            change={change}
            approvals={approvals}
            actions={
              change.status === "Approved" || change.status === "Rejected" ? (
                <Chip label={`Final state: ${change.status}`} color={change.status === "Approved" ? "success" : "error"} />
              ) : null
            }
          />

          <Box>
            <Typography variant="h6" gutterBottom>
              Approval Actions
            </Typography>
            <TextField
              label="Approval comment"
              multiline
              fullWidth
              minRows={2}
              value={decisionComment}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setDecisionComment(event.target.value)}
            />
            <Stack direction="row" spacing={1} mt={1}>
              <Button
                variant="contained"
                color="success"
                startIcon={<CheckCircleOutlineIcon />}
                disabled={!canTakeDecision || approvals.length === 0}
                onClick={() => approvals[0] && handleDecision(approvals[0].id, "Approved")}
              >
                Approve
              </Button>
              <Button
                variant="outlined"
                color="error"
                startIcon={<CloseOutlinedIcon />}
                disabled={!canTakeDecision || approvals.length === 0}
                onClick={() => approvals[0] && handleDecision(approvals[0].id, "Rejected")}
              >
                Reject
              </Button>
            </Stack>
          </Box>
        </>
      ) : null}
    </Stack>
  );
};

export default ChangeDetailPage;
