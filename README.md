# `Domain-Driven Design, Clean Architecture e Hexagonal Architecture: um estudo de caso sobre refatoração de arquiteturas em camadas.`

# `ETAPA 2/2:`

# `Projeto refatorado`

# 🧭 HelpDesk API — Sistema de Gerenciamento de Chamados

## 📘 Visão Geral

O **HelpDesk** é um sistema completo de **gerenciamento de tickets de suporte técnico**, projetado em **.NET** com **Entity Framework Core MySql**, **Swagger** e **integração AWS S3** para armazenamento de anexos.  
Inclui autenticação simplificada via header `userId`, controle de acesso por papéis (Requester, Agent, Manager), **notificações automáticas por e-mail** e **monitoramento de SLA**.

Além de atender aos requisitos funcionais do domínio de suporte, o projeto foi refatorado para aplicar princípios de **Domain-Driven Design (DDD)**, **Clean Architecture** e **Hexagonal Architecture**, com o objetivo de reduzir acoplamento, melhorar a clareza do modelo de negócio e facilitar evolução, testes e manutenção.

---

## 🧩 Estrutura Geral do Projeto

```text
HelpDesk---Patterns/
└── HelpDesk/
    ├── src/
    │   ├── HelpDesk.Api/
    │   │   ├── Contracts/
    │   │   │   ├── Attachments/
    │   │   │   ├── Collaboration/
    │   │   │   ├── IdentityAccess/
    │   │   │   ├── Operations/
    │   │   │   ├── ServiceCatalog/
    │   │   │   ├── Shared/
    │   │   │   └── Ticketing/
    │   │   ├── Controllers/
    │   │   │   ├── Attachments/
    │   │   │   ├── Collaboration/
    │   │   │   ├── IdentityAccess/
    │   │   │   ├── ServiceCatalog/
    │   │   │   └── Ticketing/
    │   │   ├── DependencyInjection/
    │   │   ├── Mapping/
    │   │   │   ├── Attachments/
    │   │   │   ├── Collaboration/
    │   │   │   ├── IdentityAccess/
    │   │   │   ├── ServiceCatalog/
    │   │   │   ├── Shared/
    │   │   │   └── Ticketing/
    │   │   └── Properties/
    │   │
    │   ├── HelpDesk.Application/
    │   │   ├── Attachments/
    │   │   │   ├── DTOs/
    │   │   │   ├── Internal/
    │   │   │   ├── Ports/
    │   │   │   └── UseCases/
    │   │   │       ├── DeleteAttachment/
    │   │   │       ├── GetAttachmentById/
    │   │   │       ├── ListAttachments/
    │   │   │       └── UploadAttachment/
    │   │   ├── Collaboration/
    │   │   │   ├── DTOs/
    │   │   │   ├── Internal/
    │   │   │   ├── Ports/
    │   │   │   └── UseCases/
    │   │   ├── IdentityAccess/
    │   │   │   ├── DTOs/
    │   │   │   ├── EventHandlers/
    │   │   │   ├── Internal/
    │   │   │   ├── Ports/
    │   │   │   └── UseCases/
    │   │   │       ├── CreateUser/
    │   │   │       ├── DeleteUser/
    │   │   │       ├── GetUserById/
    │   │   │       ├── ListUsers/
    │   │   │       └── PatchUser/
    │   │   ├── Operations/
    │   │   │   ├── DTOs/
    │   │   │   ├── EventHandlers/
    │   │   │   └── Ports/
    │   │   ├── ServiceCatalog/
    │   │   │   ├── DTOs/
    │   │   │   ├── EventHandlers/
    │   │   │   ├── Ports/
    │   │   │   └── UseCases/
    │   │   │       ├── CreateCategory/
    │   │   │       ├── DeleteCategory/
    │   │   │       ├── GetCategoryById/
    │   │   │       └── ListCategories/
    │   │   ├── Shared/
    │   │   │   ├── Abstractions/
    │   │   │   ├── Authorization/
    │   │   │   ├── DTOs/
    │   │   │   └── Errors/
    │   │   └── Ticketing/
    │   │       ├── DTOs/
    │   │       ├── Internal/
    │   │       ├── Ports/
    │   │       ├── Services/
    │   │       └── UseCases/
    │   │           ├── AssignTicket/
    │   │           ├── CancelTicket/
    │   │           ├── ChangeRequester/
    │   │           ├── ChangeStatus/
    │   │           ├── CreateTicket/
    │   │           ├── GetTicketById/
    │   │           ├── ListTickets/
    │   │           ├── ReopenTicket/
    │   │           └── UpdateTicket/
    │   │
    │   ├── HelpDesk.Domain/
    │   │   ├── Attachments/
    │   │   │   ├── Aggregates/
    │   │   │   ├── Events/
    │   │   │   ├── Rules/
    │   │   │   └── ValueObjects/
    │   │   ├── Collaboration/
    │   │   │   ├── Aggregates/
    │   │   │   ├── Enums/
    │   │   │   ├── Events/
    │   │   │   └── ValueObjects/
    │   │   ├── IdentityAccess/
    │   │   │   ├── Aggregates/
    │   │   │   ├── Events/
    │   │   │   ├── Rules/
    │   │   │   └── ValueObjects/
    │   │   ├── ServiceCatalog/
    │   │   │   ├── Aggregates/
    │   │   │   ├── Events/
    │   │   │   └── ValueObjects/
    │   │   ├── SharedKernel/
    │   │   │   ├── Exceptions/
    │   │   │   └── Primitives/
    │   │   └── Ticketing/
    │   │       ├── Aggregates/
    │   │       ├── Enums/
    │   │       ├── Events/
    │   │       └── ValueObjects/
    │   │
    │   └── HelpDesk.Infrastructure/
    │       ├── Attachments/
    │       │   ├── DependencyInjection/
    │       │   ├── Models/
    │       │   ├── Queries/
    │       │   ├── Repositories/
    │       │   └── Storage/
    │       ├── Collaboration/
    │       │   ├── DependencyInjection/
    │       │   ├── Models/
    │       │   ├── Queries/
    │       │   └── Repositories/
    │       ├── IdentityAccess/
    │       │   ├── DependencyInjection/
    │       │   ├── Models/
    │       │   ├── Queries/
    │       │   └── Repositories/
    │       ├── Operations/
    │       │   ├── DependencyInjection/
    │       │   ├── Models/
    │       │   ├── Notifications/
    │       │   │   ├── Email/
    │       │   │   └── Templates/
    │       │   └── Queries/
    │       ├── Options/
    │       ├── Persistence/
    │       │   ├── Configurations/
    │       │   │   ├── Attachments/
    │       │   │   ├── Collaboration/
    │       │   │   ├── IdentityAccess/
    │       │   │   ├── ServiceCatalog/
    │       │   │   ├── Ticketing/
    │       │   │   └── operations/
    │       │   └── Migrations/
    │       ├── ServiceCatalog/
    │       │   ├── DependencyInjection/
    │       │   ├── Models/
    │       │   ├── Queries/
    │       │   └── Repositories/
    │       ├── Shared/
    │       │   ├── Clock/
    │       │   ├── DependencyInjection/
    │       │   └── DomainEvents/
    │       └── Ticketing/
    │           ├── DependencyInjection/
    │           ├── HostedServices/
    │           ├── Models/
    │           ├── Queries/
    │           └── Repositories/
    │
    └── tests/
        ├── HelpDesk.IntegrationTests/
        │   ├── Attachments/
        │   ├── Collaboration/
        │   ├── Fakes/
        │   ├── Fixtures/
        │   ├── IdentityAccess/
        │   ├── ServiceCatalog/
        │   ├── Ticketing/
        │   └── Utilities/
        │       └── Http/
        │
        └── HelpDesk.UnitTests/
            ├── Attachments/
            ├── Collaboration/
            ├── IdentityAccess/
            ├── ServiceCatalog/
            ├── Shared/
            │   └── Fakes/
            └── Ticketing/
```

