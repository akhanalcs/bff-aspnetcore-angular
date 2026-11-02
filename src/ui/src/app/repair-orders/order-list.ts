import { DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { RepairOrderListItem } from './repair-order';

// The order list page (/repair-orders): loads the orders from the API and shows them in a table.
@Component({
  // No template writes <app-order-list>: the router creates this page from the class.
  // The selector is still the tag of the element the router inserts right after <router-outlet>.
  selector: 'app-order-list',
  imports: [DatePipe, MatProgressBarModule, MatTableModule],
  templateUrl: './order-list.html',
  styleUrl: './order-list.css',
})
export class OrderList {
  // httpResource sends the GET when the page opens and exposes the result as signals:
  // value() (the orders, [] until they arrive), isLoading() and error().
  protected readonly orders = httpResource<RepairOrderListItem[]>(() => '/api/repair-orders', {
    defaultValue: [],
  });

  // Which columns the table shows, in order. Each name matches a matColumnDef in the template.
  protected readonly columns = ['unitNumber', 'complaint', 'status', 'createdAt'];
}
