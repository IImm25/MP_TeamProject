import { Component, EventEmitter, inject, Input, model, Output } from '@angular/core';
import { Qualification } from '../../../Models/qualification';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpService } from '../../../Services/http-service';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TranslatePipe } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-dialog-qualification',
  imports: [ButtonModule, ReactiveFormsModule, InputTextModule, TranslatePipe, DialogModule, TextareaModule],
  templateUrl: './dialog-qualification.html',
  styleUrl: './dialog-qualification.css',
})
export class DialogQualification {
  private fb = inject(FormBuilder);
  private httpService = inject(HttpService);

  @Input() type: 'Edit' | 'New' = 'New';
  visible = model<boolean>(false);

  @Output() onSave = new EventEmitter<void>();

  selectedId: number | null = null;

  qualificationForm = this.fb.group({
    name: ['', Validators.required],
    description: ['', Validators.required]
  });


  patchForm(qualification: Qualification | null) {
    if (qualification) {
      this.selectedId = qualification.id;
      this.qualificationForm.patchValue({
        name: qualification.name,
        description: qualification.description
      });
    } else {
      this.selectedId = null;
      this.qualificationForm.reset();
    }
  }

  close() {
    this.visible.set(false);
  }

  save() {
    if (this.qualificationForm.invalid) return;

    const payload: Partial<Qualification> = {
       name: this.qualificationForm.value.name || '',
       description: this.qualificationForm.value.description || '',
    };
    const request = this.type === 'Edit' && this.selectedId
      ? this.httpService.updateQualification(this.selectedId, payload)
      : this.httpService.createQualification(payload);

    request.subscribe(() => {
      this.onSave.emit();
      this.close();
    });
  }
}
