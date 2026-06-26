import {
  Component,
  EventEmitter,
  inject,
  Input,
  model,
  OnInit,
  Output,
  signal,
  WritableSignal,
} from '@angular/core';
import { Task, TaskCreate, TaskQualification, TaskTool, TaskUpdate } from '../../../Models/task';
import { DialogModule } from 'primeng/dialog';
import { StepperModule } from 'primeng/stepper';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { FormBuilder } from '@angular/forms';
import { Qualification } from '../../../Models/qualification';
import { MultiSelectModule } from 'primeng/multiselect';
import { TableModule } from 'primeng/table';
import { DialogTaskTool } from '../dialog-task-tool/dialog-task-tool';
import { DialogTaskQualification } from '../dialog-task-qualification/dialog-task-qualification';
import { Tool } from '../../../Models/tool';
import { HttpService } from '../../../Services/http-service';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { Turbine } from '../../../Models/turbine';

@Component({
  selector: 'app-dialog-task',
  imports: [
    DialogModule,
    StepperModule,
    ButtonModule,
    InputNumberModule,
    InputTextModule,
    TranslatePipe,
    FormsModule,
    ReactiveFormsModule,
    MultiSelectModule,
    TableModule,
    DialogTaskTool,
    DialogTaskQualification,
    SelectModule,
    DatePickerModule,
  ],
  templateUrl: './dialog-task.html',
  styleUrl: './dialog-task.css',
})
export class DialogTask implements OnInit {
  private formBuilder = inject(FormBuilder);
  private http = inject(HttpService);

  @Output() taskSaved = new EventEmitter<void>();

  @Input({ required: true }) type: 'Edit' | 'New' | 'Detail' | 'Accident' = 'New';

  @Input() set selectedTask(val: Task | null) {
    this.currentTask = val;

    if (val) {
      const dHours = Math.floor(val.durationHours);
      const dMinutes = Math.round((val.durationHours - dHours) * 60);

      const matchingTurbine =
        this.allTurbines().find((t) => t.id === val.location?.id) || val.location;

      let startDate: Date | null = null;
      let endDate: Date | null = null;

      if (val.executionIntervalStart && !val.executionIntervalStart.startsWith('0001')) {
        const pureDate = val.executionIntervalStart.split('T')[0];
        const [year, month, day] = pureDate.split('-').map(Number);
        startDate = new Date(year, month - 1, day);
      }

      if (val.executionIntervalEnd && !val.executionIntervalEnd.startsWith('0001')) {
        const pureDate = val.executionIntervalEnd.split('T')[0];
        const [year, month, day] = pureDate.split('-').map(Number);
        endDate = new Date(year, month - 1, day);
      }

      this.taskForm.patchValue({
        name: val.name,
        durationHours: dHours,
        durationMinutes: dMinutes,
        location: matchingTurbine,
        executionIntervalStart: startDate,
        executionIntervalEnd: endDate,
        qualifications: val.requiredQualifications || [],
        tools: val.requiredTools || [],
      });

      if (this.type === 'Detail') {
        this.taskForm.disable();
      } else {
        this.taskForm.enable();
      }
    } else {
      this.taskForm.reset({
        name: '',
        durationHours: 0,
        durationMinutes: 0,
        location: null,
        executionIntervalStart: null,
        executionIntervalEnd: null,
        qualifications: [],
        tools: [],
      });
      if (this.type !== 'Detail') {
        this.taskForm.enable();
      }
    }
  }

  visible = model<boolean>(false);
  currentTask: Task | null = null;

  allQualifications: WritableSignal<Qualification[]> = signal([]);
  qualificationDialogVisible: WritableSignal<boolean> = signal(false);
  QualificationType: WritableSignal<'Edit' | 'New'> = signal('New');
  selectedQualification: WritableSignal<TaskQualification | null> = signal(null);

  toolDialogVisible: WritableSignal<boolean> = signal(false);
  ToolType: WritableSignal<'Edit' | 'New'> = signal('New');
  selectedTool: WritableSignal<TaskTool | null> = signal(null);

  allTurbines: WritableSignal<Turbine[]> = signal([]);
  allTools: WritableSignal<Tool[]> = signal([]);

