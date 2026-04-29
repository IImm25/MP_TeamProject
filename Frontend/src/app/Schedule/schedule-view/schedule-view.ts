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
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogTask } from '../../Ressources/Task/dialog-task/dialog-task';
import { DialogEmployee } from '../../Ressources/Employee/dialog-employee/dialog-employee';
import { DialogTool } from '../../Ressources/Tool/dialog-tool/dialog-tool';
import { Boat, PlanResponse } from '../../Models/boat';
import { catchError, forkJoin, map, of, switchMap } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { Qualification } from '../../Models/qualification';

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
    ButtonModule,
    TagModule,
    ToastModule,
  ],
  providers: [MessageService],
  templateUrl: './schedule-view.html',
  styleUrl: './schedule-view.css',
})
export class ScheduleView {
  private planService = inject(ScheduleService);
  private http = inject(HttpService);
  private messageService = inject(MessageService);
  private translate = inject(TranslateService);

  planResponse = signal<PlanResponse>(
    this.planService.loadBoatsFromStorage() ?? {
      totalTime: 0,
      boats: [],
      toolDiff: [],
      qualificationDiff: [],
    },
  );
  boats: WritableSignal<Boat[]> = signal([]);
  allTasks = signal<Task[]>([]);
  allEmployees = signal<Employee[]>([]);
  allTools = signal<Tool[]>([]);
  allQualifications = signal<Qualification[]>([]);

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
      qualifications: this.http.getQualifications(),
    }).subscribe(({ tasks, tools, employees, qualifications }) => {
      this.allTasks.set(tasks);
      this.allTools.set(tools);
      this.allEmployees.set(employees);
      this.allQualifications.set(qualifications);

      const unusedBoat = this.getUnusedRessources();

      this.boats.set([unusedBoat, ...this.planResponse().boats]);

      this.getProblems();
    });
  }

  LoadData() {}

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

  getQualificationName(id: number): string {
    return this.allQualifications().find((q) => q.id === id)?.name || `Qualifikation #${id}`;
  }

  isBoatEmpty(boat: Boat): boolean {
    return boat.taskItems.length === 0 && boat.persons.length === 0 && boat.tools.length === 0;
  }

  getUnusedRessources(): Boat {
    const boats = this.planResponse().boats;
    const request = this.planService.loadRequestFromStorage();
    const toolDiff = this.planResponse().toolDiff;

    if (!request) return { taskItems: [], persons: [], tools: [] };

    const used = {
      tasks: new Set(boats.flatMap((b) => (b.taskItems ?? []).map((t) => t.id))),
      people: new Set(boats.flatMap((b) => (b.persons ?? []).map((p) => p.id))),
      tools: new Set(boats.flatMap((b) => (b.tools ?? []).map((t) => t.toolId))),
    };

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
          return {
            toolId: t.id,
            requiredAmount: toolDiff.find((d) => d.id === t.id)?.required || 0,
          };
        })
        .filter((t) => t.requiredAmount > 0),
    };
  }

  getProblems() {
    const diffQual = this.planResponse().qualificationDiff || [];
    const diffTool = this.planResponse().toolDiff || [];

    diffQual.forEach((qual) => {
      if (qual.required > qual.available) {
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('MESSAGES.MISSING_QUALIFICATION'),
          detail: this.getQualificationName(qual.id),
          sticky: true,
        });
      }
    });

    diffTool.forEach((tool) => {
      if (tool.required > tool.available) {
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('MESSAGES.MISSING_TOOL'),
          detail: this.getToolName(tool.id),
          sticky: true,
        });
      }
    });
  }
}
