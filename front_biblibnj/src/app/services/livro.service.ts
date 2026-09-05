import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Livro {
  id?: number;
  titulo: string;
  autor: string;
  isbn: string;
  categoria: string;
  anoPublicacao: number;
  exemplaresDisponiveis: number;
  capaUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class LivroService {
  private apiUrl = 'https://localhost:7123/api/livros';

  constructor(private http: HttpClient) {}

  obterTodos(termoBusca?: string, categoria?: string): Observable<Livro[]> {
    let params = new HttpParams();
    if (termoBusca) params = params.set('busca', termoBusca);
    if (categoria) params = params.set('categoria', categoria);

    return this.http.get<Livro[]>(this.apiUrl, { params });
  }

  obterPorId(id: number): Observable<Livro> {
    return this.http.get<Livro>(`${this.apiUrl}/${id}`);
  }

  criar(livro: Livro): Observable<Livro> {
    return this.http.post<Livro>(this.apiUrl, livro);
  }

  atualizar(id: number, livro: Livro): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, livro);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}