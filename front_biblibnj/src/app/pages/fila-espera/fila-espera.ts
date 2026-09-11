import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FilaEsperaService, PosicaoFilaReadDto } from '../../services/fila-espera.service';
import { EmprestimoService } from '../../services/emprestimo.service';

@Component({
  selector: 'app-fila-espera',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './fila-espera.html',
  styleUrl: './fila-espera.css'
})
export class FilaEsperaComponent implements OnInit {
  private filaEsperaService = inject(FilaEsperaService);
  private emprestimoService = inject(EmprestimoService);

  reservas = signal<PosicaoFilaReadDto[]>([]);
  carregando = signal<boolean>(false);
  erroMsg = signal<string | null>(null);

  ngOnInit(): void {
    this.carregarMinhasFilas();
  }

    carregarMinhasFilas(): void {
    this.carregando.set(true);
    this.erroMsg.set(null);
 
    this.filaEsperaService.obterMinhasFilas().subscribe({
      next: (dados) => {
        this.reservas.set(dados);
        this.carregando.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar filas de espera:', err);
        this.erroMsg.set(err.error?.mensagem || 'Não foi possível carregar suas filas de espera.');
        this.carregando.set(false);
      }
    });
  }

  consultarPosicao(livroId: number): void {
    this.carregando.set(true);
    this.erroMsg.set(null);

    this.filaEsperaService.obterMinhaPosicao(livroId).subscribe({
      next: (dados) => {
        this.reservas.update(atual => [...atual.filter(r => r.livroId !== livroId), dados]);
        this.carregando.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar posição na fila:', err);
        this.erroMsg.set(err.error?.mensagem || 'Não foi possível consultar sua posição na fila de espera.');
        this.carregando.set(false);
      }
    });
  }

  get totalReservasAtivas(): number {
    return this.reservas().length;
  }

    podeTentarEmprestimo(reserva: PosicaoFilaReadDto): boolean {
    return reserva.posicao === 1 && reserva.quantidadeDisponivel > 0;
  }
  

    sairDaFila(reserva: PosicaoFilaReadDto): void {
    if (!confirm(`Deseja sair da fila de espera do livro "${reserva.tituloLivro}"?`)) {
      return;
    }
 
    this.filaEsperaService.sairDaFila(reserva.livroId).subscribe({
      next: () => {
        this.reservas.update(list => list.filter(r => r.livroId !== reserva.livroId));
      },
      error: (err) => {
        console.error('Erro ao sair da fila:', err);
        alert(err.error?.mensagem || 'Erro ao sair da fila de espera.');
      }
    });
  }

  confirmarEmprestimo(reserva: PosicaoFilaReadDto): void {
    this.emprestimoService.solicitarEmprestimo(reserva.livroId).subscribe({
      next: () => {
        alert(`Empréstimo do livro "${reserva.tituloLivro}" confirmado com sucesso! Retire no balcão.`);
        this.reservas.update(list => list.filter(r => r.livroId !== reserva.livroId));
      },
      error: (err) => {
        console.error('Erro ao confirmar empréstimo:', err);
        alert(err.error?.mensagem || 'Erro ao efetivar empréstimo.');
      }
    });
  }
}