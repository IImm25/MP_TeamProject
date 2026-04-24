import { Component, inject, Input, model, signal, WritableSignal } from '@angular/core';
import { TaskQualification } from '../../../Models/task-qualification';
import { Qualification } from '../../../Models/qualification';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { environment } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { DialogModule } from 'primeng/dialog';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { disabled } from '@angular/forms/signals';

@Component({
  selector: 'app-dialog-task-qualification',
  imports: [
    DialogModule,
    SelectModule,
    InputNumberModule,
    ButtonModule,
    TranslatePipe,
    ReactiveFormsModule,
  ],
  templateUrl: './dialog-task-qualification.html',
  styleUrl: './dialog-task-qualification.css',
})
export class DialogTaskQualification {
  private http = inject(HttpClient);
  private formBuilder = inject(FormBuilder);
  apiUrl = environment.apiUrl;

  @Input({ required: true }) type: 'Edit' | 'New' = 'New';
  selectedQualification = model<TaskQualification | null>(null);
  visible = model<boolean>(false);
  allQualifications: WritableSignal<Qualification[]> = signal([]);

  qualificationForm = this.formBuilder.group({
    selectedQualificationForm: new FormControl<Qualification | null>(null, Validators.required),
    selectedQualificationCount: new FormControl<number>(1, [
      Validators.required,
      Validators.min(1),
    ]),
  });

  ngOnInit(): void {
    this.getQualifications();
    if (this.type === 'Edit') {
      this.qualificationForm.controls.selectedQualificationForm.disable();
    }
  }

  getQualifications() {
    this.http.get<Qualification[]>(`${this.apiUrl}/qualifications`).subscribe((Qualifications) => {
      this.allQualifications.set(Qualifications);

      const currentTaskQualification = this.selectedQualification();

      if (this.type === 'Edit' && currentTaskQualification) {
        const matchingQualification =
          Qualifications.find((t) => t.id === currentTaskQualification.id) || null;
        this.qualificationForm.patchValue({
          selectedQualificationForm: matchingQualification,
          selectedQualificationCount: currentTaskQualification.count,
        });
      }
    });
  }

  addQualification() {
    if (this.qualificationForm.valid && this.qualificationForm.value) {
      const qualificationBase = this.qualificationForm.controls.selectedQualificationForm
        .value as Qualification;
      const taskQualification: TaskQualification = {
        id: qualificationBase.id,
        name: qualificationBase.name,
        count: this.qualificationForm.controls.selectedQualificationCount.value as number,
      };
      this.selectedQualification.set(taskQualification);
      this.visible.set(false);
      this.qualificationForm.reset({
        selectedQualificationForm: null,
        selectedQualificationCount: 1,
      });
    }
  }

  close() {
    this.visible.set(false);
    this.qualificationForm.reset({
      selectedQualificationForm: null,
      selectedQualificationCount: 1,
    });
  }
}
