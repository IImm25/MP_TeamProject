import { Component, inject, OnInit, signal, ViewChild, WritableSignal } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule, FormControl } from '@angular/forms';
import { HttpService } from '../../Services/http-service';
import { TaskSummary, Task } from '../../Models/task';
import { EmployeeSummary, Employee } from '../../Models/employee';
import { Tool } from '../../Models/tool';
import { PlanRequest } from '../../Models/boat';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG
import { StepperModule } from 'primeng/stepper';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { InputNumberModule } from 'primeng/inputnumber';
import { CardModule } from 'primeng/card';

// Dialoge
import { DialogTask } from '../../Ressources/Task/dialog-task/dialog-task';
import { DialogEmployee } from '../../Ressources/Employee/dialog-employee/dialog-employee';
import { DialogTool } from '../../Ressources/Tool/dialog-tool/dialog-tool';
import { ChipModule } from 'primeng/chip';

@Component({
  selector: 'app-scheduler',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    StepperModule,
    ButtonModule,
    TableModule,
    InputNumberModule,
    CardModule,
    DialogTask,
    DialogEmployee,
    DialogTool,
    ChipModule
  ],
  templateUrl: './scheduler.html',
  styleUrl: './scheduler.css'
})
export class Scheduler implements OnInit {

  @ViewChild(DialogTool) dialogComp!: DialogTool;

  private fb = inject(FormBuilder);
  private http = inject(HttpService);
  private router = inject(Router);

  tasks: WritableSignal<TaskSummary[]> = signal<TaskSummary[]>([]);
  employees: WritableSignal<EmployeeSummary[]> = signal<EmployeeSummary[]>([]);
  tools: WritableSignal<Tool[]> = signal<Tool[]>([]);

  taskVisible: WritableSignal<boolean> = signal(false);
  employeeVisible: WritableSignal<boolean> = signal(false);
  toolVisible: WritableSignal<boolean> = signal(false);

  selectedTasksTable: TaskSummary[] = [];
  selectedEmployeesTable: EmployeeSummary[] = [];
  selectedToolTable: Tool[] = [];

  toolType: WritableSignal<'Edit' | 'New'> = signal('New');

  selectedTask: WritableSignal<Task | null> = signal(null);
  taskType: WritableSignal<'Edit' | 'New' | 'Detail'> = signal('New');

  schedulerForm = this.fb.group({
    maxTime: new FormControl<number>(8, [Validators.required, Validators.min(1)]),
    boatAmount: new FormControl<number>(1, [Validators.required, Validators.min(1)]),
    selectedTasks: new FormControl<TaskSummary[]>([], { nonNullable: true, validators: [Validators.required] }),
    selectedEmployees: new FormControl<EmployeeSummary[]>([], { nonNullable: true, validators: [Validators.required] }),
    selectedTools: new FormControl<Tool[]>([], { nonNullable: true, validators: [Validators.required] })
  });


  ngOnInit() {
    this.loadData();
  }

  onTaskSelectionChange(event: TaskSummary[]) {
    this.selectedTasksTable = event;
    this.schedulerForm.patchValue({ selectedTasks: event });
  }

  onEmployeeSelectionChange(event: EmployeeSummary[]) {
    this.selectedEmployeesTable = event;
    this.schedulerForm.patchValue({ selectedEmployees: event });
  }

   onToolSelectionChange(event: Tool[]) {
    this.selectedToolTable = event;
    this.schedulerForm.patchValue({ selectedTools: event });
  }

  loadData() {
    this.http.getTasks().subscribe(data => this.tasks.set(data));
    this.http.getTools().subscribe(data => this.tools.set(data));
    this.http.getEmployees().subscribe((summaries) => {
    const employeeList = summaries as Employee[];
    this.employees.set(employeeList);

    employeeList.forEach(employee => {
      this.http.getEmployeeById(employee.id).subscribe(fullDetails => {
        this.employees.update(current =>
          current.map(e => e.id === fullDetails.id ? fullDetails : e)
        );
      });
    });
  });
  }

  openDetailTask(taskSummary: TaskSummary) {
    this.taskType.set('Detail');
    this.http.getTaskById(taskSummary.id).subscribe(fullTask => {
      this.selectedTask.set(fullTask);
      this.taskVisible.set(true);
    });
  }

  generatePlan() {
    if (this.schedulerForm.invalid){
      this.schedulerForm.markAllAsTouched();
      return;
    }


    const val = this.schedulerForm.value;
    const request: PlanRequest = {
      maxTime: val.maxTime!,
      boatNumber: val.boatAmount!,
      taskItemIds: val.selectedTasks?.map(t => t.id) || [],
      personIds: val.selectedEmployees?.map(e => e.id) || [],
      toolIds: val.selectedTools?.map(t => t.id) || []
    };

    this.http.postPlan(request).subscribe(plan => {
      console.log(plan);
      this.router.navigate(['/schedule-view']);
    });
  }

  toolEdit(selectedTool: Tool, type: 'Edit' | 'New'): void {
    if(type === 'Edit') {
      this.toolVisible.set(true);
      this.toolType.set(type);
      this.dialogComp.patchForm(selectedTool);
    } else {
      this.toolVisible.set(true);
      this.toolType.set(type);
      this.dialogComp.patchForm(null);
    }
  }

  validateStep(step: number): boolean {
    switch (step) {
      case 1:
        const maxTime = this.schedulerForm.controls.maxTime;
        const boatAmount = this.schedulerForm.controls.boatAmount;

        maxTime.markAsTouched();
        boatAmount.markAsTouched();

        return maxTime.valid && boatAmount.valid;

      case 2:
        const tasks = this.schedulerForm.controls.selectedTasks;
        tasks.markAsTouched();
        return tasks.valid && tasks.value.length > 0;;

      case 3:
        const employees = this.schedulerForm.controls.selectedEmployees;
        employees.markAsTouched();
        return employees.valid && employees.value.length > 0;;

      case 4:
        const tools = this.schedulerForm.controls.selectedTools;
        tools.markAsTouched();
        tools.value.length > 0;
        return tools.valid && tools.value.length > 0;;

      default:
        return true;
    }
  }

  validateAndNext(currentStep: number, nextStep: number, activateCallback: Function) {
    if (this.validateStep(currentStep)) {
      activateCallback(nextStep);
    }
  }

  formatDuration(decimalHours: number): string {
    if (!decimalHours) return '0 h';

    const hours = Math.floor(decimalHours);
    const minutes = Math.round((decimalHours - hours) * 60);

    if (hours > 0 && minutes > 0) {
      return `${hours} h ${minutes} min`;
    } else if (hours > 0) {
      return `${hours} h`;
    } else {
      return `${minutes} min`;
    }
  }
}
