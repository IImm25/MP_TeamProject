import { Qualification } from "./qualification";

export interface EmployeeSummary {
  id: number;
  firstname: string;
  lastname: string;
}

export interface Employee {
  id: number;
  firstname: string;
  lastname: string;
  qualifications: Qualification[];
}

export interface EmployeeCreateUpdate {
  firstname: string;
  lastname: string;
  qualificationIds: number[];
}
