import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {
  authService = inject(AuthService);
  private router = inject(Router);

  menuAberto: boolean = false;


  toggleMenu(): void {
    this.menuAberto = !this.menuAberto;
  }

  fecharMenu(): void {
    this.menuAberto = false;
  }

  logout(): void {
    this.authService.logout();
    this.fecharMenu();
    this.router.navigate(['/login']);
  }

}