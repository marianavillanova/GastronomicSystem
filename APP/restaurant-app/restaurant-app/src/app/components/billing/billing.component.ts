import { Component, OnInit } from '@angular/core';
import { BillingService } from '../../services/billing.service';
import { Bill } from '../../models/bill.model';

@Component({
  selector: 'app-billing',
  templateUrl: './billing.component.html',
})
export class BillingComponent implements OnInit {
  bills: Bill[] = [];

  constructor(private billingService: BillingService) {}

  ngOnInit(): void {
    this.loadBills();
  }

  loadBills(): void {
    this.billingService.getBills().subscribe((data) => {
      this.bills = data;
    });
  }
}