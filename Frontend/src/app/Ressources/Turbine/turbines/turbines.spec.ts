import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Turbines } from './turbines';

describe('Turbines', () => {
  let component: Turbines;
  let fixture: ComponentFixture<Turbines>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Turbines],
    }).compileComponents();

    fixture = TestBed.createComponent(Turbines);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
