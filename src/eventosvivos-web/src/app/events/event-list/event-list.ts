import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, switchMap, takeUntil, catchError, of } from 'rxjs';
import { EventsService } from '../../shared/events.service';
import { EventModel } from '../../shared/models';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <section class="page">
      <div class="page-header">
        <div class="page-title">
          <p class="eyebrow">EventosVivos</p>
          <h1>Eventos</h1>
        </div>
        <a routerLink="/events/create" class="button">+ Crear evento</a>
      </div>

      <div class="filters">
        <input [(ngModel)]="titleFilter" placeholder="Buscar título..." (input)="applyFilters()" aria-label="Buscar título" />
        <select [(ngModel)]="stateFilter" (change)="applyFilters()" aria-label="Estado">
          <option value="">Todos los estados</option>
          <option value="Active">Activo</option>
          <option value="Completed">Completado</option>
          <option value="Canceled">Cancelado</option>
        </select>
        <select [(ngModel)]="typeFilter" (change)="applyFilters()" aria-label="Tipo">
          <option value="">Todos los tipos</option>
          <option value="Conferencia">Conferencia</option>
          <option value="Taller">Taller</option>
          <option value="Concierto">Concierto</option>
        </select>
      </div>

      <p *ngIf="error()" class="error">{{ error() }}</p>

      <div class="table-panel" *ngIf="events().length > 0; else empty">
        <table>
          <thead>
            <tr>
              <th>Título</th>
              <th>Tipo</th>
              <th>Venue</th>
              <th>Inicio</th>
              <th>Precio</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let ev of events()">
              <td>
                <strong>{{ ev.title }}</strong>
                <p class="subtle">{{ ev.description }}</p>
              </td>
              <td>{{ ev.type }}</td>
              <td>{{ ev.venueName }}</td>
              <td>{{ ev.startsAt | date:'short' }}</td>
              <td>$ {{ ev.ticketPrice | number:'1.2-2' }}</td>
              <td><span [class]="'badge ' + (ev.effectiveState | lowercase)">{{ ev.effectiveState }}</span></td>
              <td>
                <div class="row-actions">
                  <a class="button btn-sm" [routerLink]="['/reservations/create', ev.id]">Reservar</a>
                  <a class="button button-secondary btn-sm" [routerLink]="['/reports', ev.id]">Reporte</a>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <ng-template #empty>
        <div class="empty-state">
          <span class="empty-icon">0</span>
          <h3>No hay eventos disponibles</h3>
          <a routerLink="/events/create" class="button">+ Crear evento</a>
        </div>
      </ng-template>
    </section>
  `
})
export class EventListComponent implements OnInit, OnDestroy {
  private readonly svc = inject(EventsService);
  private readonly filter$ = new Subject<{ title?: string; state?: string; type?: string }>();
  private readonly destroy$ = new Subject<void>();

  events = signal<EventModel[]>([]);
  error = signal('');
  titleFilter = '';
  stateFilter = '';
  typeFilter = '';

  ngOnInit() {
    this.filter$.pipe(
      switchMap(f => this.svc.getEvents(f).pipe(
        catchError(() => { this.error.set('Error al cargar eventos.'); this.events.set([]); return of(null); })
      )),
      takeUntil(this.destroy$)
    ).subscribe(result => { if (result !== null) { this.events.set(result); this.error.set(''); } });
    this.applyFilters();
  }

  ngOnDestroy() { this.destroy$.next(); this.destroy$.complete(); }

  applyFilters() {
    this.filter$.next({
      title: this.titleFilter || undefined,
      state: this.stateFilter || undefined,
      type: this.typeFilter || undefined
    });
  }
}
