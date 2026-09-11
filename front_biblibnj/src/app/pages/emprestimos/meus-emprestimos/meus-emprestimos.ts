import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EmprestimoService, EmprestimoReadDto } from '../../../services/emprestimo.service';

@Component({
  selector: 'app-meus-emprestimos',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './meus-emprestimos.html',
  styleUrl: './meus-emprestimos.css'
})
export class MeusEmprestimosComponent implements OnInit {
  private emprestimoService = inject(EmprestimoService);

  meusEmprestimos = signal<EmprestimoReadDto[]>([]);
  carregando = signal<boolean>(true);
  erroMsg = signal<string | null>(null);

  private readonly LIMITE_RENOVACOES = 2;

  ngOnInit(): void {
    this.carregarEmprestimos();
  }

  carregarEmprestimos(): void {
    this.carregando.set(true);
    this.erroMsg.set(null);

    this.emprestimoService.obterMeusEmprestimos().subscribe({
      next: (dados) => {
        this.meusEmprestimos.set(dados);
        this.carregando.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar empréstimos:', err);
        this.erroMsg.set('Não foi possível carregar os seus empréstimos.');
        this.carregando.set(false);
      }
    });
  }

  get emprestimosAtivos(): EmprestimoReadDto[] {
    return this.meusEmprestimos().filter(e => e.status !== 'Devolvido' && e.status !== 'Rejeitado');
  }

  get totalAtrasados(): number {
    return this.emprestimosAtivos.filter(e => e.status === 'Atrasado').length;
  }

  statusInfo(emprestimo: EmprestimoReadDto): { text: string; class: string } {
    switch (emprestimo.status) {
      case 'Pendente':
        return { text: 'Aguardando aprovação', class: 'status-pending' };
      case 'Atrasado':
        return { text: 'Atrasado', class: 'status-late' };
      case 'Devolvido':
        return { text: 'Devolvido', class: 'status-returned' };
      case 'Rejeitado':
        return { text: 'Recusado pelo admin', class: 'status-rejected' };
      default:
        return { text: 'Em Aberto', class: 'status-open' };
    }
  }

  podeRenovar(emprestimo: EmprestimoReadDto): boolean {
    return (emprestimo.status === 'EmAberto' || emprestimo.status === 'Atrasado') &&
           emprestimo.renovacoesRealizadas < this.LIMITE_RENOVACOES;
  }

  podeDevolver(emprestimo: EmprestimoReadDto): boolean {
    return emprestimo.status === 'EmAberto' || emprestimo.status === 'Atrasado';
  }

  renovarEmprestimo(idEmprestimo: number): void {
    this.emprestimoService.renovar(idEmprestimo).subscribe({
      next: (res) => {
        alert(res.mensagem);
        this.carregarEmprestimos();
      },
      error: (err) => {
        console.error('Erro ao renovar empréstimo:', err);
        alert(err.error?.mensagem || 'Erro ao renovar o empréstimo.');
      }
    });
  }

  devolverLivro(idEmprestimo: number): void {
    if (confirm('Tem certeza que deseja solicitar a devolução deste livro?')) {
      this.emprestimoService.devolverLivro(idEmprestimo).subscribe({
        next: (res) => {
          alert(res.mensagem);
          this.carregarEmprestimos();
        },
        error: (err) => {
          console.error('Erro ao devolver livro:', err);
          alert(err.error?.mensagem || 'Erro ao processar a devolução.');
        }
      });
    }
  }
}