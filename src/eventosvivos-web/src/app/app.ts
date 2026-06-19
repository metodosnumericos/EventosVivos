import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-shell">
      <header class="topbar">
        <a routerLink="/events" class="brand" aria-label="EventosVivos">
          <span class="brand-mark">EV</span>
          <span class="brand-copy">
            <strong>EventosVivos</strong>
            <small>Reservas</small>
          </span>
        </a>

        <nav class="main-nav" aria-label="Principal">
          <a routerLink="/events" routerLinkActive="active" class="nav-link">
            <span class="nav-icon">E</span>
            Eventos
          </a>
          <a routerLink="/reservations" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }" class="nav-link">
            <span class="nav-icon">A</span>
            Admin reservas
          </a>
          <a routerLink="/reservations/cancel" routerLinkActive="active" class="nav-link">
            <span class="nav-icon">C</span>
            Cancelar reserva
          </a>
        </nav>
      </header>

      <main class="content-shell">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
    }

    .app-shell {
      min-height: 100vh;
      background:
        linear-gradient(180deg, rgba(255,255,255,0.78), rgba(255,255,255,0.92) 240px),
        #f4f7f8;
    }

    .topbar {
      position: sticky;
      top: 0;
      z-index: 10;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 24px;
      min-height: 72px;
      padding: 14px clamp(18px, 4vw, 44px);
      border-bottom: 1px solid rgba(16, 24, 40, 0.08);
      background: rgba(255, 255, 255, 0.92);
      backdrop-filter: blur(14px);
      box-shadow: 0 12px 30px rgba(16, 24, 40, 0.06);
    }

    .brand {
      display: inline-flex;
      align-items: center;
      gap: 12px;
      color: #17202a;
      text-decoration: none;
      min-width: max-content;
    }

    .brand-mark {
      display: grid;
      place-items: center;
      width: 42px;
      height: 42px;
      border-radius: 8px;
      background: #0f766e;
      color: #ffffff;
      font-weight: 800;
      box-shadow: inset 0 -10px 18px rgba(0, 0, 0, 0.16);
    }

    .brand-copy {
      display: grid;
      gap: 1px;
      line-height: 1.1;
    }

    .brand-copy strong {
      font-size: 1rem;
    }

    .brand-copy small {
      color: #64748b;
      font-size: 0.78rem;
      font-weight: 600;
    }

    .main-nav {
      display: flex;
      align-items: center;
      gap: 8px;
      flex-wrap: wrap;
      justify-content: flex-end;
    }

    .nav-link {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      min-height: 40px;
      padding: 8px 12px;
      border: 1px solid transparent;
      border-radius: 8px;
      color: #475569;
      text-decoration: none;
      font-size: 0.92rem;
      font-weight: 700;
      transition: background 160ms ease, border-color 160ms ease, color 160ms ease;
    }

    .nav-link:hover,
    .nav-link.active {
      background: #ecfeff;
      border-color: #99f6e4;
      color: #115e59;
    }

    .nav-icon {
      display: grid;
      place-items: center;
      width: 22px;
      height: 22px;
      border-radius: 6px;
      background: #e2e8f0;
      color: #334155;
      font-size: 0.72rem;
      font-weight: 800;
    }

    .nav-link.active .nav-icon,
    .nav-link:hover .nav-icon {
      background: #0f766e;
      color: #ffffff;
    }

    .content-shell {
      width: min(1180px, calc(100% - 32px));
      margin: 0 auto;
      padding: 34px 0 48px;
    }

    @media (max-width: 760px) {
      .topbar {
        position: static;
        align-items: flex-start;
        flex-direction: column;
      }

      .main-nav {
        width: 100%;
        justify-content: stretch;
      }

      .nav-link {
        flex: 1 1 100%;
      }

      .content-shell {
        width: min(100% - 24px, 1180px);
        padding-top: 22px;
      }
    }
  `]
})
export class App {}
