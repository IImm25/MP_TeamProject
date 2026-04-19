import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Qualifications } from './qualifications';

describe('Qualifications', () => {
  let component: Qualifications;
  let fixture: ComponentFixture<Qualifications>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Qualifications],
    }).compileComponents();

    fixture = TestBed.createComponent(Qualifications);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
