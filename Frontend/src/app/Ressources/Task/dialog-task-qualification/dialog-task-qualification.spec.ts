import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogTaskQualification } from './dialog-task-qualification';

describe('DialogTaskQualification', () => {
  let component: DialogTaskQualification;
  let fixture: ComponentFixture<DialogTaskQualification>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogTaskQualification],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogTaskQualification);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
