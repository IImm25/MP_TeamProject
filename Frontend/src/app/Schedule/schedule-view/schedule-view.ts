import { Component, inject, signal, OnInit, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleService } from '../schedule-service';
import { HttpService } from '../../Services/http-service';
import { Task, TaskSummary, TaskTool } from '../../Models/task';
import { Employee, EmployeeSummary } from '../../Models/employee';
import { Tool } from '../../Models/tool';
import { CardModule } from 'primeng/card';
import { ChipModule } from 'primeng/chip';
import { PopoverModule } from 'primeng/popover';
import { DatePickerModule } from 'primeng/datepicker';
import { FormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { TranslateModule } from '@ngx-translate/core';
import { DialogTask } from '../../Ressources/Task/dialog-task/dialog-task';
import { DialogEmployee } from '../../Ressources/Employee/dialog-employee/dialog-employee';
import { DialogTool } from '../../Ressources/Tool/dialog-tool/dialog-tool';
import { Boat, BoatSchedule, PlanResponse, TaskSchedule, TravelTime } from '../../Models/boat';
import { forkJoin, switchMap } from 'rxjs';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { Qualification } from '../../Models/qualification';
import { Button } from 'primeng/button';
import { WeatherDialog } from '../weather-dialog/weather-dialog';

@Component({
  selector: 'app-schedule-view',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ChipModule,
    PopoverModule,
    DatePickerModule,
    FormsModule,
    AccordionModule,
    TranslateModule,
    DialogTask,
    DialogEmployee,
    DialogTool,
    ToastModule,
    Button,
    WeatherDialog
  ],
  providers: [MessageService],
  templateUrl: './schedule-view.html',
  styleUrl: './schedule-view.css',
})
export class ScheduleView implements OnInit {
  private planService = inject(ScheduleService);
  private http = inject(HttpService);

  // Steuersignale für Ansicht und Datum
  currentView: WritableSignal<'cards' | 'gantt'> = signal('cards');
  selectedDate: WritableSignal<Date> = signal(new Date());

  planResponse: WritableSignal<PlanResponse> = signal({ date: '', createdAt: '', boats: [] });
  boats: WritableSignal<Boat[]> = signal([]);
  unusedResources: WritableSignal<Boat> = signal({ taskSchedules: [], persons: [], tools: [], boatSchedules: [] });

  travelTimes: WritableSignal<TravelTime[]> = signal([]);

  allTasks: WritableSignal<Task[]> = signal([]);
  allEmployees: WritableSignal<Employee[]> = signal([]);
  allTools: WritableSignal<Tool[]> = signal([]);
  allQualifications: WritableSignal<Qualification[]> = signal([]);

  // Dialog-Toggles
  selectedTask: WritableSignal<Task | null> = signal(null);
  taskVisible: WritableSignal<boolean> = signal(false);
  taskType: WritableSignal<'Accident' | 'Detail'> = signal('Detail');
  selectedEmployee: WritableSignal<Employee | null> = signal(null);
  employeeVisible: WritableSignal<boolean> = signal(false);
  selectedTool: WritableSignal<Tool | null> = signal(null);
  requiredAmount: WritableSignal<number> = signal(0);
  toolVisible: WritableSignal<boolean> = signal(false);
  weatherVisible: WritableSignal<boolean> = signal(false);

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

