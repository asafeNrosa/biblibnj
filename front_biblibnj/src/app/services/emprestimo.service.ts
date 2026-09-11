import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EmprestimoReadDto {
  id: number;
  livroId: number;
  tituloLivro: string;
  isbnLivro: string;
  usuarioId: number;
  nomeUsuario: string;
  emailUsuario: string;
  dataEmprestimo: string;
  dataDevolucaoPrevista: string;
  dataDevolucaoReal?: string;
  renovacoesRealizadas: number;
  status: 'Pendente' | 'EmAberto' | 'Atrasado' | 'Devolvido' | 'Rejeitado';
  multaEstimada: number;
}

export interface EmprestimoCreateDto {
  livroId: number;
}

export interface UsuarioComMultaDto {
  id: number;
  nome: string;
  email: string;
  multaPendente: number;
}

@Injectable({
  providedIn: 'root'
})
export class EmprestimoService {
  private apiUrl = 'https://localhost:7206/api/emprestimos';

  constructor(private http: HttpClient) {}

  obterMeusEmprestimos(): Observable<EmprestimoReadDto[]> {
    return this.http.get<EmprestimoReadDto[]>(`${this.apiUrl}/meus`);
  }

  obterTodosEmprestimos(): Observable<EmprestimoReadDto[]> {
    return this.http.get<EmprestimoReadDto[]>(`${this.apiUrl}/todos`);
  }

  obterPendentes(): Observable<EmprestimoReadDto[]> {
    return this.http.get<EmprestimoReadDto[]>(`${this.apiUrl}/pendentes`);
  }

  obterHistorico(): Observable<EmprestimoReadDto[]> {
    return this.http.get<EmprestimoReadDto[]>(`${this.apiUrl}/historico`);
  }

  obterUsuariosComMulta(): Observable<UsuarioComMultaDto[]> {
    return this.http.get<UsuarioComMultaDto[]>(`${this.apiUrl}/usuarios/com-multa`);
  }

  solicitarEmprestimo(livroId: number): Observable<EmprestimoReadDto> {
    const dto: EmprestimoCreateDto = { livroId };
    return this.http.post<EmprestimoReadDto>(this.apiUrl, dto);
  }

  aprovar(id: number): Observable<EmprestimoReadDto> {
    return this.http.put<EmprestimoReadDto>(`${this.apiUrl}/${id}/aprovar`, {});
  }

  vetar(id: number): Observable<{ mensagem: string }> {
    return this.http.put<{ mensagem: string }>(`${this.apiUrl}/${id}/vetar`, {});
  }

  renovar(id: number): Observable<{ mensagem: string; emprestimo: EmprestimoReadDto }> {
    return this.http.put<{ mensagem: string; emprestimo: EmprestimoReadDto }>(`${this.apiUrl}/${id}/renovar`, {});
  }

  devolverLivro(emprestimoId: number): Observable<{ mensagem: string; multaGerada: number }> {
    return this.http.put<{ mensagem: string; multaGerada: number }>(`${this.apiUrl}/${emprestimoId}/devolver`, {});
  }

  liberarUsuario(usuarioId: number): Observable<{ mensagem: string }> {
    return this.http.put<{ mensagem: string }>(`${this.apiUrl}/usuarios/liberar`, { usuarioId });
  }
}
