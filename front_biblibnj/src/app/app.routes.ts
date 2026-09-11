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
    path: 'cadastro',
    loadComponent: () => import('./pages/cadastro/cadastro').then(m => m.CadastroComponent)
  },
  {
    path: 'esqueci-senha',
    loadComponent: () => import('./pages/esqueci-senha/esqueci-senha').then(m => m.EsqueciSenhaComponent)
  },
  {
    path: 'conta/alterar-senha',
    loadComponent: () => import('./pages/alterar-senha/alterar-senha').then(m => m.AlterarSenhaComponent)
  },
  {
    path: 'emprestimos/meus-emprestimos',
    loadComponent: () => import('./pages/emprestimos/meus-emprestimos/meus-emprestimos').then(m => m.MeusEmprestimosComponent)
  },
  {
    path: 'emprestimos/fila-espera',
    loadComponent: () => import('./pages/fila-espera/fila-espera').then(m => m.FilaEsperaComponent)
  },
  {
    path: 'admin/gerenciar-emprestimos',
    loadComponent: () => import('./pages/emprestimos/gerenciar-emprestimos/gerenciar-emprestimos').then(m => m.GerenciarEmprestimos)
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