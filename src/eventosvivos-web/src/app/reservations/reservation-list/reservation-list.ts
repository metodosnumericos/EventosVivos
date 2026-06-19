import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ReservationsService } from '../../shared/reservations.service';
import { EventsService } from '../../shared/events.service';
import { Reservation, EventModel } from '../../shared/models';

@Component({
  selector: 'app-reservation-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page">
      <div class="page-header">
        <div class="page-title">
          <p class="eyebrow">Operación</p>
          <h1>Administración de reservas</h1>
        </div>
      </div>

      <div class="filters">
        <select [(ngModel)]="selectedEventId" (change)="loadReservations()" aria-label="Evento">
          <option [ngValue]="0">Seleccione un evento...</option>
          <option *ngFor="let ev of events()" [ngValue]="ev.id">{{ ev.title }}</option>
        </select>
        <select [(ngModel)]="stateFilter" (change)="loadReservations()" aria-label="Estado">
          <option value="">Todos los estados</option>
          <option value="PendingPayment">Pendiente de pago</option>
          <option value="Confirmed">Confirmada</option>
          <option value="Canceled">Cancelada</option>
        </select>
      </div>

      <div class="table-panel" *ngIf="reservations().length > 0; else empty">
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Comprador</th>
              <th>Email</th>
              <th>Cantidad</th>
              <th>Estado</th>
              <th>Código</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let r of reservations()">
              <td><strong>#{{ r.id }}</strong></td>
              <td>{{ r.buyerName }}</td>
              <td>{{ r.buyerEmail }}</td>
              <td>{{ r.quantity }}</td>
              <td><span [class]="'badge ' + r.state.toLowerCase()">{{ r.state }}</span></td>
              <td>{{ r.reservationCode || '-' }}</td>
              <td>
                <div class="row-actions">
                  <button *ngIf="r.state === 'PendingPayment'" (click)="confirm(r)" class="btn-sm">Confirmar pago</button>
                  <button *ngIf="r.state !== 'Canceled'" (click)="cancel(r)" class="btn-sm danger">Cancelar</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <ng-template #empty>
        <div class="empty-state">
          <span class="empty-icon">R</span>
          <h3 *ngIf="selectedEventId > 0">No hay reservas para este evento</h3>
          <h3 *ngIf="selectedEventId === 0">Seleccione un evento</h3>
        </div>
      </ng-template>

      <p *ngIf="message()" [class]="messageClass()">{{ message() }}</p>
    </section>
  `
})
export class ReservationListComponent implements OnInit {
  private readonly svc = inject(ReservationsService);
  private readonly eventsSvc = inject(EventsService);
  private readonly route = inject(ActivatedRoute);

  events = signal<EventModel[]>([]);
  reservations = signal<Reservation[]>([]);
  selectedEventId = 0;
  stateFilter = '';
  message = signal('');
  messageClass = signal('success');

  ngOnInit() {
    const preselect = Number(this.route.snapshot.queryParamMap.get('eventId'));
    this.eventsSvc.getEvents().subscribe(ev => {
      this.events.set(ev);
      if (preselect) { this.selectedEventId = preselect; this.loadReservations(); }
    });
  }

  loadReservations() {
    const eventId = Number(this.selectedEventId);
    if (!eventId) { this.reservations.set([]); return; }
    this.svc.getReservations(eventId, this.stateFilter || undefined)
      .subscribe(r => this.reservations.set(r));
  }

  confirm(r: Reservation) {
    this.svc.confirmPayment(r.id).subscribe({
      next: () => { this.message.set(`Reserva ${r.id} confirmada.`); this.messageClass.set('success'); this.loadReservations(); },
      error: (e) => { this.message.set(e.error?.detail ?? 'Error al confirmar.'); this.messageClass.set('error'); }
    });
  }

  cancel(r: Reservation) {
    this.svc.adminCancel(r.id).subscribe({
      next: () => { this.message.set(`Reserva ${r.id} cancelada.`); this.messageClass.set('success'); this.loadReservations(); },
      error: (e) => { this.message.set(e.error?.detail ?? 'Error al cancelar.'); this.messageClass.set('error'); }
    });
  }
}
