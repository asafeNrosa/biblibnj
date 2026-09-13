import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PosicaoFilaReadDto {
  livroId: number;
  tituloLivro: string;
  posicao: number;
  dataEntrada: string;
  quantidadeDisponivel: number;
  mensagem: string;
}

@Injectable({
  providedIn: 'root'
})
export class FilaEsperaService {
  private apiUrl = 'https://biblibnj-api-gud0h2fjgccpc2c3.brazilsouth-01.azurewebsites.net/api/filaespera';

  constructor(private http: HttpClient) {}

  entrarNaFila(livroId: number): Observable<PosicaoFilaReadDto> {
    return this.http.post<PosicaoFilaReadDto>(`${this.apiUrl}/entrar`, { livroId });
  }

  obterMinhaPosicao(livroId: number): Observable<PosicaoFilaReadDto> {
    return this.http.get<PosicaoFilaReadDto>(`${this.apiUrl}/posicao/${livroId}`);
  }

    obterMinhasFilas(): Observable<PosicaoFilaReadDto[]> {

    return this.http.get<PosicaoFilaReadDto[]>(`${this.apiUrl}/minhas`);

  }

  sairDaFila(livroId: number): Observable<{ mensagem: string }> {

    return this.http.delete<{ mensagem: string }>(`${this.apiUrl}/sair/${livroId}`);

  }

}