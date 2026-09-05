import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-livros-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './livros-form.html',
  styleUrl: './livros-form.css'
})
export class LivrosFormComponent implements OnInit {
  livroForm!: FormGroup;
  isEdicao: boolean = false;
  livroId: number | null = null;
  carregando: boolean = false;

  categorias: string[] = [
    'Fantasia',
    'Tecnologia',
    'Ficção Científica',
    'Distopia',
    'Clássico',
    'História',
    'Biografia',
    'Outros'
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.inicializarFormulario();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEdicao = true;
      this.livroId = Number(idParam);
      this.carregarDadosLivro(this.livroId);
    }
  }

  private inicializarFormulario(): void {
    this.livroForm = this.fb.group({
      titulo: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
      autor: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      isbn: ['', [Validators.required, Validators.pattern(/^(?:\d{9}[\dX]|\d{13})$/)]],
      categoria: ['', [Validators.required]],
      quantidadeTotal: [1, [Validators.required, Validators.min(1)]],
      quantidadeDisponivel: [1, [Validators.required, Validators.min(0)]]
    }, { validators: this.validarEstoqueDisponivel });
  }

  private validarEstoqueDisponivel(control: AbstractControl): ValidationErrors | null {
    const total = control.get('quantidadeTotal')?.value;
    const disponivel = control.get('quantidadeDisponivel')?.value;

    if (disponivel !== null && total !== null && disponivel > total) {
      return { estoqueInvalido: true };
    }
    return null;
  }

  private carregarDadosLivro(id: number): void {
    this.carregando = true;

    setTimeout(() => {
      const livrosMock = [
        { id: 1, titulo: 'O Hobbit', autor: 'J.R.R. Tolkien', isbn: '9788595084742', categoria: 'Fantasia', quantidadeTotal: 5, quantidadeDisponivel: 3 },
        { id: 2, titulo: 'Clean Code', autor: 'Robert C. Martin', isbn: '9788576082675', categoria: 'Tecnologia', quantidadeTotal: 2, quantidadeDisponivel: 1 }
      ];

      const livroEncontrado = livrosMock.find(l => l.id === id);

      if (livroEncontrado) {
        this.livroForm.patchValue(livroEncontrado);
      } else {
        alert('Livro não encontrado!');
        this.router.navigate(['/livros']);
      }

      this.carregando = false;
    }, 400);
  }

  get f() {
    return this.livroForm.controls;
  }

  onSubmit(): void {
    if (this.livroForm.invalid) {
      this.livroForm.markAllAsTouched();
      return;
    }

    this.carregando = true;
    const dadosLivro = this.livroForm.value;

    setTimeout(() => {
      if (this.isEdicao) {
        alert(`Livro "${dadosLivro.titulo}" atualizado com sucesso!`);
      } else {
        alert(`Livro "${dadosLivro.titulo}" cadastrado com sucesso!`);
      }

      this.carregando = false;
      this.router.navigate(['/livros']);
    }, 600);
  }
}