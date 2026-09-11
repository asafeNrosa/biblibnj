# biblibnj

Sistema de gerenciamento de biblioteca desenvolvido como projeto do curso de **Análise e Desenvolvimento de Sistemas**. Permite que leitores consultem o acervo, solicitem empréstimos e entrem em filas de espera, enquanto administradores gerenciam o catálogo, aprovam/recusam empréstimos e controlam multas por atraso.

---

## Sobre o projeto

O biblibnj nasceu como um projeto full stack acadêmico com back-end em **C# / ASP.NET Core** e front-end em **Angular**, usando **SQL Server** como banco de dados. A proposta é digitalizar o controle de empréstimos de uma biblioteca, incluindo regras reais do dia a dia: limite de livros por usuário, prazo de devolução, renovações, multas por atraso e fila de espera para livros esgotados.

---

## Funcionalidades

### Para qualquer visitante
- Consultar o catálogo completo de livros (título, autor, ISBN, editora, disponibilidade)
- Buscar por título, autor ou ISBN
- Filtrar por disponíveis / esgotados

### Para leitores (usuários comuns)
- Criar conta (cadastro com nome, e-mail, senha, telefone e endereço)
- Login / Logout
- Recuperação de senha (fluxo simulado de envio por e-mail ou SMS)
- Alterar senha (com a conta logada)
- Solicitar empréstimo de um livro disponível
- Acompanhar seus empréstimos: status (pendente, em aberto, atrasado, devolvido), prazo de devolução, quantidade de renovações
- Renovar um empréstimo (até 2 vezes)
- Solicitar devolução
- Entrar na fila de espera de um livro esgotado e acompanhar sua posição
- Sair da fila de espera
- Tentar o empréstimo assim que chegar sua vez na fila **e** houver exemplar disponível

### Para administradores
- Cadastrar, editar e excluir livros do acervo
- Ajustar o estoque (quantidade total/disponível) de um livro
- **Aprovar ou recusar** solicitações de empréstimo pendentes
- Visualizar todos os empréstimos ativos (quem pegou, qual livro, datas, renovações)
- Registrar devolução de um livro em nome do usuário
- Visualizar usuários com multa pendente e liberá-los após o pagamento (presencial)
- *(Administradores não podem solicitar empréstimos nem entrar em filas de espera — esse perfil é só de gestão.)*

---

## Regras de negócio

- Um usuário só pode ter **1 empréstimo ativo por vez**
- Todo empréstimo passa por **aprovação do administrador** antes de ser liberado
- O exemplar já é **reservado no momento da solicitação** (não espera a aprovação para tirar do estoque)
- Prazo de empréstimo: **7 dias**, renovável em **até 2 vezes consecutivas**
- Empréstimo vencido é marcado automaticamente como **atrasado**
- Atraso gera multa de **R$ 1,00 por dia**, mesmo se o usuário renovar depois de já estar atrasado
- Usuário com multa pendente **não pode solicitar novos empréstimos** até quitar a multa (pagamento presencial) e ser liberado por um administrador

---

## Tecnologias utilizadas

### Back-end
| Tecnologia | Uso |
|---|---|
| **C# / .NET 10** | Linguagem e plataforma principal da API |
| **ASP.NET Core Web API** | Framework para os endpoints REST |
| **Entity Framework Core** | ORM para acesso ao banco de dados |
| **SQL Server** | Banco de dados relacional |
| **JWT (JSON Web Token)** | Autenticação e autorização baseada em token |
| **PBKDF2 (System.Security.Cryptography)** | Criptografia de senhas (hash com salt, nativo do .NET) |
| **Swagger / Swashbuckle** | Documentação e teste interativo da API |

### Front-end
| Tecnologia | Uso |
|---|---|
| **TypeScript** | Linguagem principal do front-end |
| **Angular 21** (standalone components + Signals) | Framework SPA |
| **Angular Reactive Forms** | Formulários com validação (login, cadastro, empréstimos etc.) |
| **RxJS** | Programação reativa para chamadas HTTP |
| **Tailwind CSS v4 + CSS aninhado** | Estilização |
| **Vite** (via Angular CLI) | Build e dev server |

