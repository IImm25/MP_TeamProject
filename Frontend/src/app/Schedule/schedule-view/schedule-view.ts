import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleService } from '../schedule-service';
import { HttpService } from '../../Services/http-service';
import { Task } from '../../Models/task';
import { Employee } from '../../Models/employee';
import { Tool } from '../../Models/tool';
import { CardModule } from 'primeng/card';
import { AccordionModule } from 'primeng/accordion';
import { ChipModule } from 'primeng/chip';
import { TranslateModule } from '@ngx-translate/core';
import { DialogTask } from '../../Ressources/Task/dialog-task/dialog-task';
import { DialogEmployee } from '../../Ressources/Employee/dialog-employee/dialog-employee';
import { DialogTool } from '../../Ressources/Tool/dialog-tool/dialog-tool';
import { Boat } from '../../Models/boat';

@Component({
  selector: 'app-schedule-view',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    AccordionModule,
    ChipModule,
    TranslateModule,
    DialogTask,
    DialogEmployee,
    DialogTool,
  ],
  templateUrl: './schedule-view.html',
  styleUrl: './schedule-view.css',
})
export class ScheduleView {
  private planService = inject(ScheduleService);
  private http = inject(HttpService);

  boats = computed(() => this.planService.currentPlan() || []);
  allTools = signal<Tool[]>([]);

  selectedTask = signal<Task | null>(null);
  taskVisible = signal(false);

  selectedEmployee = signal<Employee | null>(null);
  employeeVisible = signal(false);

  selectedTool = signal<Tool | null>(null);
  toolVisible = signal(false);

  ngOnInit() {
    this.http.getTools().subscribe((tools) => this.allTools.set(tools));
  }

  openTask(task: Task) {
    this.selectedTask.set(task);
    this.taskVisible.set(true);
  }

  openEmployee(emp: Employee) {
    this.selectedEmployee.set(emp);
    this.employeeVisible.set(true);
  }

  openTool(toolId: number) {
    const tool = this.allTools().find((t) => t.id === toolId);
    if (tool) {
      this.selectedTool.set(tool);
      this.toolVisible.set(true);
    }
  }

  getToolName(id: number) {
    return this.allTools().find((t) => t.id === id)?.name || `Tool #${id}`;
  }

  isBoatEmpty(boat: Boat): boolean {
    return boat.taskItems.length === 0 && boat.people.length === 0 && boat.tools.length === 0;
  }
}
