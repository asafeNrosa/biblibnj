import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LivrosForm } from './livros-form';

describe('LivrosForm', () => {
  let component: LivrosForm;
  let fixture: ComponentFixture<LivrosForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LivrosForm],
    }).compileComponents();

    fixture = TestBed.createComponent(LivrosForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
