import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LivroReadDto {
  id: number;
  titulo: string;
  autor: string;
  isbn: string;
  editora: string;
  anoPublicacao: number;
  quantidadeTotal: number;
  quantidadeDisponivel: number;
}

export interface LivroCreateDto {
  titulo: string;
  autor: string;
  isbn: string;
  editora: string;
  anoPublicacao: number;
  quantidadeTotal: number;
}

export interface LivroUpdateDto {
  titulo: string;
  autor: string;
  editora: string;
  anoPublicacao: number;
}

export interface AjusteEstoqueDto {
  novaQuantidadeTotal: number;
}

@Injectable({
  providedIn: 'root'
})
export class LivroService {
  private apiUrl = 'https://localhost:7206/api/livros';

  constructor(private http: HttpClient) {}

  obterTodos(busca?: string): Observable<LivroReadDto[]> {
    let params = new HttpParams();
    if (busca) params = params.set('busca', busca);

    return this.http.get<LivroReadDto[]>(this.apiUrl, { params });
  }

  obterPorId(id: number): Observable<LivroReadDto> {
    return this.http.get<LivroReadDto>(`${this.apiUrl}/${id}`);
  }

  cadastrar(livro: LivroCreateDto): Observable<LivroReadDto> {
    return this.http.post<LivroReadDto>(this.apiUrl, livro);
  }

  atualizar(id: number, livro: LivroUpdateDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, livro);
  }

  ajustarEstoque(id: number, novaQuantidadeTotal: number): Observable<any> {
    const dto: AjusteEstoqueDto = { novaQuantidadeTotal };
    return this.http.patch<any>(`${this.apiUrl}/${id}/estoque`, dto);
  }

  excluir(id: number): Observable<{ mensagem: string }> {
    return this.http.delete<{ mensagem: string }>(`${this.apiUrl}/${id}`);
}
}