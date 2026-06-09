import { Component, EventEmitter, inject, Input, model, Output } from '@angular/core';
import { Qualification } from '../../../Models/qualification';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpService } from '../../../Services/http-service';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TranslatePipe } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { required } from '@angular/forms/signals';

@Component({
  selector: 'app-dialog-qualification',
  imports: [ButtonModule, ReactiveFormsModule, InputTextModule, TranslatePipe, DialogModule, TextareaModule],
  templateUrl: './dialog-qualification.html',
  styleUrl: './dialog-qualification.css',
})
export class DialogQualification {
  private fb = inject(FormBuilder);
  private httpService = inject(HttpService);

  @Input({required: true}) type: 'Edit' | 'New' | 'Detail' = 'New';
  @Input() set selectedQualification(qual: Qualification | null) {
    this.currentQualification = qual;
    if (qual) {
      this.qualificationForm.patchValue({
        name: qual.name,
        description: qual.description || '',
      });
      if (this.type === 'Detail') {
        this.qualificationForm.disable();
      } else {
        this.qualificationForm.enable();
      }
    } else {
      this.qualificationForm.reset({
        name: '',
        description: '',
      });
      if (this.type !== 'Detail') {
        this.qualificationForm.enable();
      }
    }
  };
  visible = model<boolean>(false);

  @Output() onSave = new EventEmitter<void>();

  currentQualification: Qualification | null = null;

  qualificationForm = this.fb.group({
    name: ['', Validators.required],
    description: ['', Validators.required]
  });

  close() {
    this.visible.set(false);
  }

  save() {
    if (this.qualificationForm.invalid) return;

    const payload: Partial<Qualification> = {
       name: this.qualificationForm.value.name || '',
       description: this.qualificationForm.value.description || '',
    };
    const request = this.type === 'Edit' && this.currentQualification
      ? this.httpService.updateQualification(this.currentQualification.id, payload)
      : this.httpService.createQualification(payload);

    request.subscribe(() => {
      this.onSave.emit();
      this.close();
    });
  }
}
