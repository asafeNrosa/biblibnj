import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmprestimoService, EmprestimoReadDto, UsuarioComMultaDto } from '../../../services/emprestimo.service';
import { FilaEsperaService, PosicaoFilaReadDto } from '../../../services/fila-espera.service';

@Component({
  selector: 'app-gerenciar-emprestimos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './gerenciar-emprestimos.html',
  styleUrl: './gerenciar-emprestimos.css',
})
export class GerenciarEmprestimos implements OnInit {
  private emprestimoService = inject(EmprestimoService);
  private filaEsperaService = inject(FilaEsperaService);

  abaAtiva = signal<'pendentes' | 'ativos' | 'multas' | 'fila'>('pendentes');

  pendentes = signal<EmprestimoReadDto[]>([]);
  ativos = signal<EmprestimoReadDto[]>([]);
  usuariosComMulta = signal<UsuarioComMultaDto[]>([]);
  filasDeEspera = signal<PosicaoFilaReadDto[]>([]);

  carregando = signal<boolean>(true);
  erroMsg = signal<string | null>(null);

  ngOnInit(): void {
    this.carregarTudo();
  }

  carregarTudo(): void {
    this.carregando.set(true);
    this.erroMsg.set(null);

    this.emprestimoService.obterPendentes().subscribe({
      next: (dados) => this.pendentes.set(dados),
      error: (err) => this.tratarErroCarregamento(err)
    });

    this.emprestimoService.obterTodosEmprestimos().subscribe({
      next: (dados) => {
        this.ativos.set(dados);
        this.carregando.set(false);
      },
      error: (err) => this.tratarErroCarregamento(err)
    });

    this.emprestimoService.obterUsuariosComMulta().subscribe({
      next: (dados) => this.usuariosComMulta.set(dados)
    });

    this.filaEsperaService.obterTodasAsFilas().subscribe({
      next: (dados) => this.filasDeEspera.set(dados)
    });
  }

  podeAutorizar(fila: PosicaoFilaReadDto): boolean {
    return fila.posicao === 1 && fila.quantidadeDisponivel > 0;
  }

  autorizar(fila: PosicaoFilaReadDto): void {
    if (!confirm(`Autorizar o empréstimo de "${fila.tituloLivro}" para ${fila.nomeUsuario}?`)) {
      return;
    }
    this.filaEsperaService.autorizar(fila.id).subscribe({
      next: (res) => {
        alert(res.mensagem);
        this.carregarTudo();
      },
      error: (err) => alert(err.error?.mensagem || 'Erro ao autorizar o empréstimo.')
    });
  }

  private tratarErroCarregamento(err: any): void {
    console.error('Erro ao carregar dados de gerenciamento:', err);
    this.erroMsg.set('Não foi possível carregar os dados de empréstimos.');
    this.carregando.set(false);
  }

  aprovar(id: number): void {
    this.emprestimoService.aprovar(id).subscribe({
      next: () => {
        alert('Empréstimo aprovado com sucesso.');
        this.carregarTudo();
      },
      error: (err) => alert(err.error?.mensagem || 'Erro ao aprovar o empréstimo.')
    });
  }

  vetar(id: number): void {
    if (!confirm('Tem certeza que deseja recusar esta solicitação de empréstimo?')) {
      return;
    }
    this.emprestimoService.vetar(id).subscribe({
      next: (res) => {
        alert(res.mensagem);
        this.carregarTudo();
      },
      error: (err) => alert(err.error?.mensagem || 'Erro ao recusar o empréstimo.')
    });
  }

  devolver(id: number): void {
    if (!confirm('Confirmar a devolução deste livro?')) {
      return;
    }
    this.emprestimoService.devolverLivro(id).subscribe({
      next: (res) => {
        alert(res.mensagem);
        this.carregarTudo();
      },
      error: (err) => alert(err.error?.mensagem || 'Erro ao registrar a devolução.')
    });
  }

  liberarUsuario(usuarioId: number, nome: string): void {
    if (!confirm(`Confirmar que ${nome} pagou a multa e pode voltar a solicitar empréstimos?`)) {
      return;
    }
    this.emprestimoService.liberarUsuario(usuarioId).subscribe({
      next: (res) => {
        alert(res.mensagem);
        this.carregarTudo();
      },
      error: (err) => alert(err.error?.mensagem || 'Erro ao liberar o usuário.')
    });
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Atrasado': return 'status-late';
      case 'EmAberto': return 'status-open';
      default: return 'status-open';
    }
  }
}