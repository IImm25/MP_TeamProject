import { Component, inject, signal, ViewChild } from '@angular/core';
import { DialogTool } from '../dialog-tool/dialog-tool';
import { HttpService } from '../../../Services/http-service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { Tool } from '../../../Models/tool';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-tools',
  imports: [DialogModule, DialogTool, TranslatePipe, ConfirmDialogModule, ButtonModule, TableModule],
  providers: [ConfirmationService],
  templateUrl: './tools.html',
  styleUrl: './tools.css',
})
export class Tools {
  @ViewChild(DialogTool) dialogComp!: DialogTool;
  http = inject(HttpService);
  private translate = inject(TranslateService);
  private confirmationService = inject(ConfirmationService);

  tools = signal<Tool[]>([]);

  dialogVisible = false;
  dialogType: 'Edit' | 'New' = 'New';

  ngOnInit() {
    this.loadTools();
  }

  loadTools() {
    this.http.getTools().subscribe((data) => this.tools.set(data));
  }

  openDialog(type: 'Edit' | 'New', tool?: Tool) {
    if (type === 'New' || !tool) {
      this.dialogType = 'New';
      this.dialogComp.patchForm(null);
    } else {
      this.dialogType = 'Edit';
      this.dialogComp.patchForm(tool);
    }
    this.dialogVisible = true;
  }

  deleteTool(tool: Tool) {
    this.confirmationService.confirm({
      message: this.translate.instant('TOOLS.DIALOG.DELETE_MESSAGE', {name: tool.name}),
      header: this.translate.instant('DIALOG.DIALOG.DELETE_TITLE'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('COMMON.DELETE'),
      rejectLabel: this.translate.instant('COMMON.CANCEL'),
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.http.deleteTool(tool.id).subscribe(() => {
          this.tools.update((toolList) =>
            toolList.filter((t) => t.id !== tool.id),
          );
        });
      },
    });
  }
}
