import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface LoginResponse {
  token: string;
  usuario: {
    id: number;
    nome: string;
    email: string;
    role: 'Admin' | 'Leitor';
  };
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7123/api/auth';

  isLoggedIn = signal<boolean>(!!localStorage.getItem('token'));
  isAdmin = signal<boolean>(localStorage.getItem('user_role') === 'Admin');
  usuarioAtual = signal<string | null>(localStorage.getItem('user_name'));

  constructor(private http: HttpClient) {}

  login(credentials: { email: string; senha: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((res) => {
        localStorage.setItem('token', res.token);
        localStorage.setItem('user_role', res.usuario.role);
        localStorage.setItem('user_name', res.usuario.nome);

        this.isLoggedIn.set(true);
        this.isAdmin.set(res.usuario.role === 'Admin');
        this.usuarioAtual.set(res.usuario.nome);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user_role');
    localStorage.removeItem('user_name');

    this.isLoggedIn.set(false);
    this.isAdmin.set(false);
    this.usuarioAtual.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }
}