import { Turbine } from "./turbine";

export interface TaskQualification {
  qualificationId: number;
  requiredAmount: number;
}

export interface TaskTool {
  toolId: number;
  requiredAmount: number;
}

export interface TaskSummary {
  id: number;
  name: string;
  durationHours: number;
  isCompleted: boolean;
  executionIntervalStart: string;
  executionIntervalEnd: string;
}

export interface Task {
  id: number;
  name: string;
  durationHours: number;
  isCompleted: boolean;
  executionIntervalStart: string;
  executionIntervalEnd: string;
  location: Turbine;
  requiredQualifications: TaskQualification[];
  requiredTools: TaskTool[];
}

export interface TaskCreate {
  name: string;
  durationHours: number;
  executionIntervalStart: string;
  executionIntervalEnd: string;
  locationId: number;
  requiredQualifications: TaskQualification[];
  requiredTools: TaskTool[];
}

export interface TaskUpdate {
  name?: string;
  durationHours?: number;
  isCompleted?: boolean;
  executionIntervalStart?: string;
  executionIntervalEnd?: string;
  locationId?: number;
  requiredQualifications?: TaskQualification[];
  requiredTools?: TaskTool[];
}
