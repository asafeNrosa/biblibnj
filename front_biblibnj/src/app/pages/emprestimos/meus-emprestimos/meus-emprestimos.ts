import { Component } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';


interface Emprestimo {
  id: number;
  idLivro: number;
  tituloLivro: string;
  autorLivro: string;
  dataEmprestimo: Date;
  dataPrevisaoDevolucao: Date;
  dataDevolucaoEfetiva?: Date;
}

@Component({
  selector: 'app-meus-emprestimos',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './meus-emprestimos.html',
  styleUrl: './meus-emprestimos.css'
})
export class MeusEmprestimosComponent {

  meusEmprestimos: Emprestimo[] = [
    {
      id: 101,
      idLivro: 2,
      tituloLivro: 'Clean Code',
      autorLivro: 'Robert C. Martin',
      dataEmprestimo: new Date('2023-10-15'),
      dataPrevisaoDevolucao: new Date('2023-10-29') // Sem atraso
    },
    {
      id: 102,
      idLivro: 1,
      tituloLivro: 'O Hobbit',
      autorLivro: 'J.R.R. Tolkien',
      dataEmprestimo: new Date('2023-09-01'),
      dataPrevisaoDevolucao: new Date('2023-09-15') // Com atraso
    },
    {
      id: 103,
      idLivro: 4,
      tituloLivro: '1984',
      autorLivro: 'George Orwell',
      dataEmprestimo: new Date('2023-10-01'),
      dataPrevisaoDevolucao: new Date('2023-10-15'),
      dataDevolucaoEfetiva: new Date('2023-10-14') // Devolvido em dia
    }
  ];

  get emprestimosEmAberto(): Emprestimo[] {
    return this.meusEmprestimos.filter(e => !e.dataDevolucaoEfetiva);
  }

  get totalAtrasados(): number {
    const hoje = new Date();
    return this.emprestimosEmAberto.filter(e => e.dataPrevisaoDevolucao < hoje).length;
  }

  calcularStatus(emprestimo: Emprestimo): { text: string; class: string } {
    if (emprestimo.dataDevolucaoEfetiva) {
      return { text: 'Devolvido', class: 'status-returned' };
    }

    const hoje = new Date();
    if (emprestimo.dataPrevisaoDevolucao < hoje) {
      return { text: 'Atrasado', class: 'status-late' };
    }

    return { text: 'Em Aberto', class: 'status-open' };
  }

  devolverLivro(idEmprestimo: number): void {
    if (confirm('Tem certeza que deseja solicitar a devolução deste livro?')) {
      alert(`Solicitação de devolução para o empréstimo #${idEmprestimo} enviada!`);
      
      const emprestimo = this.meusEmprestimos.find(e => e.id === idEmprestimo);
      if (emprestimo) {
        emprestimo.dataDevolucaoEfetiva = new Date(); // Define a data de hoje como devolução
      }
    }
  }
}