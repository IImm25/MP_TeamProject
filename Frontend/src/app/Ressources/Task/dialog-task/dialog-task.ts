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
import { Task, TaskQualification, TaskTool } from '../../../Models/task';
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
import { environment } from '../../../../environments/environment';
import { DialogTaskQualification } from '../dialog-task-qualification/dialog-task-qualification';
import { Tool } from '../../../Models/tool';
import { HttpService } from '../../../Services/http-service';

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
  ],
  templateUrl: './dialog-task.html',
  styleUrl: './dialog-task.css',
})
export class DialogTask implements OnInit {
  private formBuilder = inject(FormBuilder);
  private http = inject(HttpService);

  @Output() taskSaved = new EventEmitter<void>();

  @Input({ required: true }) type: 'Edit' | 'New' | 'Detail' = 'New';
  @Input() set selectedTask(val: Task | null) {
    this.currentTask = val;

    if (val) {
      const dHours = Math.floor(val.durationHours);
      const dMinutes = Math.round((val.durationHours - dHours) * 60);
      this.taskForm.patchValue({
        name: val.name,
        durationHours: dHours,
        durationMinutes: dMinutes,
        qualifications: val.requiredQualifications,
        tools: val.requiredTools,
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

  taskForm = this.formBuilder.group({
    name: ['', Validators.required],
    durationHours: [0],
    durationMinutes: [0, [Validators.max(59)]],
    qualifications: [[] as TaskQualification[], Validators.required],
    tools: [[] as TaskTool[], Validators.required],
  });

  allTools: WritableSignal<Tool[]> = signal([]);

  ngOnInit(): void {
    this.http.getQualifications().subscribe((qualifications) => {
      this.allQualifications.set(qualifications);
    });
    this.http.getTools().subscribe((tools) => {
      this.allTools.set(tools);
    });
  }

  validateStep(step: number): boolean {
    if (this.type === 'Detail') return true;
    switch (step) {
      case 1:
        const nameControl = this.taskForm.controls.name;
        const hControl = this.taskForm.controls.durationHours;
        const mControl = this.taskForm.controls.durationMinutes;

        if (nameControl?.invalid) nameControl.markAsTouched();
        if (hControl?.invalid) hControl.markAsTouched();
        if (mControl?.invalid) mControl.markAsTouched();

        const h = hControl?.value || 0;
        const m = mControl?.value || 0;
        const isDurationValid = h > 0 || m > 0;

        if (!isDurationValid) {
          hControl?.setErrors({ minDuration: true });
          mControl?.setErrors({ minDuration: true });
          hControl.markAsTouched();
          mControl.markAsTouched();
          hControl.updateValueAndValidity();
          mControl.updateValueAndValidity();
        } else {
          if (hControl?.hasError('minDuration')) {
            const { minDuration, ...rest } = hControl.errors!;
            hControl.setErrors(Object.keys(rest).length ? rest : null);
          }
          if (mControl?.hasError('minDuration')) {
            const { minDuration, ...rest } = mControl.errors!;
            mControl.setErrors(Object.keys(rest).length ? rest : null);
          }
        }
        return (
          nameControl?.valid === true &&
          hControl?.valid === true &&
          mControl?.valid === true &&
          isDurationValid
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

    const payload: Partial<Task> = {
      name: val.name || '',
      durationHours: totalDuration,
      requiredQualifications: val.qualifications || [],
      requiredTools: val.tools || [],
    };

    if (this.type === 'Edit' && this.currentTask) {
      this.http.updateTask(this.currentTask.id, payload).subscribe({
        next: () => {
          this.taskSaved.emit();
          this.visible.set(false);
        },
      });
    } else if (this.type === 'New') {
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
}