  dateValid: WritableSignal<boolean> = signal(true);

  taskForm = this.formBuilder.group({
    name: ['', Validators.required],
    durationHours: [0],
    durationMinutes: [0, [Validators.max(59)]],
    location: [null as Turbine | null, Validators.required],
    executionIntervalStart: [null as Date | null, Validators.required],
    executionIntervalEnd: [null as Date | null, Validators.required],
    qualifications: [[] as TaskQualification[], Validators.required],
    tools: [[] as TaskTool[], Validators.required],
  });

  ngOnInit(): void {
    console.log('Init', this.currentTask);
    this.http.getQualifications().subscribe((qualifications) => {
      this.allQualifications.set(qualifications);
    });
    this.http.getTools().subscribe((tools) => {
      this.allTools.set(tools);
    });
    this.http.getAllTurbines().subscribe((turbines) => {
      this.allTurbines.set(turbines);
      if (this.currentTask) {
        const matchingTurbine = turbines.find((t) => t.id === this.currentTask!.location?.id);
        if (matchingTurbine) {
          this.taskForm.patchValue({ location: matchingTurbine });
        }
      }
    });
  }

  validateStep(step: number): boolean {
    if (this.type === 'Detail') return true;
    switch (step) {
      case 1:
        const nameControl = this.taskForm.controls.name;
        const hControl = this.taskForm.controls.durationHours;
        const mControl = this.taskForm.controls.durationMinutes;
        const locControl = this.taskForm.controls.location;

        let startDate: Date = new Date();
        let endDate: Date = new Date();

        if (this.taskForm.controls.executionIntervalStart) {
          startDate = this.taskForm.controls.executionIntervalStart.getRawValue()!;
        }

        if (this.taskForm.controls.executionIntervalEnd) {
          endDate = this.taskForm.controls.executionIntervalEnd.getRawValue()!;
        }

        this.dateValid.set(startDate <= endDate);

        if (nameControl?.invalid) nameControl.markAsTouched();
        if (hControl?.invalid) hControl.markAsTouched();
        if (mControl?.invalid) mControl.markAsTouched();
        if (locControl?.invalid) locControl.markAsTouched();

        const h = hControl?.value || 0;
        const m = mControl?.value || 0;
        const isDurationValid = h > 0 || m > 0;

        if (!isDurationValid) {
          hControl?.setErrors({ minDuration: true });
          mControl?.setErrors({ minDuration: true });
          hControl.markAsTouched();
          mControl.markAsTouched();
        }

        return (
          nameControl?.valid === true &&
          locControl?.valid === true &&
          hControl?.valid === true &&
          mControl?.valid === true &&
          isDurationValid &&
          this.dateValid()
        );
      case 2:
        if (this.taskForm.controls.qualifications.invalid)
          this.taskForm.controls.qualifications.markAsTouched();
        if (this.taskForm.controls.qualifications.value?.length === 0) return false;
        return this.taskForm.controls.qualifications.valid === true;
      default:
        if (this.taskForm.controls.tools.invalid) this.taskForm.controls.tools.markAsTouched();
        if (this.taskForm.controls.tools.value?.length === 0) return false;
        return this.taskForm.controls.tools.valid === true;
    }
  }

  validateAndGoToNext(currentStep: number, nextStep: number, activateCallback: Function): void {
    if (this.validateStep(currentStep)) {
      activateCallback(nextStep);
    }
  }

