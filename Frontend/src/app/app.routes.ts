import { Routes } from '@angular/router';
import { Tasks } from './Ressources/Task/tasks/tasks';
import { Employees } from './Ressources/Employee/employees/employees';
import { Qualifications } from './Ressources/Qualification/qualifications/qualifications';
import { Scheduler } from './Schedule/scheduler/scheduler';
import { ScheduleView } from './Schedule/schedule-view/schedule-view';
import { Tools } from './Ressources/Tool/tools/tools';

export const routes: Routes = [
  { path: '', redirectTo: 'scheduler', pathMatch: 'full' },
  { path: 'tasks', component: Tasks },
  { path: 'employees', component: Employees },
  { path: 'qualifications', component: Qualifications },
  { path: 'tools', component: Tools },
  { path: 'scheduler', component: Scheduler },
  { path: 'schedule-view', component: ScheduleView },
];
