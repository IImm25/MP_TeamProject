import { Component, inject, OnInit, signal, WritableSignal } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogTask } from '../dialog-task/dialog-task';
import { Task, TaskSummary } from '../../../Models/task';
import { environment } from '../../../../environments/environment';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { HttpService } from '../../../Services/http-service';

@Component({
  selector: 'app-tasks',
  imports: [TranslatePipe, ButtonModule, TableModule, DialogTask, ConfirmDialogModule],
  providers: [ConfirmationService],
  templateUrl: './tasks.html',
  styleUrl: './tasks.css',
})
export class Tasks implements OnInit {
  private http = inject(HttpService);
  private translate = inject(TranslateService);
  private confirmationService = inject(ConfirmationService);

  type: WritableSignal<'Edit' | 'New' | 'Detail'> = signal('New');
  selectedTask: WritableSignal<Task | null> = signal(null);
  visible: WritableSignal<boolean> = signal(false);
  tasks: WritableSignal<TaskSummary[]> = signal([]);

  apiUrl = environment.apiUrl;

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.http.getTasks().subscribe((tasks) => this.tasks.set(tasks));
  }

  openEditOrDetail(taskSummary: TaskSummary, mode: 'Edit' | 'Detail') {
    this.type.set(mode);
    this.http.getTaskById(taskSummary.id).subscribe(fullTask => {
      this.selectedTask.set(fullTask);
      this.visible.set(true);
    });
  }

  deleteTask(task: Task) {
    this.confirmationService.confirm({
      message: this.translate.instant('TASKS.DIALOG.DELETE_MESSAGE'),
      header: this.translate.instant('TASKS.DIALOG.DELETE_TITLE'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('COMMON.DELETE'),
      rejectLabel: this.translate.instant('COMMON.CANCEL'),
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.http.deleteTask(task.id).subscribe(() => {
          this.tasks.update((tasks) => tasks.filter((t) => t.id !== task.id));
        });
      },
    });
  }

  formatDuration(decimalHours: number): string {
    if (!decimalHours) return '0 h';

    const hours = Math.floor(decimalHours);
    const minutes = Math.round((decimalHours - hours) * 60);

    if (hours > 0 && minutes > 0) {
      return `${hours} h ${minutes} min`;
    } else if (hours > 0) {
      return `${hours} h`;
    } else {
      return `${minutes} min`;
    }
  }
}
