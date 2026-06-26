import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WeatherDialog } from './weather-dialog';

describe('WeatherDialog', () => {
  let component: WeatherDialog;
  let fixture: ComponentFixture<WeatherDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WeatherDialog],
    }).compileComponents();

    fixture = TestBed.createComponent(WeatherDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
