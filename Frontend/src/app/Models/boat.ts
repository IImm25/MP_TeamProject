import { Employee, EmployeeSummary } from "./employee";
import { Task, TaskSummary, TaskTool } from "./task";

export interface PlanRequest {
  maxTime: number;
  boatNumber: number;
  taskItemIds: number[];
  personIds: number[];
  toolIds: number[];
}

export interface PlanResponse {
  totalTime: number;
  boats: Boat[];
  toolDiff: RequirementDiff[];
  qualificationDiff: RequirementDiff[];
}

export interface Boat {
  taskItems: TaskSummary[];
  persons: EmployeeSummary[];
  tools: TaskTool[];
}

export interface RequirementDiff {
  id: number;
  required: number;
  available: number;
}
