import { Component, EventEmitter, inject, Input, model, Output, signal, SimpleChanges, WritableSignal } from '@angular/core';
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
  @Input() selectedTool: Tool | null = null;
  @Input() requiredAmount: number = 0;
  visible = model<boolean>(false);

  @Output() onSave = new EventEmitter<void>();

  currentTool: Tool | null = null;

  toolForm = this.fb.group({
    name: ['', Validators.required],
    availableStock: [1, Validators.required],
    requiredAmount: [{ value: 0, disabled: true }],
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['selectedTool'] || changes['type'] || changes['requiredAmount']) {
      const tool = this.selectedTool;
      this.currentTool = tool;

      if (tool) {
        this.toolForm.patchValue({
          name: tool.name,
          availableStock: tool.availableStock,
          requiredAmount: this.requiredAmount,
        });

        if (this.type === 'Detail') {
          this.toolForm.disable();
          this.toolForm.controls.requiredAmount.disable();
        } else {
          this.toolForm.enable();
          this.toolForm.controls.requiredAmount.disable();
        }
      } else {
        this.toolForm.reset({ name: '', availableStock: 0, requiredAmount: 0 });
        if (this.type !== 'Detail') {
          this.toolForm.enable();
          this.toolForm.controls.requiredAmount.disable();
        }
      }
    }
  }

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
