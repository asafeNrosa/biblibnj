import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface Livro {
  id: number;
  titulo: string;
  autor: string;
  isbn: string;
  quantidadeTotal: number;
  quantidadeDisponivel: number;
  categoria: string;
}

@Component({
  selector: 'app-livro-lista',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './livros-lista.html',
  styleUrl: './livros-lista.css'
})
export class LivroListaComponent {
  isLoggedIn: boolean = true;
  isAdmin: boolean = false;

  termoBusca: string = '';
  filtroStatus: 'todos' | 'disponiveis' | 'esgotados' = 'todos';

  livros: Livro[] = [
    { id: 1, titulo: 'O Hobbit', autor: 'J.R.R. Tolkien', isbn: '978-8595084742', quantidadeTotal: 5, quantidadeDisponivel: 3, categoria: 'Fantasia' },
    { id: 2, titulo: 'Clean Code', autor: 'Robert C. Martin', isbn: '978-8576082675', quantidadeTotal: 2, quantidadeDisponivel: 1, categoria: 'Tecnologia' },
    { id: 3, titulo: 'Duna', autor: 'Frank Herbert', isbn: '978-8525056009', quantidadeTotal: 3, quantidadeDisponivel: 0, categoria: 'Ficção Científica' },
    { id: 4, titulo: '1984', autor: 'George Orwell', isbn: '978-8535909555', quantidadeTotal: 6, quantidadeDisponivel: 5, categoria: 'Distopia' },
    { id: 5, titulo: 'O Algoritmo Mestre', autor: 'Pedro Domingos', isbn: '978-8575422328', quantidadeTotal: 2, quantidadeDisponivel: 2, categoria: 'Tecnologia' },
    { id: 6, titulo: 'Dom Casmurro', autor: 'Machado de Assis', isbn: '978-8508123456', quantidadeTotal: 4, quantidadeDisponivel: 0, categoria: 'Clássico' }
  ];

  get livrosFiltrados(): Livro[] {
    return this.livros.filter(livro => {
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

  solicitarEmprestimo(livro: Livro): void {
    alert(`Solicitação de empréstimo para o livro "${livro.titulo}" realizada!`);
  }

  entrarNaFila(livro: Livro): void {
    alert(`Você entrou na fila de espera para o livro "${livro.titulo}".`);
  }

  excluirLivro(id: number): void {
    if (confirm('Tem certeza que deseja remover este livro do acervo?')) {
      this.livros = this.livros.filter(l => l.id !== id);
    }
  }
}