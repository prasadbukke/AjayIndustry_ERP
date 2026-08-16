# 00 - Documentation Index

## Project

Ajay Industries ERP

## Purpose

This file defines the responsibility of every documentation file so the same information is not maintained in multiple places.

The rule is:

- one fact may be referenced from many documents
- one fact should have only one primary owner
- current status belongs in Project Progress / Project State
- historical work belongs in Sprint Log
- permanent decisions belong in Decision Log
- physical schema belongs in Database Design / Database Relationship
- business process belongs in Business Flow / Transaction Flow

---

# Canonical Documents

| File | Primary Responsibility |
|---|---|
| 01_ProjectVision.md | Long-term project vision, goals and engineering principles |
| 02_ProjectProgress.md | Current module completion status only |
| 02_Requirements.md | Functional scope and module requirements |
| 03_BusinessFlow.md | High-level business process sequence |
| 04_DatabaseDesign.md | Actual database tables, fields and database rules |
| 05_API.md | API strategy and future API contracts |
| 06_UIStandards.md | UI layout and interaction standards |
| 07_CodingStandards.md | Coding rules, layering rules and development/documentation process |
| 08_Deployment.md | Deployment and environment notes |
| 09_SprintLog.md | Chronological development history |
| 10_Architecture.md | Frozen architecture and dependency/runtime flow |
| 11_ComponentLibrary.md | Implemented reusable technical/UI patterns |
| 12_DecisionLog.md | Final architecture/business decisions and reasons |
| 13_ProjectState.md | Exact handoff snapshot of the current codebase |
| 14_UITheme.md | Visual theme tokens and UI styling direction |
| 15_MasterDependency.md | Master-to-master and master-to-transaction dependencies |
| 16_ProjectRoadmap.md | Module order and next development phases |
| 16_DatabaseRelationship.md | Physical/business entity relationships |
| 17_ModuleBlueprint.md | Reusable implementation blueprint for modules |
| 18_TransactionFlow.md | Transaction lifecycle, status handoff and stock/accounting effects |
| 19_ProductionWorkflow.md | Future production-specific process design |
| 20_WorkflowEngine.md | Future generic database-driven workflow design |

---

# Removed Duplicate Documents

The following duplicate documents should no longer be maintained:

- `14_CodingStandards.md`
  - merged into `07_CodingStandards.md`
- `15_ProjectVision.md`
  - module scope belongs in `02_Requirements.md`
  - project vision belongs in `01_ProjectVision.md`

The old file `16. ProjectRoadmap.md` should be renamed to:

`16_ProjectRoadmap.md`

---

# Documentation Update Rule

When a module is completed, update only the documents whose responsibility changed.

Minimum update set:

1. `02_ProjectProgress.md`
2. `09_SprintLog.md`
3. `12_DecisionLog.md` when a new permanent decision was made
4. `13_ProjectState.md`
5. `16_ProjectRoadmap.md`

Also update when relevant:

- Database changed → `04_DatabaseDesign.md`, `16_DatabaseRelationship.md`
- Business flow changed → `03_BusinessFlow.md`, `18_TransactionFlow.md`
- New reusable UI/technical pattern → `06_UIStandards.md`, `11_ComponentLibrary.md`
- New module implementation pattern → `17_ModuleBlueprint.md`
- Architecture changed → `10_Architecture.md` only after explicit architecture approval

---

# Current Documentation Milestone

Current completed transaction milestone:

Purchase Order

Next selected module:

GRN - Goods Receipt Note
