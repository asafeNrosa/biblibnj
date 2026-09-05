import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface Livro {
  id: number;
  titulo: string;
  autor: string;
  isbn: string;
  quantidadeDisponivel: number;
  categoria: string;
  imagemCapa?: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class HomeComponent {
  // Simulando os 5 lançamentos mais recentes
  lancamentos: Livro[] = [
    { id: 1, titulo: 'O Hobbit', autor: 'J.R.R. Tolkien', isbn: '978-8595084742', quantidadeDisponivel: 3, categoria: 'Fantasia' },
    { id: 2, titulo: 'Clean Code', autor: 'Robert C. Martin', isbn: '978-8576082675', quantidadeDisponivel: 1, categoria: 'Tecnologia' },
    { id: 3, titulo: 'Duna', autor: 'Frank Herbert', isbn: '978-8525056009', quantidadeDisponivel: 0, categoria: 'Ficção Científica' },
    { id: 4, titulo: '1984', autor: 'George Orwell', isbn: '978-8535909555', quantidadeDisponivel: 5, categoria: 'Distopia' },
    { id: 5, titulo: 'O Algoritmo Mestre', autor: 'Pedro Domingos', isbn: '978-8575422328', quantidadeDisponivel: 2, categoria: 'Tecnologia' }
  ];

  // Simulando os livros mais emprestados
  maisEmprestados: Livro[] = [
    { id: 2, titulo: 'Clean Code', autor: 'Robert C. Martin', isbn: '978-8576082675', quantidadeDisponivel: 1, categoria: 'Tecnologia' },
    { id: 4, titulo: '1984', autor: 'George Orwell', isbn: '978-8535909555', quantidadeDisponivel: 5, categoria: 'Distopia' },
    { id: 6, titulo: 'Dom Casmurro', autor: 'Machado de Assis', isbn: '978-8508123456', quantidadeDisponivel: 0, categoria: 'Clássico' },
    { id: 1, titulo: 'O Hobbit', autor: 'J.R.R. Tolkien', isbn: '978-8595084742', quantidadeDisponivel: 3, categoria: 'Fantasia' }
  ];
}