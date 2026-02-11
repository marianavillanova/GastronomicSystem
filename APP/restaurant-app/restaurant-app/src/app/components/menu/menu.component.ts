import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleService } from '../../services/article.service';
import { Article } from '../../models/article.model';
import { OrderService } from '../../services/order.service';
import { Order, OrderItem } from '../../models/order.model';
import { SessionService } from '../../services/session.service';
import { TableService } from '../../services/table.service';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { BillingService } from '../../services/billing.service';
import { CreateBillDto, PaymentDetails } from '../../models/bill.model';
import { Customer, CustomerService } from '../../services/customer.service';
import { AlertService } from '../../shared/alert/alert.service';


@Component({
  selector: 'app-menu',
  standalone: true,
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss'],
  imports: [CommonModule, FormsModule]
})


export class MenuComponent implements OnInit {
  tableId: number | null = null;
  articles: Article[] = [];
  categories: string[] = [];
  selectedCategory: string | null = null;
  subcategories: string[] = [];
  selectedSubCategory: string | null = null;
  filteredArticles: Article[] = [];
  paxAmount: number | null = null;
  currentOrder: Order | null = null;
  currentOrderItems: OrderItem[] = [];
  selectedArticles: {
    articleId: number;
    name: string;
    price: number;
    quantity: number;
    comment?: string;
    discount?: number;
  }[] = [];

  employeeName: string = '';
  isLoading: boolean = true;

  showCommentModal = false;
  showDiscountModal = false;
  showPaymentModal = false;
  corporateStep: 'info' | 'payment' = 'info';

  activeItem: any = null;
  tempDiscount: number | null = null;

  globalDiscountPercentage: number | null = null;
  showGlobalDiscountModal = false;

  customerType: 'final' | 'corporate' | null = null;
  paymentMethod: 'cash' | 'card' | 'split' | null = null;

  splitCashAmount: number = 0;
  splitCardAmount: number = 0;

  corporateInfo = {
    companyName: '',
    vatNumber: '',
    address: '',
    contact: ''
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private articleService: ArticleService,
    private orderService: OrderService,
    private sessionService: SessionService,
    private tableService: TableService,
    private billingService: BillingService,
    private customerService: CustomerService,
    private alertService: AlertService
  ) {}


//Starting Point
  ngOnInit(): void {
  this.route.paramMap.subscribe(params => {
    this.tableId = Number(params.get('tableId'));
      this.route.queryParamMap.subscribe(params => {
      const paxParam = params.get('paxAmount');
      this.paxAmount = paxParam ? Number(paxParam) : null;
      console.log('👥 Guest count from route:', this.paxAmount);
    });

    console.log('🔄 Loaded menu for table:', this.tableId);
    this.employeeName = this.sessionService.getEmployeeName();

    if (this.tableId) {
      this.tableService.getTableById(this.tableId).subscribe(table => {
        this.paxAmount = table.pax;
        console.log('👥 Guest count:', this.paxAmount);

        this.orderService.getActiveOrderForTable(this.tableId!).subscribe((order: Order) => {
          this.currentOrder = order ?? null;
          this.globalDiscountPercentage = order?.globalDiscount ?? null;

          const rawItems = order?.orderItems;
          this.currentOrderItems = Array.isArray(rawItems)
            ? rawItems
            : (rawItems && '$values' in rawItems)
              ? rawItems.$values
              : [];

          console.log('📦 Current order items:', this.currentOrderItems);
          console.log('📦 Loaded active order:', JSON.stringify(order, null, 2));
        });
      });
    }

    this.articleService.getArticles().subscribe((data: any) => {
      const articlesArray = Array.isArray(data) ? data : data.$values;
      if (Array.isArray(articlesArray)) {
        this.articles = articlesArray.filter((a: Article) => a.isActive);
        this.categories = [...new Set(this.articles.map(a => a.category))];
      } else {
        this.articles = [];
      }
      this.isLoading = false;
    });
  });
}


// Menu logic: categories & subcategories
  selectCategory(category: string): void {
    this.selectedCategory = category;
    this.selectedSubCategory = null;
    this.subcategories = [...new Set(
      this.articles.filter(a => a.category === category).map(a => a.subCategory)
    )];
  }

