import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogTurbine } from './dialog-turbine';

describe('DialogTurbine', () => {
  let component: DialogTurbine;
  let fixture: ComponentFixture<DialogTurbine>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogTurbine],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogTurbine);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
