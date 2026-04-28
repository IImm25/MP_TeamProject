import { Component, EventEmitter, inject, Input, model, Output, signal, WritableSignal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpService } from '../../../Services/http-service';
import { Tool } from '../../../Models/tool';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-dialog-tool',
  imports: [
    DialogModule,
    ReactiveFormsModule,
    InputTextModule,
    ButtonModule,
    InputNumberModule,
    TranslatePipe,
  ],
  templateUrl: './dialog-tool.html',
  styleUrl: './dialog-tool.css',
})
export class DialogTool {
  private fb = inject(FormBuilder);
  private httpService = inject(HttpService);

  @Input({ required: true }) type: 'Edit' | 'New' | 'Detail' = 'New';
  visible = model<boolean>(false);
  @Input() set selectedTool(tool: Tool | null) {
    this.currentTool = tool;
    if (tool) {
      this.toolForm.patchValue({
        name: tool.name,
        availableStock: tool.availableStock,
      });
      if (this.type === 'Detail') {
        this.toolForm.disable();
      } else {
        this.toolForm.enable();
      }
    } else {
      this.toolForm.reset({
        name: '',
        availableStock: 0,
      });
      if (this.type !== 'Detail') {
        this.toolForm.enable();
      }
    }
  }

  @Output() onSave = new EventEmitter<void>();

  toolForm = this.fb.group({
    name: ['', Validators.required],
    availableStock: [1, Validators.required],
  });

  currentTool: Tool | null = null;

  save() {
    if (this.toolForm.invalid || this.type === 'Detail') {
      this.toolForm.markAllAsTouched();
      return;
    }

    const payload: Partial<Tool> = {
      name: this.toolForm.value.name || '',
      availableStock: this.toolForm.value.availableStock || 0,
    };

    const request = this.currentTool?.id
      ? this.httpService.updateTool(this.currentTool.id, payload)
      : this.httpService.createTool(payload);

    request.subscribe(() => {
      this.onSave.emit();
      this.visible.set(false);
    });
  }
}
