import { Component, inject, Input, model, OnInit, signal, WritableSignal } from '@angular/core';
import { TaskTool } from '../../../Models/task-tool';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { Tool } from '../../../Models/tool';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-dialog-task-tool',
  imports: [
    DialogModule,
    SelectModule,
    InputNumberModule,
    ButtonModule,
    TranslatePipe,
    ReactiveFormsModule,
  ],
  templateUrl: './dialog-task-tool.html',
  styleUrl: './dialog-task-tool.css',
})
export class DialogTaskTool implements OnInit {
  private http = inject(HttpClient);
  private formBuilder = inject(FormBuilder);
  apiUrl = environment.apiUrl;

  @Input({ required: true }) type: 'Edit' | 'New' = 'New';
  selectedTool = model<TaskTool | null>(null);
  visible = model<boolean>(false);
  allTools: WritableSignal<Tool[]> = signal([]);

  toolForm = this.formBuilder.group({
    selectedToolForm: new FormControl<Tool | null>(null, Validators.required),
    selectedToolCount: new FormControl<number>(1, [Validators.required, Validators.min(1)]),
  });

  ngOnInit(): void {
    this.getTools();
    if (this.type === 'Edit') {
      this.toolForm.controls.selectedToolForm.disable();
    }
  }

  getTools() {
    this.http.get<Tool[]>(`${this.apiUrl}/tools`).subscribe((tools) => {
      this.allTools.set(tools);

      const currentTaskTool = this.selectedTool();

      if (this.type === 'Edit' && currentTaskTool) {
        const matchingTool = tools.find((t) => t.id === currentTaskTool.id) || null;
        this.toolForm.patchValue({
          selectedToolForm: matchingTool,
          selectedToolCount: currentTaskTool.count,
        });
      }
    });
  }

  addTool() {
    if (this.toolForm.valid && this.toolForm.value) {
      const toolBase = this.toolForm.controls.selectedToolForm.value as Tool;
      const taskTool: TaskTool = {
        id: toolBase.id,
        name: toolBase.name,
        count: this.toolForm.controls.selectedToolCount.value as number,
      };
      this.selectedTool.set(taskTool);
      this.visible.set(false);
      this.toolForm.reset({ selectedToolForm: null, selectedToolCount: 1 });
    }
  }

  close() {
    this.visible.set(false);
    this.toolForm.reset({ selectedToolForm: null, selectedToolCount: 1 });
  }
}
