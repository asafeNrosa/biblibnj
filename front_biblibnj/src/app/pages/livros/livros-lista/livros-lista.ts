import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LivroService, LivroReadDto } from '../../../services/livro.service';
import { AuthService } from '../../../services/auth.service';
import { EmprestimoService } from '../../../services/emprestimo.service';
import { FilaEsperaService } from '../../../services/fila-espera.service';

@Component({
  selector: 'app-livro-lista',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './livros-lista.html',
  styleUrl: './livros-lista.css'
})
export class LivroListaComponent implements OnInit {
  // Injeção de dependências
  private livroService = inject(LivroService);
  private emprestimoService = inject(EmprestimoService);
  private filaEsperaService = inject(FilaEsperaService);
  public authService = inject(AuthService);

  // Estados com Signals
  livros = signal<LivroReadDto[]>([]);
  carregando = signal<boolean>(true);
  erroMsg = signal<string | null>(null);

  termoBusca: string = '';
  filtroStatus: 'todos' | 'disponiveis' | 'esgotados' = 'todos';

  ngOnInit(): void {
    this.carregarLivros();
  }

  carregarLivros(): void {
    this.carregando.set(true);
    this.erroMsg.set(null);

    this.livroService.obterTodos().subscribe({
      next: (dados) => {
        this.livros.set(dados);
        this.carregando.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar livros:', err);
        this.erroMsg.set('Não foi possível conectar com o servidor para buscar o catálogo.');
        this.carregando.set(false);
      }
    });
  }

  get livrosFiltrados(): LivroReadDto[] {
    return this.livros().filter(livro => {
      const termo = this.termoBusca.toLowerCase().trim();
      const combinaTermo = !termo || 
        livro.titulo.toLowerCase().includes(termo) ||
        livro.autor.toLowerCase().includes(termo) ||
        livro.isbn.includes(termo);

      if (this.filtroStatus === 'disponiveis') {
        return combinaTermo && livro.quantidadeDisponivel > 0;
      }
      if (this.filtroStatus === 'esgotados') {
        return combinaTermo && livro.quantidadeDisponivel === 0;
      }

      return combinaTermo;
    });
  }

  solicitarEmprestimo(livro: LivroReadDto): void {
    this.emprestimoService.solicitarEmprestimo(livro.id).subscribe({
      next: () => {
        alert(`Solicitação de empréstimo para "${livro.titulo}" realizada com sucesso!`);
        this.carregarLivros();
      },
      error: (err) => {
  if (err.status === 401) {
    alert('Efetue login para realizar empréstimos.');
    return;
  }
  alert(err.error?.mensagem || 'Erro ao solicitar empréstimo.');
}
    });
  }

  entrarNaFila(livro: LivroReadDto): void {
    this.filaEsperaService.entrarNaFila(livro.id).subscribe({
      next: (res) => {
        alert(res.mensagem || `Você entrou na fila de espera para "${livro.titulo}".`);
      },
      error: (err) => {
  if (err.status === 401) {
    alert('Efetue login para entrar na fila de espera.');
    return;
  }
  alert(err.error?.mensagem || 'Erro ao entrar na fila de espera.');
}
    });
  }

  excluirLivro(id: number, titulo: string): void {
    if (confirm(`Tem certeza que deseja remover "${titulo}" do acervo?`)) {
      this.livroService.excluir(id).subscribe({
        next: (res) => {
          alert(res.mensagem || 'Livro removido com sucesso.');
          this.carregarLivros();
        },
        error: (err) => alert(err.error?.mensagem || 'Erro ao excluir o livro.')
      });
    }
  }
}