---

## 🏛️ Função de Cada Camada

### `HelpDesk.Domain`

É o **centro do sistema** e concentra as regras mais importantes do negócio.  
Nessa camada ficam os **agregados**, **value objects**, **eventos de domínio**, **regras invariantes** e conceitos essenciais como Ticket, User, Category, Comment e Attachment.

**Responsabilidade principal:** representar o negócio de forma pura, sem depender de banco de dados, frameworks, serviços externos ou detalhes de infraestrutura.

**Benefício arquitetural:** preserva o conhecimento de negócio no núcleo da aplicação, evitando que regras críticas fiquem espalhadas em controllers, ORM ou integrações.

### `HelpDesk.Application`

Orquestra os **casos de uso** do sistema.  
Essa camada coordena o fluxo das operações, aplica regras de autorização, chama portas, manipula DTOs e aciona o domínio para executar comportamentos.

**Responsabilidade principal:** transformar intenções do usuário em execução de casos de uso, sem conter detalhes técnicos de persistência ou integração.

**Benefício arquitetural:** mantém a lógica de aplicação organizada por operação de negócio, facilitando testes, leitura e evolução funcional.

### `HelpDesk.Infrastructure`

Implementa os detalhes técnicos necessários para a aplicação funcionar, como:

- persistência com ORM
- repositórios
- commands/queries
- gerenciamento de arquivos/anexos
- envio de e-mail
- hosted services
- configurações de banco
- adaptação a serviços externos