### Arquitetura geral
- API REST stateless, consumida via `HttpClient` do Angular
- Autenticação via **Bearer Token (JWT)**, com interceptor HTTP no front que anexa o token automaticamente
- Perfis de acesso (`Comum` / `Admin`) controlados por *claims* no token e `[Authorize(Roles = "...")]` no back-end
- CORS configurado para liberar apenas a origem do front-end em desenvolvimento

---

## Estrutura do projeto

```
biblibnj/
├── biblibnj/                  # Back-end (ASP.NET Core Web API)
│   └── biblibnj/
│       ├── Controllers/       # AuthController, LivrosController, EmprestimosController, FilaEsperaController
│       ├── Entities/          # Usuario, Livro, Emprestimo, FilaEspera
│       ├── DTOs/               # Objetos de transferência de dados (request/response)
│       ├── Services/           # TokenService (JWT), PasswordHasher (criptografia)
│       ├── Context/             # BiblibnjDbContext (Entity Framework)
│       └── Program.cs           # Configuração da aplicação (DI, JWT, CORS, Swagger)
│
└── front_biblibnj/             # Front-end (Angular)
    └── src/app/
        ├── pages/               # Telas: login, cadastro, catálogo, empréstimos, fila de espera, admin...
        ├── components/          # Componentes reutilizáveis (navbar)
        ├── services/            # Comunicação com a API (auth, livros, empréstimos, fila de espera)
        └── interceptors/        # Interceptor que anexa o token JWT às requisições
```

---

## Como executar o projeto localmente

### Pré-requisitos
- [.NET SDK 10](https://dotnet.microsoft.com/)
- [Node.js](https://nodejs.org/) (18+) e npm
- [SQL Server](https://www.microsoft.com/sql-server) (Express ou superior) com uma instância local

### 1. Banco de dados
Crie um banco chamado `biblibnj_db` e ajuste a string de conexão em `appsettings.json` (back-end) se necessário:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=biblibnj_db;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
Rode as migrations do Entity Framework (ou aplique o schema conforme o script SQL do projeto) para criar as tabelas `Usuarios`, `Livros`, `Emprestimos` e `FilaEspera`.

### 2. Back-end (API)
```bash
cd biblibnj/biblibnj
dotnet restore
dotnet run
```
A API sobe em `https://localhost:7206` (verifique a porta exata em `Properties/launchSettings.json`). A documentação interativa fica em `https://localhost:7206/swagger`.

### 3. Front-end (Angular)
```bash
cd front_biblibnj
npm install
ng serve
```
A aplicação abre em `http://localhost:4200`.

### 4. Primeiro acesso
1. Acesse `http://localhost:4200/cadastro` e crie uma conta de leitor
2. Para testar como administrador, promova um usuário para o perfil `Admin` diretamente no banco (coluna `Perfil` da tabela `Usuarios`)

---

##  Como o projeto foi construído

O desenvolvimento seguiu um fluxo iterativo típico de projeto acadêmico full stack:

1. **Modelagem inicial**: definição das entidades (Usuário, Livro, Empréstimo, Fila de Espera) e das regras de negócio da biblioteca
2. **Back-end primeiro**: API REST em ASP.NET Core com autenticação JWT, CRUD de livros e empréstimos básicos
3. **Front-end em Angular**: telas de catálogo, login e empréstimos consumindo a API
4. **Ciclo de depuração**: correção de problemas de integração entre front e back (portas, CORS, nomes de campos divergentes entre C# e TypeScript, constraints de banco desalinhadas com o código)
5. **Evolução das regras de negócio**: implementação do fluxo de aprovação de empréstimos pelo administrador, sistema de renovação, cálculo de multas e bloqueio de usuários inadimplentes
6. **Experiência do usuário**: páginas de cadastro, recuperação e alteração de senha
7. **Segurança**: substituição do armazenamento de senha em texto puro por hash criptográfico (PBKDF2)

Ao longo do processo, foram tratados bugs reais de projeto (erros de digitação em nomes de campos, caminhos de import incorretos, condições de corrida entre reserva de estoque e aprovação, entre outros), simulando o ciclo de vida de manutenção de um sistema em desenvolvimento contínuo.

---

## Asafe Nogueira Rosa

Projeto desenvolvido para o curso de Análise e Desenvolvimento de Sistemas.
