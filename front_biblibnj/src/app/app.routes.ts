import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'livros', pathMatch: 'full' },
  {
    path: 'livros',
    loadComponent: () => import('./pages/livros/livros-lista/livros-lista').then(m => m.LivroListaComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'emprestimos/meus-emprestimos',
    loadComponent: () => import('./pages/emprestimos/meus-emprestimos/meus-emprestimos').then(m => m.MeusEmprestimosComponent)
  },
  {
    path: 'emprestimos/fila-espera',
    loadComponent: () => import('./pages/fila-espera/fila-espera').then(m => m.FilaEspera)
  },
  {
    path: 'admin/livros/novo',
    loadComponent: () => import('./pages/livros/livros-form/livros-form').then(m => m.LivrosFormComponent)
  },
  {
    path: 'admin/livros/editar/:id',
    loadComponent: () => import('./pages/livros/livros-form/livros-form').then(m => m.LivrosFormComponent)
  },
  { path: '**', redirectTo: 'livros' }
];