import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ReservationsService } from '../../shared/reservations.service';

@Component({
  selector: 'app-buyer-cancel',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page">
      <div class="page-header">
        <div class="page-title">
          <p class="eyebrow">Autoservicio</p>
          <h1>Cancelar mi reserva</h1>
        </div>
        <a routerLink="/events" class="button button-secondary">Volver</a>
      </div>

      <div class="form-panel">
        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="form-grid">
            <label>
              ID de reserva
              <input type="number" formControlName="reservationId" />
            </label>

            <label>
              Email del comprador
              <input type="email" formControlName="buyerEmail" />
            </label>

            <label class="field-full">
              Código de reserva
              <input formControlName="reservationCode" placeholder="EV-######" />
            </label>
          </div>

          <p *ngIf="error()" class="error">{{ error() }}</p>
          <p *ngIf="success()" class="success">Reserva cancelada exitosamente.</p>

          <div class="form-actions">
            <a routerLink="/events" class="button button-secondary">Cancelar</a>
            <button type="submit" [disabled]="form.invalid || success()" class="button-danger">Cancelar reserva</button>
          </div>
        </form>
      </div>
    </section>
  `
})
export class BuyerCancelComponent {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(ReservationsService);

  error = signal('');
  success = signal(false);

  form = this.fb.group({
    reservationId: [null as number | null, [Validators.required, Validators.min(1)]],
    buyerEmail: ['', [Validators.required, Validators.email]],
    reservationCode: ['']
  });

  submit() {
    if (this.form.invalid) return;
    const v = this.form.value;
    this.error.set('');
    this.svc.buyerCancel(v.reservationId!, v.buyerEmail!, v.reservationCode || undefined)
      .subscribe({
        next: () => this.success.set(true),
        error: (e) => this.error.set(e.error?.detail ?? 'Error al cancelar la reserva.')
      });
  }
}
