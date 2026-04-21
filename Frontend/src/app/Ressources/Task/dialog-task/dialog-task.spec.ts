import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogTask } from './dialog-task';

describe('DialogTask', () => {
  let component: DialogTask;
  let fixture: ComponentFixture<DialogTask>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogTask],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogTask);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