  save() {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    const val = this.taskForm.value;
    const totalDuration = (val.durationHours || 0) + (val.durationMinutes || 0) / 60;

    const targetLocationId = val.location ? (val.location as unknown as Turbine).id : 0;

    const formattedStartDate = this.formatDateToApi(val.executionIntervalStart || new Date());
    const formattedEndDate = this.formatDateToApi(val.executionIntervalEnd || new Date());

    if (this.type === 'Edit' && this.currentTask) {
      const payload: TaskUpdate = {
        name: val.name || '',
        durationHours: totalDuration,
        locationId: targetLocationId,
        executionIntervalStart: formattedStartDate,
        executionIntervalEnd: formattedEndDate,
        requiredQualifications: val.qualifications || [],
        requiredTools: val.tools || [],
      };

      console.log('Edit', payload);

      this.http.updateTask(this.currentTask.id, payload).subscribe({
        next: () => {
          this.taskSaved.emit();
          this.visible.set(false);
        },
      });
    } else if (this.type === 'New' || this.type === 'Accident') {
      const payload: TaskCreate = {
        name: val.name || '',
        durationHours: totalDuration,
        locationId: targetLocationId,
        executionIntervalStart: formattedStartDate,
        executionIntervalEnd: formattedEndDate,
        requiredQualifications: val.qualifications || [],
        requiredTools: val.tools || [],
      };

      console.log('New', payload);

      this.http.createTask(payload).subscribe({
        next: () => {
          this.taskSaved.emit();
          this.visible.set(false);
        },
      });
    }
  }

  addTool() {
    this.toolDialogVisible.set(true);
    this.ToolType.set('New');
    this.selectedTool.set(null);
  }

  onToolAdded(newTool: TaskTool | null) {
    if (newTool) {
      const currentTools = [...(this.taskForm.controls.tools.value ?? [])];

      if (this.ToolType() === 'Edit') {
        const originalTool = this.selectedTool();
        const index = currentTools.findIndex((t) => t.toolId === originalTool?.toolId);

        if (index !== -1) {
          currentTools[index] = newTool;
        }
      } else {
        const existingIndex = currentTools.findIndex((t) => t.toolId === newTool.toolId);
        if (existingIndex !== -1) {
          currentTools[existingIndex].requiredAmount += newTool.requiredAmount;
        } else {
          currentTools.push(newTool);
        }
      }

      this.taskForm.patchValue({ tools: currentTools });
    }
    this.toolDialogVisible.set(false);
  }

  removeTool(index: number) {
    const currentTools = this.taskForm.controls.tools.value as TaskTool[];
    const updatedTools = currentTools.filter((_, i) => i !== index);
    this.taskForm.patchValue({ tools: updatedTools });
  }

  getToolName(id: number): string {
    const t = this.allTools().find((item) => item.id === id);
    return t ? t.name : 'Unbekannt';
  }

  removeQualification(index: number) {
    const currentQualifications = this.taskForm.controls.qualifications
      .value as TaskQualification[];
    const updatedQualifications = currentQualifications.filter((_, i) => i !== index);
    this.taskForm.patchValue({ qualifications: updatedQualifications });
  }

  addQualification() {
    this.qualificationDialogVisible.set(true);
    this.QualificationType.set('New');
    this.selectedQualification.set(null);
  }

  onQualificationAdded(newQualification: TaskQualification | null) {
    if (newQualification) {
      const currentQualifications = [...(this.taskForm.controls.qualifications.value ?? [])];

      if (this.QualificationType() === 'Edit') {
        const originalQualification = this.selectedQualification();
        const index = currentQualifications.findIndex(
          (q) => q.qualificationId === originalQualification?.qualificationId,
        );

        if (index !== -1) {
          currentQualifications[index] = newQualification;
        }
      } else {
        const existingIndex = currentQualifications.findIndex(
          (q) => q.qualificationId === newQualification.qualificationId,
        );
        if (existingIndex !== -1) {
          currentQualifications[existingIndex].requiredAmount += newQualification.requiredAmount;
        } else {
          currentQualifications.push(newQualification);
        }
      }

      this.taskForm.patchValue({ qualifications: currentQualifications });
    }
    this.qualificationDialogVisible.set(false);
  }

  getQualificationName(id: number): string {
    const q = this.allQualifications().find((item) => item.id === id);
    return q ? q.name : 'Unbekannt';
  }

  private formatDateToApi(dateVal: any): string {
    if (!dateVal) {
      const today = new Date();
      const year = today.getFullYear();
      const month = String(today.getMonth() + 1).padStart(2, '0');
      const day = String(today.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    }

    // Falls es bereits ein Date-Objekt ist (z.B. vom p-datepicker)
    const date = dateVal instanceof Date ? dateVal : new Date(dateVal);

    // Lokale Komponenten extrahieren, um Zeitzonen-Verschiebungen zu verhindern
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
