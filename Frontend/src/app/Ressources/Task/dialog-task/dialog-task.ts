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
import { Task } from '../../../Models/task';
import { DialogModule } from 'primeng/dialog';
import { StepperModule } from 'primeng/stepper';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { FormBuilder } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Qualification } from '../../../Models/qualification';
import { TaskTool } from '../../../Models/task-tool';
import { MultiSelectModule } from 'primeng/multiselect';
import { TableModule } from 'primeng/table';
import { DialogTaskTool } from '../dialog-task-tool/dialog-task-tool';
import { environment } from '../../../../environments/environment';

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
  ],
  templateUrl: './dialog-task.html',
  styleUrl: './dialog-task.css',
})
export class DialogTask implements OnInit {
  private formBuilder = inject(FormBuilder);
  private http = inject(HttpClient);

  @Output() taskSaved = new EventEmitter<void>();

  @Input({ required: true }) type: 'Edit' | 'New' | 'Detail' = 'New';
  @Input() set selectedTask(val: Task | null) {
    this._currentTask = val;

    if (val) {
      const dHours = Math.floor(val.durationHours);
      const dMinutes = Math.round((val.durationHours - dHours) * 60);

      this.taskForm.patchValue({
        name: val.name,
        durationHours: dHours,
        durationMinutes: dMinutes,
        qualifications: val.taskQualifications,
        tools: val.tasktools,
      });
      if (this.type === 'Detail') {
        this.taskForm.disable();
      } else {
        this.taskForm.enable();
      }
    } else {
      this.taskForm.reset({ name: '', durationHours: 0, durationMinutes: 0, qualifications: [], tools: [] });
      if (this.type !== 'Detail') {
        this.taskForm.enable();
      }
    }
  }

  _currentTask: Task | null = null;

  apiUrl = environment.apiUrl;

  allQualifications: WritableSignal<Qualification[]> = signal([]);
  visible = model<boolean>(false);
  toolDialogVisible: WritableSignal<boolean> = signal(false);
  ToolType: WritableSignal<'Edit' | 'New'> = signal('New');
  selectedTool: WritableSignal<TaskTool | null> = signal(null);

  taskForm = this.formBuilder.group({
    name: ['', Validators.required],
    durationHours: [0, [Validators.required]],
    durationMinutes: [0, [Validators.required, Validators.max(59)]],
    qualifications: [[] as Qualification[], Validators.required],
    tools: [[] as TaskTool[], Validators.required],
  });

  ngOnInit(): void {
    this.getQualifications();
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
        const isDurationValid = (h > 0 || m > 0);

        if (!isDurationValid) {
          hControl?.setErrors({ minDuration: true });
          mControl?.setErrors({ minDuration: true });
        }

        return nameControl?.valid === true && hControl?.valid === true && mControl?.valid === true && isDurationValid;
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
    const totalDuration = (val.durationHours || 0) + ((val.durationMinutes || 0) / 60);
    const payload = {
      name: val.name,
      durationHours: totalDuration,
      taskQualifications: val.qualifications,
      tasktools: val.tools,
    };

    if (this.type === 'Edit' && this._currentTask) {
      this.http.patch(`${this.apiUrl}/tasks/${this._currentTask.id}`, payload).subscribe({
        next: () => {
          this.taskSaved.emit();
          this.visible.set(false);
        },
      });
    } else if (this.type === 'New') {
      this.http.post(`${this.apiUrl}/tasks`, payload).subscribe({
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
        const index = currentTools.findIndex(t => t.id === originalTool?.id);

        if (index !== -1) {
           currentTools[index] = newTool;
        }
      } else {
        const existingIndex = currentTools.findIndex(t => t.id === newTool.id);
        if (existingIndex !== -1) {
          currentTools[existingIndex].count += newTool.count;
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

  getQualifications() {
    this.http
      .get<Qualification[]>(`${this.apiUrl}/qualifications`)
      .subscribe((qualifications) => this.allQualifications.set(qualifications));
  }
}