**Responsabilidade principal:** materializar as portas definidas pela aplicação, isolando tecnologia do núcleo de negócio.

**Benefício arquitetural:** permite trocar tecnologias com menor impacto, como banco de dados, storage ou serviço de e-mail, sem reescrever o domínio.

### `HelpDesk.Api`

É a camada de **entrada HTTP** do sistema.  
Contém controllers, contratos de request/response, mapeamentos e composição das dependências.

**Responsabilidade principal:** expor os casos de uso ao mundo externo por meio da API REST, traduzindo requisições e respostas sem concentrar regra de negócio.

**Benefício arquitetural:** evita controllers anêmicos com lógica misturada, deixando a API focada em transporte, validação de entrada e documentação.

### `HelpDesk.UnitTests`

Contém testes unitários das regras de domínio e dos casos de uso, isolando dependências por meio de fakes e mocks.

**Responsabilidade principal:** validar comportamentos do sistema de forma rápida e determinística.

### `HelpDesk.IntegrationTests`

Contém testes de integração que verificam a interação real entre camadas, endpoints, persistência e infraestrutura.

**Responsabilidade principal:** garantir que os fluxos principais da aplicação funcionem corretamente em cenários próximos do ambiente real.

---

## 🔐 Autenticação e Papéis

A API usa **autenticação via cabeçalho HTTP**:

```http
userId: 1
```

> Esse identificador é validado no banco de dados em todas as rotas protegidas.

**Papéis suportados:**

- 🧑‍💼 `Manager`: pode criar, editar e excluir usuários, categorias e tickets.
- 👩‍💻 `Agent`: pode atuar em tickets atribuídos e alterar status.
- 🙋‍♂️ `Requester`: cria e gerencia seus próprios tickets.

---

## 🎫 TicketsController (`/api/tickets`)

Gerencia todo o ciclo de vida de um ticket, desde a criação até o fechamento.

### 🔹 Regras Gerais

- **Criação:** apenas `Requester` e `Manager`.
- **Edição:** somente o dono (Requester) ou Manager.
- **Cancelamento e Reabertura:** requer motivo obrigatório (`reason`).
- **SLA:** automático conforme prioridade (Crítica = 8h, Alta = 24h, Média = 48h, Baixa = 72h).
- **Histórico:** cada ação gera uma entrada em `TicketActions`.

