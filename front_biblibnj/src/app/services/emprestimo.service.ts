import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Emprestimo {
  id: number;
  livroId: number;
  livroTitulo: string;
  dataEmprestimo: string;
  dataDevolucaoPrevista: string;
  dataDevolucaoReal?: string;
  status: 'Ativo' | 'Devolvido' | 'Atrasado';
}

export interface FilaEspera {
  id: number;
  livroId: number;
  livroTitulo: string;
  posicao: number;
  dataEntrada: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmprestimoService {
  private apiUrl = 'https://localhost:7123/api/emprestimos';

  constructor(private http: HttpClient) {}

  obterMeusEmprestimos(): Observable<Emprestimo[]> {
    return this.http.get<Emprestimo[]>(`${this.apiUrl}/meus-emprestimos`);
  }

  solicitarEmprestimo(livroId: number): Observable<Emprestimo> {
    return this.http.post<Emprestimo>(`${this.apiUrl}/solicitar`, { livroId });
  }

  devolverLivro(emprestimoId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${emprestimoId}/devolver`, {});
  }

  obterMinhaFilaEspera(): Observable<FilaEspera[]> {
    return this.http.get<FilaEspera[]>(`${this.apiUrl}/fila-espera`);
  }

  entrarNaFilaEspera(livroId: number): Observable<FilaEspera> {
    return this.http.post<FilaEspera>(`${this.apiUrl}/fila-espera/entrar`, { livroId });
  }

  sairDaFilaEspera(filaId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/fila-espera/${filaId}`);
  }
}