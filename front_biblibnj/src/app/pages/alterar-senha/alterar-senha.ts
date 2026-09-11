import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

function senhasIguaisValidator(group: AbstractControl): ValidationErrors | null {
  const novaSenha = group.get('novaSenha')?.value;
  const confirmarSenha = group.get('confirmarSenha')?.value;
  return novaSenha === confirmarSenha ? null : { senhasDiferentes: true };
}

@Component({
  selector: 'app-alterar-senha',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './alterar-senha.html',
  styleUrl: './alterar-senha.css'
})
export class AlterarSenhaComponent {
  form: FormGroup;
  mensagemErro: string = '';
  mensagemSucesso: string = '';
  carregando: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {
    this.form = this.fb.group({
      senhaAtual: ['', [Validators.required]],
      novaSenha: ['', [Validators.required, Validators.minLength(6)]],
      confirmarSenha: ['', [Validators.required]]
    }, { validators: senhasIguaisValidator });
  }

  get f() {
    return this.form.controls;
  }

  onSubmit(): void {
    this.mensagemErro = '';
    this.mensagemSucesso = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando = true;
    const { senhaAtual, novaSenha } = this.form.value;

    this.authService.alterarSenha(senhaAtual, novaSenha).subscribe({
      next: (res) => {
        this.carregando = false;
        this.mensagemSucesso = res.mensagem;
        this.form.reset();
      },
      error: (err) => {
        this.carregando = false;
        this.mensagemErro = err.error?.mensagem || 'Não foi possível alterar a senha.';
      }
    });
  }
}
