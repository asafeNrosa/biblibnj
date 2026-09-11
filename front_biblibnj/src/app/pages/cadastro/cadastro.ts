import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './cadastro.html',
  styleUrl: './cadastro.css'
})
export class CadastroComponent {
  cadastroForm: FormGroup;
  exibirSenha: boolean = false;
  mensagemErro: string = '';
  carregando: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.cadastroForm = this.fb.group({
      nome: ['', [Validators.required, Validators.maxLength(150)]],
      email: ['', [Validators.required, Validators.email]],
      senha: ['', [Validators.required, Validators.minLength(6)]],
      telefone: ['', [Validators.required]],
      rua: ['', [Validators.required]],
      numero: ['', [Validators.required]],
      cidade: ['', [Validators.required]],
      cep: ['', [Validators.required]]
    });
  }

  get f() {
    return this.cadastroForm.controls;
  }

  toggleMostrarSenha(): void {
    this.exibirSenha = !this.exibirSenha;
  }

  onSubmit(): void {
    this.mensagemErro = '';

    if (this.cadastroForm.invalid) {
      this.cadastroForm.markAllAsTouched();
      return;
    }

    this.carregando = true;

    this.authService.cadastrar(this.cadastroForm.value).subscribe({
      next: () => {
        this.carregando = false;
        this.router.navigate(['/livros']);
      },
      error: (err) => {
        this.carregando = false;
        this.mensagemErro = err.error?.mensagem || 'Não foi possível concluir o cadastro. Tente novamente.';
      }
    });
  }
}
