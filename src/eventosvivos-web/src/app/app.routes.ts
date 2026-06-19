import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'events', pathMatch: 'full' },
  {
    path: 'events',
    loadComponent: () => import('./events/event-list/event-list').then(m => m.EventListComponent)
  },
  {
    path: 'events/create',
    loadComponent: () => import('./events/event-create/event-create').then(m => m.EventCreateComponent)
  },
  {
    path: 'reservations',
    loadComponent: () => import('./reservations/reservation-list/reservation-list').then(m => m.ReservationListComponent)
  },
  {
    path: 'reservations/create/:eventId',
    loadComponent: () => import('./reservations/reservation-create/reservation-create').then(m => m.ReservationCreateComponent)
  },
  {
    path: 'reservations/cancel',
    loadComponent: () => import('./reservations/buyer-cancel/buyer-cancel').then(m => m.BuyerCancelComponent)
  },
  {
    path: 'reports/:eventId',
    loadComponent: () => import('./reports/occupancy-report/occupancy-report').then(m => m.OccupancyReportComponent)
  }
];