  selectSubCategory(subCat: string): void {
    this.selectedSubCategory = subCat;
    this.filteredArticles = this.articles.filter(
      a => a.category === this.selectedCategory && a.subCategory === subCat
    );
  }

  // Order item management
  addToOrder(article: Article): void {
    const existing = this.selectedArticles.find(a => a.articleId === article.articleId);
    if (existing) {
      existing.quantity += 1;
    } else {
      this.selectedArticles.push({
        articleId: article.articleId,
        name: article.name,
        price: article.price,
        quantity: 1
      });
    }
  }

  trackByArticleId(index: number, item: { articleId: number }): number {
  return item.articleId;
  }

  //Delete from selected items
  removeFromOrder(item: OrderItem): void {
  this.selectedArticles = this.selectedArticles.filter(a => a.articleId !== item.articleId);
}

  //Delete from Order (submitted items)
  canDeleteSubmittedItem(): boolean {
  const role = this.sessionService.getEmployeeRole()!;
  return ['manager', 'supervisor', 'head waiter'].includes(role.toLowerCase());
}



  deleteSubmittedItem(item: OrderItem): void {
    if (!item?.orderItemId) return;

    this.orderService.deleteOrderItem(item.orderItemId).subscribe(() => {
      console.log(`🗑️ Deleted item ${item.orderItemId}`);
      this.currentOrderItems = this.currentOrderItems.filter(i => i.orderItemId !== item.orderItemId);

      // Safeguard: if no items left, close the table
      if (this.currentOrderItems.length === 0 && this.currentOrder?.orderId) {
        this.orderService.closeTableByOrder(this.currentOrder.orderId).subscribe(() => {
          console.log(`✅ Closed table because order ${this.currentOrder?.orderId} is now empty`);
          this.router.navigate(['/tables']); // optional navigation
        });
      }
    });
  }


  // Navigation
  goBack(): void {
    this.router.navigate(['/tables']);
  }

  //Modals for comments and discounts
  openCommentModal(item: any): void {
    this.activeItem = item;
    this.showCommentModal = true;
  }

  openDiscountModal(item: any): void {
    this.activeItem = item;
    this.tempDiscount = item.discount ?? null;
    this.showDiscountModal = true;
  }

  closeModals(): void {
    this.showCommentModal = false;
    this.showDiscountModal = false;
    this.showGlobalDiscountModal = false;
    this.activeItem = null;
    this.tempDiscount = null;
  }

  submitComment(): void {
    if (this.activeItem) {
      const index = this.selectedArticles.findIndex(a => a.articleId === this.activeItem.articleId);
      if (index !== -1) {
        this.selectedArticles[index].comment = this.activeItem.comment;
      }
    }
    this.closeModals();
  }

  submitDiscount(): void {
    if (this.activeItem) {
      const index = this.selectedArticles.findIndex(a => a.articleId === this.activeItem.articleId);
      if (index !== -1) {
        this.selectedArticles[index].discount = this.tempDiscount ?? 0;
      }
    }
    this.closeModals();
  }

 //Keyboard trigger for touchscreen devices
  triggerKeyboard(): void {
    setTimeout(() => {
      const input = document.querySelector('.modal-input') as HTMLInputElement;
      if (input) input.focus();
    }, 200);
  }

