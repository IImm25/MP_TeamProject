import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogEmployee } from './dialog-employee';

describe('DialogEmployee', () => {
  let component: DialogEmployee;
  let fixture: ComponentFixture<DialogEmployee>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogEmployee],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogEmployee);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
