import { Component, inject, signal, computed, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleService } from '../schedule-service';
import { HttpService } from '../../Services/http-service';
import { Task, TaskSummary } from '../../Models/task';
import { Employee, EmployeeSummary } from '../../Models/employee';
import { Tool } from '../../Models/tool';
import { CardModule } from 'primeng/card';
import { AccordionModule } from 'primeng/accordion';
import { ChipModule } from 'primeng/chip';
import { TranslateModule } from '@ngx-translate/core';
import { DialogTask } from '../../Ressources/Task/dialog-task/dialog-task';
import { DialogEmployee } from '../../Ressources/Employee/dialog-employee/dialog-employee';
import { DialogTool } from '../../Ressources/Tool/dialog-tool/dialog-tool';
import { Boat, PlanRequest, PlanResponse } from '../../Models/boat';
import { forkJoin, switchMap } from 'rxjs';

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

  planResponse = signal<PlanResponse>(
    this.planService.loadBoatsFromStorage() ?? { totalTime: 0, boats: [] },
  );
  boats: WritableSignal<Boat[]> = signal([]);
  allTasks = signal<Task[]>([]);
  allEmployees = signal<Employee[]>([]);
  allTools = signal<Tool[]>([]);

  selectedTask = signal<Task | null>(null);
  taskVisible = signal(false);

  selectedEmployee = signal<Employee | null>(null);
  employeeVisible = signal(false);

  selectedTool = signal<Tool | null>(null);
  requiredAmount = signal(0);
  toolVisible = signal(false);

  ngOnInit(): void {
    forkJoin({
      tasks: this.http
        .getTasks()
        .pipe(switchMap((tasks) => forkJoin(tasks.map((t) => this.http.getTaskById(t.id))))),
      tools: this.http.getTools(),
      employees: this.http
        .getEmployees()
        .pipe(
          switchMap((summaries) => forkJoin(summaries.map((e) => this.http.getEmployeeById(e.id)))),
        ),
    }).subscribe(({ tasks, tools, employees }) => {
      this.allTasks.set(tasks);
      this.allTools.set(tools);
      this.allEmployees.set(employees);

      const raw = this.planService.loadBoatsFromStorage();
      console.log('raw plan response:', raw);

      const unusedBoat = this.getUnusedRessources();

      this.boats.set([unusedBoat, ...this.planResponse().boats]);

      console.log(this.boats());
    });
  }

  openTask(taskSummary: TaskSummary) {
    const fullTask = this.allTasks().find((t) => t.id === taskSummary.id);

    if (fullTask) {
      this.selectedTask.set(fullTask);
      this.taskVisible.set(true);
    }
  }

  openEmployee(emp: EmployeeSummary) {
    const fullEmployee = this.allEmployees().find((e) => e.id === emp.id);
    if (fullEmployee) {
      this.selectedEmployee.set(fullEmployee);
      this.employeeVisible.set(true);
    }
  }

  openTool(toolId: number, requiredAmount: number) {
    const tool = this.allTools().find((t) => t.id === toolId);

    if (tool) {
      this.selectedTool.set(tool);
      this.requiredAmount.set(requiredAmount);
      this.toolVisible.set(true);
    }
  }

  getToolName(id: number) {
    return this.allTools().find((t) => t.id === id)?.name || `Tool #${id}`;
  }

  isBoatEmpty(boat: Boat): boolean {
    return boat.taskItems.length === 0 && boat.persons.length === 0 && boat.tools.length === 0;
  }

  getUnusedRessources(): Boat {
    const boats = this.planResponse().boats;
    const request = this.planService.loadRequestFromStorage();

    if (!request) return { taskItems: [], persons: [], tools: [] };

    const used = {
      tasks: new Set(boats.flatMap((b) => (b.taskItems ?? []).map((t) => t.id))),
      people: new Set(boats.flatMap((b) => (b.persons ?? []).map((p) => p.id))),
      tools: new Set(boats.flatMap((b) => (b.tools ?? []).map((t) => t.toolId))),
    };

    console.log(used);

    return {
      taskItems: this.allTasks().filter(
        (t) => request.taskItemIds.includes(t.id) && !used.tasks.has(t.id),
      ),
      persons: this.allEmployees().filter(
        (e) => request.personIds.includes(e.id) && !used.people.has(e.id),
      ),
      tools: this.allTools()
        .filter((t) => request.toolIds.includes(t.id))
        .map((t) => {
          const usedAmount = boats
            .flatMap((b) => b.tools ?? [])
            .filter((bt) => bt.toolId === t.id)
            .reduce((sum, bt) => sum + bt.requiredAmount, 0);
          return { toolId: t.id, requiredAmount: t.availableStock - usedAmount };
        })
        .filter((t) => t.requiredAmount > 0),
    };
  }
}
