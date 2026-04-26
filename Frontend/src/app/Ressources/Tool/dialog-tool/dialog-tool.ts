import { Component, EventEmitter, inject, Input, model, Output } from '@angular/core';
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
  imports: [DialogModule, ReactiveFormsModule, InputTextModule, ButtonModule, InputNumberModule, TranslatePipe],
  templateUrl: './dialog-tool.html',
  styleUrl: './dialog-tool.css',
})
export class DialogTool {
  private fb = inject(FormBuilder);
  private httpService = inject(HttpService);

  @Input() type: 'Edit' | 'New' = 'New';
  visible = model<boolean>(false);

  @Output() onSave = new EventEmitter<void>();

  selectedId: number | null = null;

  toolForm = this.fb.group({
    name: ['', Validators.required],
    availableStock: [1, Validators.required]
  });


  patchForm(tool: Tool | null) {
    if (tool) {
      this.selectedId = tool.id;
      this.toolForm.patchValue({
        name: tool.name,
        availableStock: tool.availableStock
      });
    } else {
      this.selectedId = null;
      this.toolForm.reset();
    }
  }

  close() {
    this.visible.set(false);
  }

  save() {
    if (this.toolForm.invalid) return;

    const payload: Partial<Tool> = {
       name: this.toolForm.value.name || '',
       availableStock: this.toolForm.value.availableStock || 0,
    };
    const request = this.type === 'Edit' && this.selectedId
      ? this.httpService.updateTool(this.selectedId, payload)
      : this.httpService.createTool(payload);

    request.subscribe(() => {
      this.onSave.emit();
      this.close();
    });
  }
}
