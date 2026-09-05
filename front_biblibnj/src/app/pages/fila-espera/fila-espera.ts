import { Component } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

interface ReservaFila {
  id: number;
  idLivro: number;
  tituloLivro: string;
  autorLivro: string;
  categoria: string;
  posicao: number;
  totalNaFila: number;
  dataSolicitacao: Date;
  status: 'Aguardando' | 'DisponivelParaRetirada';
}

@Component({
  selector: 'app-fila-espera',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './fila-espera.html',
  styleUrl: './fila-espera.css'
})
export class FilaEspera {
  reservas: ReservaFila[] = [
    {
      id: 201,
      idLivro: 3,
      tituloLivro: 'Duna',
      autorLivro: 'Frank Herbert',
      categoria: 'Ficção Científica',
      posicao: 1,
      totalNaFila: 3,
      dataSolicitacao: new Date('2023-10-10'),
      status: 'DisponivelParaRetirada'
    },
    {
      id: 202,
      idLivro: 6,
      tituloLivro: 'Dom Casmurro',
      autorLivro: 'Machado de Assis',
      categoria: 'Clássico',
      posicao: 4,
      totalNaFila: 6,
      dataSolicitacao: new Date('2023-10-18'),
      status: 'Aguardando'
    }
  ];

  get totalReservasAtivas(): number {
    return this.reservas.length;
  }

  get totalDisponiveisRetirada(): number {
    return this.reservas.filter(r => r.status === 'DisponivelParaRetirada').length;
  }

  cancelarReserva(reserva: ReservaFila): void {
    if (confirm(`Deseja realmente sair da fila de espera do livro "${reserva.tituloLivro}"?`)) {
      this.reservas = this.reservas.filter(r => r.id !== reserva.id);
      alert(`Sua reserva para "${reserva.tituloLivro}" foi cancelada.`);
    }
  }

  confirmarEmprestimo(reserva: ReservaFila): void {
    alert(`Empréstimo do livro "${reserva.tituloLivro}" confirmado com sucesso! Retire no balcão.`);
    this.reservas = this.reservas.filter(r => r.id !== reserva.id);
  }
}