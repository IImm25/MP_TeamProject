import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogTool } from './dialog-tool';

describe('DialogTool', () => {
  let component: DialogTool;
  let fixture: ComponentFixture<DialogTool>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogTool],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogTool);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
