import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReservationsService } from '../../shared/reservations.service';

@Component({
  selector: 'app-reservation-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page">
      <div class="page-header">
        <div class="page-title">
          <p class="eyebrow">Compra</p>
          <h1>Reservar entradas</h1>
        </div>
        <a routerLink="/events" class="button button-secondary">Volver</a>
      </div>

      <div class="form-panel">
        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="form-grid">
            <label>
              Nombre
              <input formControlName="buyerName" />
            </label>

            <label>
              Email
              <input type="email" formControlName="buyerEmail" />
            </label>

            <label>
              Cantidad de entradas
              <input type="number" formControlName="quantity" min="1" />
            </label>
          </div>

          <p *ngIf="error()" class="error">{{ error() }}</p>

          <div *ngIf="success()" class="success">
            <p>Reserva creada. Código: <strong>{{ reservationCode() || 'Pendiente de confirmación' }}</strong></p>
            <p>Estado: {{ reservationState() }}</p>
          </div>

          <div class="form-actions">
            <a routerLink="/events" class="button button-secondary">Cancelar</a>
            <button type="submit" [disabled]="form.invalid || success()">Reservar</button>
          </div>
        </form>
      </div>
    </section>
  `
})
export class ReservationCreateComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(ReservationsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  eventId = 0;
  error = signal('');
  success = signal(false);
  reservationCode = signal<string | undefined>(undefined);
  reservationState = signal('');

  form = this.fb.group({
    buyerName: ['', [Validators.required, Validators.minLength(1)]],
    buyerEmail: ['', [Validators.required, Validators.email]],
    quantity: [1, [Validators.required, Validators.min(1)]]
  });

  ngOnInit() {
    this.eventId = Number(this.route.snapshot.paramMap.get('eventId'));
  }

  submit() {
    if (this.form.invalid) return;
    const v = this.form.value;
    this.error.set('');
    this.svc.createReservation({
      eventId: this.eventId,
      quantity: v.quantity!,
      buyerName: v.buyerName!,
      buyerEmail: v.buyerEmail!
    }).subscribe({
      next: (res) => {
        this.success.set(true);
        this.reservationCode.set(res.reservationCode);
        this.reservationState.set(res.state);
        setTimeout(() => this.router.navigate(['/reservations'], { queryParams: { eventId: this.eventId } }), 1500);
      },
      error: (e) => this.error.set(e.error?.detail ?? 'Error al crear la reserva.')
    });
  }
}
