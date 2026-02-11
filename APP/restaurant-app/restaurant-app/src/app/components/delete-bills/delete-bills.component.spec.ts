import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteBillsComponent } from './delete-bills.component';

describe('DeleteBillsComponent', () => {
  let component: DeleteBillsComponent;
  let fixture: ComponentFixture<DeleteBillsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteBillsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeleteBillsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