### 🔸 Endpoints Principais

| Método                             | Descrição                                                                  |
| ---------------------------------- | -------------------------------------------------------------------------- |
| `GET /api/tickets`                 | Lista tickets com filtros (status, prioridade, datas, usuários, SLA, etc). |
| `GET /api/tickets/{id}`            | Retorna detalhes completos (comentários, anexos, ações).                   |
| `POST /api/tickets`                | Cria novo ticket.                                                          |
| `PATCH /api/tickets/{id}`          | Atualiza título, descrição, prioridade ou categoria.                       |
| `POST /api/tickets/{id}/assign`    | Atribui o ticket a um `Agent`.                                             |
| `POST /api/tickets/{id}/requester` | Altera o `Requester` do ticket.                                            |
| `POST /api/tickets/{id}/status`    | Atualiza o status do ticket.                                               |
| `POST /api/tickets/{id}/reopen`    | Reabre ticket. Requer `reason`.                                            |
| `POST /api/tickets/{id}/cancel`    | Cancela ticket. Requer `reason`.                                           |

### ⚙️ Status Possíveis

```text
Novo → Em Análise → Em Andamento → Resolvido → Fechado / Cancelado
```

### 🧾 TicketActions

Cada alteração relevante gera um log automático:

- mudança de status
- mudança de prioridade
- mudança de categoria
- mudança de responsável
- comentários
- cancelamentos
- reaberturas

---

## 💬 CommentsController (`/api/tickets/{ticketId}/comments`)

Permite incluir comunicação entre Requester, Agent e Manager dentro de um ticket.

### 🔹 Regras

- Comentários **internos** só podem ser criados por participantes do ticket.
- **Visibilidades:** `Público` ou `Interno`.
- **Limite:** 4000 caracteres.

### 🔸 Endpoints

| Método         | Descrição                      |
| -------------- | ------------------------------ |
| `POST`         | Adiciona um novo comentário.   |
| `GET`          | Lista comentários do ticket.   |
| `GET /{id}`    | Retorna comentário específico. |
| `PUT /{id}`    | Atualiza mensagem.             |
| `DELETE /{id}` | Exclui comentário.             |

---

## 📎 AttachmentsController (`/api/tickets/{ticketId}/attachments`)

Gerencia anexos de um ticket com armazenamento no Amazon S3.

### 🔹 Regras

- Apenas tickets ativos aceitam operações de anexo.
- Cabeçalho obrigatório: `userId`.
- Tamanho máximo por arquivo: **10 MB**.
- Extensões bloqueadas: `.exe`, `.bat`, `.sh`.
- Chave de armazenamento: `tickets/{ticketId}/{fileName}`.

### 🔸 Endpoints

| Método         | Descrição               |
| -------------- | ----------------------- |
| `POST`         | Upload de arquivo.      |
| `GET`          | Lista anexos do ticket. |
| `GET /{id}`    | Detalhe de um anexo.    |
| `DELETE /{id}` | Exclui anexo.           |

---

## 🗂️ CategoriesController (`/api/categories`)

Gerencia categorias hierárquicas de tickets.

### 🔹 Regras

- Somente `Manager` pode criar ou excluir.
- Subcategoria só é permitida se o pai não tiver outro pai.
- Nomes devem ser únicos.

### 🔸 Endpoints

| Método         | Descrição                       |
| -------------- | ------------------------------- |
| `POST`         | Cria categoria ou subcategoria. |
| `GET`          | Lista categorias.               |
| `GET /{id}`    | Retorna categoria pelo ID.      |
| `DELETE /{id}` | Exclui categoria.               |

---

## 👥 UsersController (`/api/users`)

Gerencia usuários, papéis e vínculos com o sistema.

### 🔹 Regras

