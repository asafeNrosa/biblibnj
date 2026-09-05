import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GerenciarEmprestimos } from './gerenciar-emprestimos';

describe('GerenciarEmprestimos', () => {
  let component: GerenciarEmprestimos;
  let fixture: ComponentFixture<GerenciarEmprestimos>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GerenciarEmprestimos],
    }).compileComponents();

    fixture = TestBed.createComponent(GerenciarEmprestimos);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
