import { Routes } from '@angular/router';
import { Tasks } from './tasks/tasks';
import { Employees } from './employees/employees';
import { Qualifications } from './qualifications/qualifications';
import { Scheduler } from './scheduler/scheduler';
import { ScheduleView } from './schedule-view/schedule-view';
import { Tools } from './tools/tools';

export const routes: Routes = [
  { path: '', redirectTo: 'scheduler', pathMatch: 'full' },
  { path: 'tasks', component: Tasks },
  { path: 'employees', component: Employees },
  { path: 'qualifications', component: Qualifications },
  { path: 'tools', component: Tools },
  { path: 'scheduler', component: Scheduler },
  { path: 'schedule-view', component: ScheduleView },
];
