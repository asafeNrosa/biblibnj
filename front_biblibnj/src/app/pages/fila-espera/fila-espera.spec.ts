import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilaEspera } from './fila-espera';

describe('FilaEspera', () => {
  let component: FilaEspera;
  let fixture: ComponentFixture<FilaEspera>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FilaEspera],
    }).compileComponents();

    fixture = TestBed.createComponent(FilaEspera);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
