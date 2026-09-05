import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {
  isLoggedIn: boolean = true;
  isAdmin: boolean = true;
  menuAberto: boolean = false;

  nomeUsuario: string = 'Carlos Silva';

  constructor(private router: Router) {}

  toggleMenu(): void {
    this.menuAberto = !this.menuAberto;
  }

  fecharMenu(): void {
    this.menuAberto = false;
  }

  togglePerfil(): void {
    this.isAdmin = !this.isAdmin;
  }

  logout(): void {
    this.isLoggedIn = false;
    this.fecharMenu();
    this.router.navigate(['/login']);
  }

  login(): void {
    this.router.navigate(['/login']);
  }
}