import { Qualification } from "./qualification";

// Entspricht PersonSummaryDto (für Listenansichten)
export interface EmployeeSummary {
  id: number;
  firstname: string;
  lastname: string;
}

// Entspricht PersonDetailDto (für Detailansichten)
export interface Employee {
  id: number;
  firstname: string;
  lastname: string;
  qualifications: Qualification[];
}

// Entspricht PersonCreateDto & PersonUpdateDto
export interface EmployeeCreateUpdate {
  firstname: string;
  lastname: string;
  qualificationIds: number[];
}
