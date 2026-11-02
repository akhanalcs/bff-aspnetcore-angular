// The shape of one row from GET /api/repair-orders (RepairOrderListItem in the API).
// Hand-written for now; replaced by types generated from the API's OpenAPI document (roadmap 4).
export type RepairOrderStatus = 'Open' | 'InProgress' | 'Closed' | 'Cancelled';

export interface RepairOrderListItem {
  id: number;
  unitNumber: string;
  complaint: string;
  status: RepairOrderStatus;
  createdAt: string; // ISO 8601 with offset, e.g. "2026-11-01T09:31:22+00:00"
}
