import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogTaskTool } from './dialog-task-tool';

describe('DialogTaskTool', () => {
  let component: DialogTaskTool;
  let fixture: ComponentFixture<DialogTaskTool>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogTaskTool],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogTaskTool);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