  //Order logic
  submitOrder(): void {
  if (!this.tableId || this.selectedArticles.length === 0) return;

  const employeeId = this.sessionService.getEmployeeId() ?? 0;
  const paxAmount = this.paxAmount ?? 1;

  const sendItems = (orderId: number) => {
    const requests = this.selectedArticles.map(item => {
      return this.orderService.addOrderItem({
        tableId: this.tableId!,
        articleId: item.articleId,
        quantity: item.quantity,
        price: item.price,
        comment: item.comment || '',
        discount: item.discount || 0
      });
    });

    forkJoin(requests).subscribe(() => {
      // ⏱️ Add a short delay before submitting the order
      setTimeout(() => {
        this.orderService.submitOrder(orderId).subscribe(() => {
          console.log('📤 Order submitted successfully');

          this.orderService.getActiveOrderForTable(this.tableId!).subscribe((order: Order) => {
          this.currentOrder = order;

          if (order?.orderId && typeof this.globalDiscountPercentage === 'number') {
            this.orderService.setGlobalDiscount(order.orderId, this.globalDiscountPercentage).subscribe(() => {
              console.log('🌍 Updated discount on reused order');
            });
          }

          const rawItems = order?.orderItems;
          this.currentOrderItems = Array.isArray(rawItems)
            ? rawItems
            : (rawItems && '$values' in rawItems)
              ? rawItems.$values
              : [];
        });


          this.selectedArticles = [];
          this.goBack();
        });
      }, 300); // 300ms delay to ensure items are committed
    });
  };

  // ✅ Check for existing active order
  this.orderService.getActiveOrderForTable(this.tableId!).subscribe(existingOrder => {
    if (existingOrder?.orderId) {
      console.log('🔁 Reusing existing order:', existingOrder.orderId);
      this.currentOrder = existingOrder;
      sendItems(existingOrder.orderId);
    } else {
      console.log('🆕 No active order — creating new order first');

      // ✅ Create order first
      this.orderService.createOrder({
        tableId: this.tableId!,
        employeeId,
        paxAmount,
        globalDiscount: this.globalDiscountPercentage ?? null
      }).subscribe(newOrder => {
        this.currentOrder = newOrder;

        // ✅ Then open the table
        this.tableService.openTable(this.tableId!, {
          employeeId,
          paxAmount
        }).subscribe(() => {
          sendItems(newOrder.orderId);
        });
      });
    }
  });
}

  //Global discount modal
  openGlobalDiscountModal(): void {
  this.showGlobalDiscountModal = true;
  }

  submitGlobalDiscount(): void {
    this.showGlobalDiscountModal = false;

    if (this.currentOrder?.orderId && typeof this.globalDiscountPercentage === 'number') {
      this.orderService.setGlobalDiscount(this.currentOrder.orderId, this.globalDiscountPercentage).subscribe(() => {
        console.log(`🌍 Global discount ${this.globalDiscountPercentage}% saved for order ${this.currentOrder?.orderId}`);
      });
    }
  }



  // Payment modal
  openPaymentModal(): void {
    const hasSubmittedItems = this.currentOrderItems.length > 0;
  const hasPendingSelectedItems = this.selectedArticles.length > 0;

  if (!hasSubmittedItems) {
    this.alertService.showAlert('This order has no submitted items. Please submit items before charging the customer.');
    return;
  }

  if (hasPendingSelectedItems) {
    this.alertService.showAlert('There are items selected but not submitted. Please submit them before charging.');
    return;
  }
    
    console.log('🔔 Payment modal triggered');
    this.splitCardAmount = this.totalAmount;
    this.splitCashAmount = 0;
    this.showPaymentModal = true;
  }

  closePaymentModal(): void {
    this.showPaymentModal = false;
    this.customerType = null;
    this.paymentMethod = null;
    this.splitCashAmount = 0;
    this.splitCardAmount = 0;
    this.corporateInfo = {
      companyName: '',
      vatNumber: '',
      address: '',
      contact: ''
    };
  }

  selectCustomerType(type: 'final' | 'corporate') {
  this.customerType = type;
  this.paymentMethod = null;
  this.corporateStep = type === 'corporate' ? 'info' : 'payment';
}

  selectPaymentMethod(method: 'cash' | 'card' | 'split') {
    this.paymentMethod = method;
  }

  //Corporate customer logic
  continueCorporate(): void {
  console.log('🚀 Continue button clicked');

  const customerPayload: Customer = {
    name: this.corporateInfo.companyName,
    customerType: 'corporate',
    contact: this.corporateInfo.contact,
    vatNumber: this.corporateInfo.vatNumber,
    address: this.corporateInfo.address
  };

  this.customerService.addCustomer(customerPayload).subscribe(customer => {
    const customerId = customer?.customerId;

    if (customerId !== undefined && this.currentOrder?.orderId) {
      this.orderService.assignCustomerToOrder(this.currentOrder.orderId, customerId).subscribe(() => {
        console.log('🔗 Customer linked to order');
      });
    } else {
      console.warn('⚠️ Cannot link customer — missing customer ID or order');
    }
  });

  this.corporateStep = 'payment';
}

