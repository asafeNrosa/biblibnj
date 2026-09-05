import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  loginForm: FormGroup;
  exibirSenha: boolean = false;
  mensagemErro: string = '';
  carregando: boolean = false;

  constructor(
    private fb: FormBuilder,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      senha: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  get email() {
    return this.loginForm.get('email');
  }

  get senha() {
    return this.loginForm.get('senha');
  }

  toggleMostrarSenha(): void {
    this.exibirSenha = !this.exibirSenha;
  }

  onSubmit(): void {
    this.mensagemErro = '';

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.carregando = true;

    setTimeout(() => {
      const { email, senha } = this.loginForm.value;

      if (email === 'admin@biblibnj.com' && senha === '123456') {
        alert('Login efetuado com sucesso (Perfil: Admin)!');
        this.router.navigate(['/livros']);
      } else if (email === 'usuario@biblibnj.com' && senha === '123456') {
        alert('Login efetuado com sucesso (Perfil: Usuário)!');
        this.router.navigate(['/livros']);
      } else {
        this.mensagemErro = 'E-mail ou senha inválidos. Tente novamente.';
      }

      this.carregando = false;
    }, 1000);
  }
}