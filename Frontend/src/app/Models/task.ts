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
}

export interface Task {
  id: number;
  name: string;
  durationHours: number;
  requiredQualifications: TaskQualification[];
  requiredTools: TaskTool[];
}
