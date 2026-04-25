import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogQualification } from './dialog-qualification';

describe('DialogQualification', () => {
  let component: DialogQualification;
  let fixture: ComponentFixture<DialogQualification>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogQualification],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogQualification);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
