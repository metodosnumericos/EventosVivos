import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { EventsService } from '../../shared/events.service';
import { VenuesService } from '../../shared/venues.service';
import { Venue } from '../../shared/models';

@Component({
  selector: 'app-event-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page">
      <div class="page-header">
        <div class="page-title">
          <p class="eyebrow">Administración</p>
          <h1>Crear evento</h1>
        </div>
        <a routerLink="/events" class="button button-secondary">Volver</a>
      </div>

      <div class="form-panel">
        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="form-grid">
            <label class="field-full">
              Título
              <input formControlName="title" />
              <span class="validation" *ngIf="form.get('title')?.errors?.['minlength']">Mínimo 5 caracteres</span>
            </label>

            <label class="field-full">
              Descripción
              <textarea formControlName="description"></textarea>
              <span class="validation" *ngIf="form.get('description')?.errors?.['minlength']">Mínimo 10 caracteres</span>
            </label>

            <label>
              Venue
              <select formControlName="venueId">
                <option [ngValue]="null" disabled>Seleccione un venue</option>
                <option *ngFor="let v of venues()" [ngValue]="v.id">{{ v.name }} (cap. {{ v.capacity }})</option>
              </select>
            </label>

            <label>
              Tipo
              <select formControlName="type">
                <option value="Conferencia">Conferencia</option>
                <option value="Taller">Taller</option>
                <option value="Concierto">Concierto</option>
              </select>
            </label>

            <label>
              Capacidad máxima
              <input type="number" formControlName="maxCapacity" min="1" [attr.max]="selectedVenueCapacity() ?? null" />
            </label>

            <label>
              Precio por entrada
              <input type="number" formControlName="ticketPrice" step="0.01" />
            </label>

            <label>
              Inicio
              <input type="datetime-local" formControlName="startsAt" [min]="minStartDateTime" />
            </label>

            <label>
              Fin
              <input type="datetime-local" formControlName="endsAt" [min]="minEndDateTime" />
            </label>
          </div>

          <p *ngIf="form.errors?.['startInPast']" class="validation">El inicio debe estar en el futuro.</p>
          <p *ngIf="form.errors?.['endNotAfterStart']" class="validation">El fin debe ser posterior al inicio.</p>
          <p *ngIf="error()" class="error">{{ error() }}</p>

          <div class="form-actions">
            <a routerLink="/events" class="button button-secondary">Cancelar</a>
            <button type="submit" [disabled]="form.invalid">Crear evento</button>
          </div>
        </form>
      </div>
    </section>
  `
})
export class EventCreateComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(EventsService);
  private readonly venuesSvc = inject(VenuesService);
  private readonly router = inject(Router);

  venues = signal<Venue[]>([]);
  selectedVenueCapacity = signal<number | null>(null);
  error = signal('');
  minStartDateTime = '';
  minEndDateTime = '';

  form = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
    venueId: [null as number | null, Validators.required],
    maxCapacity: [1, [Validators.required, Validators.min(1)]],
    startsAt: ['', Validators.required],
    endsAt: ['', Validators.required],
    ticketPrice: [0.01, [Validators.required, Validators.min(0.01)]],
    type: ['Conferencia', Validators.required]
  }, { validators: EventCreateComponent.eventDateRangeValidator });

  ngOnInit() {
    this.venuesSvc.getVenues().subscribe(v => {
      this.venues.set(v);
      this.updateVenueCapacity();
    });
    const start = this.nextBusinessDayAt(14, 18, 0);
    const end = new Date(start.getTime() + 2 * 60 * 60 * 1000);
    this.minStartDateTime = this.toDateTimeLocalInput(new Date());
    this.form.patchValue({
      startsAt: this.toDateTimeLocalInput(start),
      endsAt: this.toDateTimeLocalInput(end)
    });
    this.updateEndMinimum(this.form.controls.startsAt.value);
    this.form.controls.startsAt.valueChanges.subscribe(value => this.updateEndMinimum(value));
    this.form.controls.venueId.valueChanges.subscribe(() => this.updateVenueCapacity());
  }

  submit() {
    this.form.updateValueAndValidity();
    if (this.form.invalid) return;
    const v = this.form.value;
    this.error.set('');
    this.svc.createEvent({
      title: v.title!,
      description: v.description!,
      venueId: Number(v.venueId),
      maxCapacity: v.maxCapacity!,
      startsAt: new Date(v.startsAt!).toISOString(),
      endsAt: new Date(v.endsAt!).toISOString(),
      ticketPrice: v.ticketPrice!,
      type: v.type!
    }).subscribe({
      next: () => this.router.navigate(['/events']),
      error: (e) => this.error.set(e.error?.detail ?? 'Error al crear el evento.')
    });
  }

  private nextBusinessDayAt(daysAhead: number, hour: number, minute: number): Date {
    const date = new Date();
    date.setDate(date.getDate() + daysAhead);
    date.setHours(hour, minute, 0, 0);

    while (date.getDay() === 0 || date.getDay() === 6) {
      date.setDate(date.getDate() + 1);
    }

    return date;
  }

  private toDateTimeLocalInput(date: Date): string {
    const pad = (value: number) => value.toString().padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  private updateEndMinimum(startsAt: string | null) {
    const start = startsAt ? new Date(startsAt) : new Date();
    const minimumEnd = new Date(start.getTime() + 60_000);
    this.minEndDateTime = this.toDateTimeLocalInput(minimumEnd);
  }

  private updateVenueCapacity() {
    const venue = this.venues().find(v => v.id === Number(this.form.controls.venueId.value));
    const capacity = venue?.capacity ?? null;
    this.selectedVenueCapacity.set(capacity);
    this.form.controls.maxCapacity.setValidators([
      Validators.required,
      Validators.min(1),
      ...(capacity ? [Validators.max(capacity)] : [])
    ]);
    this.form.controls.maxCapacity.updateValueAndValidity({ emitEvent: false });
  }

  private static eventDateRangeValidator(control: AbstractControl) {
    const { startsAt, endsAt } = control.value as { startsAt?: string; endsAt?: string };
    if (!startsAt || !endsAt) return null;

    const start = new Date(startsAt);
    const end = new Date(endsAt);
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return null;
    if (start.getTime() <= Date.now()) return { startInPast: true };
    if (end.getTime() <= start.getTime()) return { endNotAfterStart: true };
    return null;
  }
}
