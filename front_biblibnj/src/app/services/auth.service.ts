import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface LoginResponse {
  token: string;
  nome: string;
  email: string;
  perfil: 'Admin' | 'Leitor';
  expiracao: string;
}

export interface CadastroRequest {
  nome: string;
  email: string;
  senha: string;
  telefone: string;
  rua: string;
  numero: string;
  cidade: string;
  cep: string;
}

export interface RecuperarSenhaResponse {
  mensagem: string;
  canalSimulado: string;
  conteudoSimulado: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://biblibnj-api-gud0h2fjgccpc2c3.brazilsouth-01.azurewebsites.net/api/Auth';

  isLoggedIn = signal<boolean>(!!localStorage.getItem('token'));
  isAdmin = signal<boolean>(localStorage.getItem('user_role') === 'Admin');
  usuarioAtual = signal<string | null>(localStorage.getItem('user_name'));

  constructor(private http: HttpClient) {}

  login(credentials: { email: string; senha: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((res) => this.aplicarSessao(res))
    );
  }

  cadastrar(dados: CadastroRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/cadastrar`, dados).pipe(
      tap((res) => this.aplicarSessao(res))
    );
  }

  recuperarSenha(emailOuTelefone: string): Observable<RecuperarSenhaResponse> {
    return this.http.post<RecuperarSenhaResponse>(`${this.apiUrl}/recuperar-senha`, { emailOuTelefone });
  }

  alterarSenha(senhaAtual: string, novaSenha: string): Observable<{ mensagem: string }> {
    return this.http.put<{ mensagem: string }>(`${this.apiUrl}/alterar-senha`, { senhaAtual, novaSenha });
  }

  private aplicarSessao(res: LoginResponse): void {
    localStorage.setItem('token', res.token);
    localStorage.setItem('user_role', res.perfil);
    localStorage.setItem('user_name', res.nome);

    this.isLoggedIn.set(true);
    this.isAdmin.set(res.perfil === 'Admin');
    this.usuarioAtual.set(res.nome);
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