- Somente `Manager` pode criar, editar e excluir.
- `Email` deve ser único e válido.
- `Role` deve ser `Requester`, `Agent` ou `Manager`.

### 🔸 Endpoints

| Método         | Descrição                    |
| -------------- | ---------------------------- |
| `POST`         | Cria usuário.                |
| `GET`          | Lista usuários.              |
| `GET /{id}`    | Retorna detalhes do usuário. |
| `PATCH /{id}`  | Atualiza dados do usuário.   |
| `DELETE /{id}` | Remove usuário.              |

---

## 📦 Serviços Auxiliares

### 🕐 SlaBackgroundService

Executa verificações periódicas sobre o consumo do SLA, gerando alertas quando um ticket se aproxima do vencimento.

### ✉️ NotificationService / EmailService

Envia notificações automáticas por e-mail quando eventos relevantes ocorrem no ciclo de vida do ticket.

### ☁️ FileStorageService

Gerencia upload, persistência e remoção de anexos no Amazon S3.

---

## 🧠 SLA e Prioridades

| Prioridade | Tempo de SLA |
| ---------- | ------------ |
| Crítica    | 8h           |
| Alta       | 24h          |
| Média      | 48h          |
| Baixa      | 72h          |

O SLA é iniciado no momento configurado pela política do sistema e pode ser recalculado em mudanças relevantes de prioridade ou estado.

---

## 🧾 Geração de Documentação Swagger

Swagger configurado automaticamente em desenvolvimento:

```text
https://localhost:44314/swagger/index.html
```

Para exportar o YAML atualizado:

```bash
/api/SwaggerExport/yaml
```

---

## 🧰 Tecnologias Utilizadas

### ⚙️ Backend

- **.NET 8 / C#**
- **Entity Framework Core (Pomelo MySQL Provider)**
- **Swagger / Swashbuckle.AspNetCore**
- **Amazon S3 (AWS SDK)**
- **MailKit / MimeKit**
- **Hosted Services / Background Tasks**

### 🧪 Testes Automatizados

- **xUnit**
- **FluentAssertions**
- **Moq**
- **EF Core InMemory Provider**
- **EF Core SQLite (in-memory)**
- **Microsoft.AspNetCore.Mvc.Testing**

### 🔄 Variações Tecnológicas

O projeto também possui uma branch dedicada a experimentos e substituições de tecnologia.  
Para explorar essas alternativas, o leitor pode trocar para a branch:

```bash
git checkout changes/technology
```

Nessa branch, a arquitetura foi preparada para permitir substituições com impacto reduzido, reforçando os benefícios de **Clean Architecture** e **Hexagonal Architecture**, especialmente pela separação entre domínio, portas e adaptadores.

#### ✉️ Opções para envio de e-mail

Na branch `changes/technology`, existem diferentes implementações para o envio de e-mails, permitindo comparar bibliotecas e estratégias de integração:

- `AsposeEmailSender.cs`
- `FluentEmailSenderAdapter.cs`
- `LimilabsEmailSender.cs`
- `SystemNetMailEmailSender.cs`

#### 📁 Opções para gerenciamento de arquivos

Na pasta `Attachments/Storage`, também há alternativas para o gerenciamento e armazenamento de arquivos:

- `CloudinaryFileStoragePort.cs`
- `LocalFileStoragePort.cs`
- `SupabaseFileStoragePort.cs`
- `UploadCareFileStoragePort.cs`

#### 🗄️ Opções para troca de SGBD

A branch também apresenta uma estratégia de abstração para troca de banco de dados, com suporte a provedores distintos por meio de adaptadores:

- `MySqlDatabaseProvider.cs`
- `PostgreSqlDatabaseProvider.cs`
- `IDatabaseProviderAdapter.cs`

Essas variações mostram como a aplicação pode evoluir ou trocar tecnologias externas com menor impacto no núcleo do negócio, preservando as regras de domínio e reduzindo acoplamento com infraestrutura.

