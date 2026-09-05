import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LivrosLista } from './livros-lista';

describe('LivrosLista', () => {
  let component: LivrosLista;
  let fixture: ComponentFixture<LivrosLista>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LivrosLista],
    }).compileComponents();

    fixture = TestBed.createComponent(LivrosLista);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
