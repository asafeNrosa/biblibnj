import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService, RecuperarSenhaResponse } from '../../services/auth.service';

@Component({
  selector: 'app-esqueci-senha',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './esqueci-senha.html',
  styleUrl: './esqueci-senha.css'
})
export class EsqueciSenhaComponent {
  form: FormGroup;
  mensagemErro: string = '';
  carregando: boolean = false;
  resultado: RecuperarSenhaResponse | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {
    this.form = this.fb.group({
      emailOuTelefone: ['', [Validators.required]]
    });
  }

  get emailOuTelefone() {
    return this.form.get('emailOuTelefone');
  }

  onSubmit(): void {
    this.mensagemErro = '';
    this.resultado = null;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando = true;

    this.authService.recuperarSenha(this.form.value.emailOuTelefone).subscribe({
      next: (res) => {
        this.carregando = false;
        this.resultado = res;
      },
      error: (err) => {
        this.carregando = false;
        this.mensagemErro = err.error?.mensagem || 'Não foi possível processar a solicitação.';
      }
    });
  }
}
