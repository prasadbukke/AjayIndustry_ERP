# 20 - Workflow Engine

## Status

Future architecture direction.

Not yet implemented as a generic ERP workflow engine.

Current Purchase Order workflow is enforced directly by PurchaseOrderService.

---

# Future Goal

Database-driven workflow.

Avoid hardcoding complex production stages when a configurable workflow becomes necessary.

---

# Every Future Workflow May Contain

- Stage
- Sequence
- Status
- Assigned User
- Assigned Machine
- Start Time
- End Time
- Duration
- Remarks

---

# Generic Pipeline

Pending

↓

Running

↓

Completed

↓

Next Stage

---

# Supported Future Actions

- Start
- Pause
- Resume
- Complete
- Reject
- Rework
- Cancel
- Hold

These are future generic workflow actions and should not be assumed to exist on every transaction.

---

# Current Purchase Order Exception

Purchase Order currently uses explicit service-enforced transitions:

Draft
→ Confirmed
→ Sent

Future GRN will drive receipt states.

Purchase Order does not currently use the generic Workflow Engine.

---

# Future Tracking

- Stage History
- Audit Trail
- Current Stage
- Overall Progress
- Expected Completion
- Delay

---

# Future Dashboard

- Running Orders
- Completed Orders
- Pending Orders
- Delayed Orders
- Rejected Orders
- Machine Utilization
- Operator Utilization

---

# Future Extensions

- Workflow Designer
- Approval Engine
- Notifications
- Escalation