      const cachedPlan = this.planService.loadBoatsFromStorage();
      if (cachedPlan) {
        this.planResponse.set(cachedPlan);
        this.boats.set(cachedPlan.boats || []);
        this.selectedDate.set(new Date(cachedPlan.date));
        this.unusedResources.set(this.calculateUnusedRessources());
        localStorage.removeItem('boats');
      } else {
        this.loadPlanForDate(this.selectedDate());
      }
      this.loadTravelTimes();
    });
  }

  onDateChange(newDate: Date) {
    this.loadPlanForDate(newDate);
  }

  loadPlanForDate(date: Date) {
    const formattedDate = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;

    this.http.getPlanByDate(formattedDate).subscribe({
      next: (plan) => {
        this.planResponse.set(plan);
        this.boats.set(plan.boats || []);
        this.unusedResources.set(this.calculateUnusedRessources());
      },
      error: () => {
        // Falls kein Plan existiert, State leeren
        this.planResponse.set({ date: formattedDate, createdAt: '', boats: [] });
        this.boats.set([]);
        this.unusedResources.set({ taskSchedules: [], persons: [], tools: [], boatSchedules: [] });
      },
    });
  }

  // --- HILFSMETHODEN FÜR DAS GANTT-CHART & DIALOGE ---
  calculatePosition(startTime: string): string {
    if (!startTime || startTime === 'string') return '0%';
    const [hours, minutes] = startTime.split(':').map(Number);
    const decimalTime = hours + minutes / 60;
    return `${Math.max(0, Math.min(95, ((decimalTime - 7) / 10) * 100))}%`;
  }

  calculateWidth(durationHours: number): string {
    return `${Math.max(5, Math.min(100, (durationHours / 10) * 100))}%`;
  }

  formatDuration(decimalHours: number): string {
    if (!decimalHours) return '0 h';
    const hours = Math.floor(decimalHours);
    const minutes = Math.round((decimalHours - hours) * 60);
    return hours > 0 && minutes > 0
      ? `${hours}h ${minutes}m`
      : hours > 0
        ? `${hours}h`
        : `${minutes}m`;
  }

  openTask(taskSummary: TaskSummary | null, mode: 'Accident' | 'Detail') {
    if (!taskSummary && mode === 'Accident') {
      this.selectedTask.set(null);
      this.taskVisible.set(true);
      this.taskType.set('Accident');
    } else if (taskSummary && mode === 'Detail') {
      const fullTask = this.allTasks().find((t) => t.id === taskSummary.id);
      if (fullTask) {
        this.selectedTask.set(fullTask);
        this.taskVisible.set(true);
        this.taskType.set(mode);
      }
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

  getToolName(id: number): string {
    return this.allTools().find((t) => t.id === id)?.name || `Tool #${id}`;
  }

  private calculateUnusedRessources(): Boat {
    const boats = this.planResponse().boats || [];

    // Wir erstellen Sets aus allen IDs, die in IRGENDEINEM Boot verplant wurden
    const used = {
      tasks: new Set(
        boats.flatMap((b) =>
          (b.taskSchedules || []).flatMap((ts) => (ts.task?.id != null ? [ts.task.id] : [])),
        ),
      ),
      people: new Set(boats.flatMap((b) => (b.persons ?? []).map((p) => p.id))),
      tools: new Set(boats.flatMap((b) => (b.tools ?? []).map((t) => t.toolId))),
    };

    // 1. Unbenutzte Aufgaben: Alle geladenen Tasks, die in keinem Boot vorkommen
    const unusedTasks: TaskSchedule[] = this.allTasks()
      .filter((t) => !used.tasks.has(t.id))
      .map((t) => ({
        task: {
          id: t.id,
          name: t.name,
          durationHours: t.durationHours,
          isCompleted: t.isCompleted,
          executionIntervalStart: t.executionIntervalStart,
          executionIntervalEnd: t.executionIntervalEnd,
        } as TaskSummary,
        startTime: '',
      }));

    // 2. Unbenutzte Mitarbeiter: Alle geladenen Employees, die in keinem Boot sitzen
    const unusedPersons: EmployeeSummary[] = this.allEmployees()
      .filter((e) => !used.people.has(e.id))
      .map((e) => ({
        id: e.id,
        firstname: e.firstname,
        lastname: e.lastname,
      }));

    // 3. Unbenutzte Werkzeuge: Alle geladenen Tools, die in keinem Boot gebraucht werden
    const unusedTools: TaskTool[] = this.allTools()
      .filter((t) => !used.tools.has(t.id))
      .map((t) => ({
        toolId: t.id,
        requiredAmount: 0,
      }));

    return {
      taskSchedules: unusedTasks,
      persons: unusedPersons,
      tools: unusedTools,
      boatSchedules: [],
    };
  }

  loadTravelTimes() {
    const calculatedTravelTimes: TravelTime[] = [];

    this.boats().forEach((boat, boatIndex) => {
      if (
        !boat.boatSchedules ||
        boat.boatSchedules.length === 0 ||
        !boat.taskSchedules ||
        boat.taskSchedules.length === 0
      ) {
        return;
      }

      // 1. Sortiere die Aufgaben und Bootsfahrten chronologisch
      const sortedTasks: TaskSchedule[] = [...boat.taskSchedules].sort((a, b) =>
        a.startTime.localeCompare(b.startTime),
      );

      const BoatSchedules: BoatSchedule[] = [...boat.boatSchedules].sort((a, b) =>
        a.departure.localeCompare(b.departure),
      );

      // --- A) ERSTE FAHRT: HINFAHRT (Vom Hafen zur 1. Aufgabe) ---
      const firstTask = sortedTasks[0];
      const outboundMinutes =
        this.durationStringToMinutes(firstTask.startTime) -
        this.durationStringToMinutes(BoatSchedules[0].departure);
      const outboundDuration = outboundMinutes / 60;

      if (outboundDuration > 0) {
        calculatedTravelTimes.push({
          boatNumber: boatIndex + 1,
          travelTime: BoatSchedules[0].departure,
          travelDuration: outboundDuration,
        });
      }

      // --- B) ZWISCHENFAHRTEN & RÜCKFAHRT SCHLEIFE ---
      for (let travelIndex = 0; travelIndex < sortedTasks.length; travelIndex++) {
        const currentTask = sortedTasks[travelIndex];
        const currentTaskEndMinutes =
          this.durationStringToMinutes(currentTask.startTime) +
          Math.round(currentTask.task.durationHours * 60);

        if (travelIndex < sortedTasks.length - 1) {
          // --- FAHRTEN ZWISCHEN DEN AUFGABEN ---
          const nextTaskStartMinutes = this.durationStringToMinutes(
            sortedTasks[travelIndex + 1].startTime,
          );
          const transitMinutes = nextTaskStartMinutes - currentTaskEndMinutes;
          const transitDuration = transitMinutes / 60;

          if (transitDuration > 0) {
            calculatedTravelTimes.push({
              boatNumber: boatIndex + 1,
              travelTime: this.addDurationToTime(
                currentTask.startTime,
                currentTask.task.durationHours,
              ),
              travelDuration: transitDuration,
            });
          }
        } else {
          // --- C) LETZTE FAHRT: RÜCKFAHRT (Von letzter Aufgabe zum Hafen) ---
          const boatArrivalMinutes = this.durationStringToMinutes(
            BoatSchedules[BoatSchedules.length - 1].arrival,
          );
          const inboundMinutes = boatArrivalMinutes - currentTaskEndMinutes;
          const inboundDuration = inboundMinutes / 60;

          if (inboundDuration > 0) {
            calculatedTravelTimes.push({
              boatNumber: boatIndex + 1,
              travelTime: this.addDurationToTime(
                currentTask.startTime,
                currentTask.task.durationHours,
              ),
              travelDuration: inboundDuration,
            });
          }
        }
      }
    });

    this.travelTimes.set(calculatedTravelTimes);
  }

  // Hilfsmethode: Wandelt "HH:mm:ss" in reine Minuten um
  private durationStringToMinutes(time: string): number {
    if (!time || time === 'string') return 0;
    const [hours, minutes] = time.split(':').map(Number);
    return hours * 60 + minutes;
  }

  //time-format: "HH:mm"
  private addDurationToTime(time: string, duration: number): string {
    const [hours, minutes] = time.split(':').map(Number);
    const totalMinutes = hours * 60 + minutes + duration * 60;
    const newHours = Math.floor(totalMinutes / 60);
    const newMinutes = totalMinutes % 60;
    return `${String(newHours).padStart(2, '0')}:${String(newMinutes).padStart(2, '0')}:00`;
  }
}
