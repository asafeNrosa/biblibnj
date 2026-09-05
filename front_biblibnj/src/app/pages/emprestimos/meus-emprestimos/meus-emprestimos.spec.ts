import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MeusEmprestimos } from './meus-emprestimos';

describe('MeusEmprestimos', () => {
  let component: MeusEmprestimos;
  let fixture: ComponentFixture<MeusEmprestimos>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MeusEmprestimos],
    }).compileComponents();

    fixture = TestBed.createComponent(MeusEmprestimos);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
