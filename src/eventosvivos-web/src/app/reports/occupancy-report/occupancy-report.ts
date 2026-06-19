import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EventsService } from '../../shared/events.service';
import { OccupancyReport } from '../../shared/models';

@Component({
  selector: 'app-occupancy-report',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="page">
      <div class="page-header">
        <div class="page-title">
          <p class="eyebrow">Reporte</p>
          <h1>Ocupación</h1>
        </div>
        <a routerLink="/events" class="button button-secondary">Volver</a>
      </div>

      <div *ngIf="report() as r" class="report-card form-panel">
        <div class="page-header">
          <h2>{{ r.eventTitle }}</h2>
          <span [class]="'badge ' + r.eventState.toLowerCase()">{{ r.eventState }}</span>
        </div>

        <div class="metric-grid">
          <div class="metric">
            <span>Confirmadas</span>
            <strong>{{ r.confirmedTicketsSold }}</strong>
          </div>
          <div class="metric">
            <span>Pendientes</span>
            <strong>{{ r.pendingTicketsHeld }}</strong>
          </div>
          <div class="metric">
            <span>Restantes</span>
            <strong>{{ r.remainingTickets }}</strong>
          </div>
          <div class="metric">
            <span>Perdidas</span>
            <strong>{{ r.lostTickets }}</strong>
          </div>
          <div class="metric">
            <span>Ocupación</span>
            <strong>{{ r.occupancyPercentage | number:'1.1-2' }}%</strong>
          </div>
          <div class="metric">
            <span>Ingresos</span>
            <strong>$ {{ r.totalIncome | number:'1.2-2' }}</strong>
          </div>
        </div>

        <div class="meter" aria-label="Porcentaje de ocupación">
          <span [style.width.%]="r.occupancyPercentage"></span>
        </div>
      </div>

      <p *ngIf="error()" class="error">{{ error() }}</p>
    </section>
  `
})
export class OccupancyReportComponent implements OnInit {
  private readonly svc = inject(EventsService);
  private readonly route = inject(ActivatedRoute);

  report = signal<OccupancyReport | undefined>(undefined);
  error = signal('');

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('eventId'));
    this.svc.getOccupancyReport(id).subscribe({
      next: (r) => this.report.set(r),
      error: (e) => this.error.set(e.error?.detail ?? 'Error al cargar el reporte.')
    });
  }
}