---

## 🧠 Resumo de DDD Aplicado no Projeto

A refatoração do HelpDesk foi guiada por conceitos centrais de **Domain-Driven Design**, com ênfase em **Event Storming**, **Linguagem Ubíqua**, **subdomínios** e **Bounded Contexts**.

O Event Storming ajudou a mapear o fluxo real do negócio a partir de comandos, eventos, agregados, políticas e integrações externas. Isso permitiu identificar com clareza o ciclo de vida do chamado, a gestão de categorias, comentários, anexos, SLA e notificações.

A modelagem passou a usar termos consistentes com o negócio, como **Ticket**, **Prioridade**, **Atribuição**, **Comentário Interno**, **Anexo**, **SLA**, **Notificação** e **Categoria**, aproximando código e linguagem do domínio.

Estratégicamente, o sistema foi dividido em subdomínios:

- **Atendimento de Chamados**: **Core Domain**
- **Catálogo de Serviços**: **Supporting Domain**
- **Comunicação**: **Supporting Domain**
- **Anexos**: **Supporting Domain**
- **Operações (SLA e Notificações)**: **Supporting Domain**
- **Identidade e Acesso**: **Generic Domain**

Com isso, foram definidos **Bounded Contexts** independentes para:

- **Ticketing**
- **Identity Access**
- **Service Catalog**
- **Collaboration**
- **Attachments**
- **Operations**

Esses contextos se relacionam por padrões como **Conformist**, **Customer-Supplier**, **Upstream/Downstream**, **Event-Driven** e **ACL (Anti-Corruption Layer)**, reduzindo acoplamento, protegendo o modelo interno e facilitando evolução.

---

## 💡 Boas Práticas Arquiteturais Implementadas

- **Separação explícita entre domínio, aplicação, infraestrutura e API**, reduzindo o acoplamento entre regra de negócio e detalhes técnicos.
- **Casos de uso organizados por intenção de negócio**, favorecendo leitura, manutenção e evolução incremental.
- **Domínio isolado de frameworks e serviços externos**, preservando pureza do modelo e facilitando testes unitários.
- **Uso de portas e adaptadores**, permitindo substituir mecanismos de persistência, e-mail e armazenamento sem afetar o núcleo da aplicação.
- **Bounded Contexts explícitos**, reduzindo colisões semânticas entre conceitos como Ticket, Categoria, Usuário, SLA e Notificação.
- **Eventos de domínio para desacoplamento entre ações centrais e reações secundárias**, evitando dependências diretas entre módulos.
- **Integrações externas protegidas por ACLs**, impedindo que contratos de S3, SMTP ou outras tecnologias contaminem o modelo interno.
- **Regras de negócio concentradas em agregados e objetos de valor**, reforçando invariantes e evitando lógica espalhada.
- **Organização por subdomínios**, facilitando entendimento estrutural do sistema e priorização do esforço de modelagem.
- **Maior testabilidade das regras centrais**, já que o comportamento de negócio pode ser validado sem exigir infraestrutura real.
- **Maior flexibilidade para evolução tecnológica**, permitindo adaptar banco, storage e mecanismos de notificação com menor impacto.
- **Melhor alinhamento entre código e negócio**, por meio de Linguagem Ubíqua e modelagem orientada ao processo real do atendimento.

---

## ✅ Benefícios Obtidos com a Refatoração

A adoção conjunta de **DDD**, **Clean Architecture** e **Hexagonal Architecture** trouxe ganhos importantes para o projeto:

- maior clareza sobre o que pertence ao negócio e o que pertence à tecnologia
- redução da dependência entre camadas
- melhor isolamento do núcleo do sistema
- aumento da coesão interna dos módulos
- facilidade para escrever testes unitários e de integração
- menor impacto de mudanças externas
- melhor comunicação entre implementação e linguagem do domínio
- base mais sustentável para crescimento e manutenção do sistema