  //Payment proccessing
  processPayment(method: 'cash' | 'card') {
  const payload: any = {
    method,
    customerType: this.customerType
  };

  if (this.customerType === 'corporate') {
    payload.corporateInfo = this.corporateInfo;
  }

  this.finalizeOrder(payload);
}

  processSplitPayment() {
  const payload: any = {
    method: 'split',
    customerType: this.customerType,
    splitCash: this.splitCashAmount,
    splitCard: this.splitCardAmount
  };

  if (this.customerType === 'corporate') {
    payload.corporateInfo = this.corporateInfo;
  }

  this.finalizeOrder(payload);
}

onCashAmountChange(value: number): void {
  const total = this.getTotal();
  const clampedCash = Math.min(Math.max(value, 0.01), total);

  this.splitCashAmount = clampedCash;
  this.splitCardAmount = total - clampedCash;
}



//Bills
finalizeOrder(details: PaymentDetails): void {
  if (!this.currentOrder?.orderId) {
    console.warn('⚠️ Cannot finalize — no active order');
    return;
  }

  const hasSubmittedItems = this.currentOrderItems.length > 0;
  const hasPendingSelectedItems = this.selectedArticles.length > 0;

  if (!hasSubmittedItems) {
    this.alertService.showAlert('This order has no submitted items. Please submit items before processing payment.');
    return;
  }

  if (hasPendingSelectedItems) {
    this.alertService.showAlert('There are items selected but not submitted. Please submit them before processing payment.');
    return;
  }

  console.log('🧾 Finalizing order with:', details.method);

  this.createBill(
    this.currentOrder.orderId,
    details.method,
    details.splitCash,
    details.splitCard
  );
}


createBill(
  orderId: number,
  method: 'cash' | 'card' | 'split',
  splitCash?: number,
  splitCard?: number
): void {
  const subtotal = this.getSubtotal();
  const total = this.getTotal();
  const discountAmount = subtotal - total;

  const payload: CreateBillDto = {
    orderId,
    paymentMethod: method,
    subtotal,
    discount: discountAmount
  };

  if (method === 'split') {
    payload.splitCashAmount = splitCash ?? 0;
    payload.splitCardAmount = splitCard ?? 0;
  }

  this.billingService.createBill(payload).subscribe(() => {
    console.log('✅ Bill created successfully');
    this.orderService.closeTableByOrder(orderId).subscribe(() => {
      this.resetUI();
      this.router.navigate(['/tables']);
    });
  });
}


//UI cleaner
 resetUI(): void {
  this.showPaymentModal = false;
  this.customerType = null;
  this.paymentMethod = null;
  this.splitCashAmount = 0;
  this.splitCardAmount = 0;
  this.corporateInfo = {
    companyName: '',
    vatNumber: '',
    address: '',
    contact: ''
  };
  this.selectedArticles = [];
  this.currentOrderItems = [];
}

getSubtotal(): number {
  const allItems = [...this.currentOrderItems, ...this.selectedArticles];
  return allItems.reduce((sum, item) => sum + item.price * item.quantity, 0);
}

//Total logic
getTotal(): number {
  const allItems = [...this.currentOrderItems, ...this.selectedArticles];

  const itemDiscountedTotal = allItems.reduce((sum, item) => {
    const discount = item.discount ?? 0;
    const discountedPrice = item.price * (1 - discount / 100);
    return sum + discountedPrice * item.quantity;
  }, 0);

  const globalDiscount = this.globalDiscountPercentage ?? 0;
  const finalTotal = itemDiscountedTotal * (1 - globalDiscount / 100);

  return parseFloat(finalTotal.toFixed(2));
}


  get totalAmount(): number {
    return this.getTotal();
  }


}
