import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SessionControlComponent } from './session-control.component';

describe('SessionControlComponent', () => {
  let component: SessionControlComponent;
  let fixture: ComponentFixture<SessionControlComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SessionControlComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SessionControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
