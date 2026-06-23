# OrderService

Este repositório contém a API do OrderService, construída com .NET 10 e PostgreSQL. A aplicação segue os princípios de Clean Architecture, separando responsabilidades em camadas de Domain, Application, Infrastructure e Api.

O banco de dados utilizado é o PostgreSQL, que é configurado e inicializado automaticamente com dados iniciais (seed de produtos) durante a execução do serviço.

## Tecnologias e Dependências

A aplicação utiliza as seguintes tecnologias e pacotes:

- **.NET 10.0** (SDK e Runtime)
- **PostgreSQL** (via Entity Framework Core e Npgsql)
- **Autenticação JWT** para proteção dos endpoints
- **Scalar / OpenAPI** para documentação da API

---

# Como rodar o projeto

Você pode executar o projeto de duas formas: utilizando o Docker (recomendado para facilidade) ou rodando localmente na sua máquina.

## 1. Rodando via Docker (Recomendado)

O projeto possui um arquivo `docker-compose.yml` que já configura tanto a API quanto o banco de dados PostgreSQL.

### Pré-requisitos

- Docker
- Docker Compose

### Passos

1. Navegue até o diretório raiz do projeto (onde o arquivo `docker-compose.yml` está localizado).

2. Execute o comando:

```bash
docker-compose up --build -d
```

3. O Docker fará o download da imagem do PostgreSQL, construirá a imagem da API .NET e iniciará os dois containers.

4. O banco de dados será criado automaticamente e as migrations serão executadas durante a inicialização da API.

5. A API estará disponível em:

```text
http://localhost:8080
```

6. A documentação estará disponível em:

```text
http://localhost:8080/scalar/v1
```

### Parar os containers

```bash
docker-compose down
```

---

## 2. Rodando Localmente (Sem Docker)

### Pré-requisitos

- .NET 10 SDK
- PostgreSQL instalado localmente

### Configurar o banco de dados

O projeto utiliza por padrão:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=OrderService;Username=postgres;Password=postgres"
  }
}
```

Caso seu ambiente utilize credenciais diferentes, altere o arquivo:

```text
appsettings.json
```

ou

```text
appsettings.Development.json
```

### Restaurar dependências

```bash
dotnet restore
```

### Executar a aplicação

```bash
dotnet run
```

A API estará disponível em:

```text
http://localhost:5235
```

ou

```text
https://localhost:7117
```

### Acessar a documentação

```text
http://localhost:5235/scalar/v1
```

---

# Como testar a API

A API possui endpoints de autenticação e gerenciamento de pedidos.

Um arquivo:

```text
OrderService.http
```

está disponível para facilitar os testes em IDEs compatíveis.

Você também pode utilizar:

- Scalar
- Postman
- Insomnia

---

# Fluxo Básico de Teste

## 1. Registrar um Usuário

### Endpoint

```http
POST /auth/register
```

### Body

```json
{
  "username": "admin",
  "password": "admin"
}
```

---

## 2. Fazer Login

### Endpoint

```http
POST /auth/token
```

### Body

```json
{
  "username": "admin",
  "password": "admin"
}
```

### Resposta

```json
{
  "token": "jwt-token"
}
```

Copie o token retornado.

---

## 3. Utilizar o Token

Todos os endpoints protegidos exigem o header:

```http
Authorization: Bearer {token}
```

---

# Documentação dos Endpoints de Pedidos

Todos os endpoints abaixo exigem autenticação JWT.

---

## Criar Pedido

Cria um novo pedido.

### Endpoint

```http
POST /orders
```

### Request Body

```json
{
  "customerId": 1,
  "currency": "BRL",
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 4,
      "quantity": 4
    }
  ]
}
```

### Campos

| Campo | Tipo | Obrigatório | Descrição |
|---------|---------|---------|---------|
| customerId | int | Sim | Identificador do cliente |
| currency | string | Sim | Código da moeda ISO-4217 |
| items | array | Sim | Lista de itens do pedido |
| items[].productId | int | Sim | Identificador do produto |
| items[].quantity | int | Sim | Quantidade desejada |

### Resposta de Sucesso

**201 Created**

```json
{
  "id": "eb47d9b4-59d3-4e52-9e9b-c94df71d15d7"
}
```

### Possíveis Erros

| Status | Motivo |
|----------|----------|
| 400 | Pedido sem itens |
| 400 | Quantidade inválida |
| 400 | Preço inválido |
| 404 | Produto não encontrado |
| 409 | Estoque insuficiente |

---

## Obter Pedido por Id

Retorna os detalhes completos de um pedido.

### Endpoint

```http
GET /orders/{id}
```

### Path Parameters

| Nome | Tipo | Descrição |
|--------|--------|--------|
| id | Guid | Identificador do pedido |

### Resposta de Sucesso

**200 OK**

```json
{
  "id": "eb47d9b4-59d3-4e52-9e9b-c94df71d15d7",
  "customerId": 1,
  "status": "Placed",
  "currency": "BRL",
  "total": 100,
  "createdAt": "2025-06-20T10:00:00Z",
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 50
    }
  ]
}
```

### Possíveis Erros

| Status | Motivo |
|----------|----------|
| 404 | Pedido não encontrado |

---

## Listar Pedidos

Retorna pedidos de forma paginada.

### Endpoint

```http
GET /orders
```

### Query Parameters

| Nome | Tipo | Obrigatório | Descrição |
|--------|--------|--------|--------|
| page | int | Não | Página desejada |
| pageSize | int | Não | Quantidade de registros por página |
| customerId | int | Não | Identificador do cliente |
| status | int | Não | Código de status |
| from | datetime | Não | Data inicial do filtro |
| to | datetime | Não | Data final do filtro |

### Exemplo

```http
GET /orders?page=1&pageSize=10&customerId=1&status=1&from=2025-01-01&to=2025-12-31
```

### Resposta de Sucesso

**200 OK**

```json
{
  "data": [
    {
      "id": "eb47d9b4-59d3-4e52-9e9b-c94df71d15d7",
      "status": "Placed",
      "total": 100,
      "createdAt": "2025-06-20T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

---

## Confirmar Pedido

Confirma um pedido e realiza a baixa de estoque dos produtos.

### Endpoint

```http
POST /orders/{id}/confirm
```

### Path Parameters

| Nome | Tipo | Descrição |
|--------|--------|--------|
| id | Guid | Identificador do pedido |

### Regras de Negócio

- Apenas pedidos com status **Placed** podem ser confirmados.
- A confirmação reduz o estoque dos produtos.
- A operação é transacional.
- Caso qualquer produto não possua estoque suficiente, nenhuma alteração é persistida.

### Resposta de Sucesso

**204 No Content**

Sem conteúdo.

### Possíveis Erros

| Status | Motivo |
|----------|----------|
| 404 | Pedido não encontrado |
| 404 | Produto não encontrado |
| 409 | Estoque insuficiente |
| 409 | Status inválido para confirmação |

---

## Cancelar Pedido

Cancela um pedido.

### Endpoint

```http
POST /orders/{id}/cancel
```

### Path Parameters

| Nome | Tipo | Descrição |
|--------|--------|--------|
| id | Guid | Identificador do pedido |

### Regras de Negócio

- Pedidos em status **Placed** podem ser cancelados.
- Pedidos em status **Confirmed** podem ser cancelados.
- Ao cancelar um pedido confirmado, o estoque é devolvido aos produtos.
- A operação é executada em transação.

### Resposta de Sucesso

**204 No Content**

Sem conteúdo.

### Possíveis Erros

| Status | Motivo |
|----------|----------|
| 404 | Pedido não encontrado |
| 404 | Produto não encontrado |
| 409 | Status inválido para cancelamento |

---

# Fluxo de Estados do Pedido

```text
Draft
  ↓
Placed
  ↓
Confirmed
  ↓
Canceled
```

## Estados

| Status | Descrição |
|----------|----------|
| Draft | Pedido em construção |
| Placed | Pedido criado e aguardando confirmação |
| Confirmed | Pedido confirmado e estoque debitado |
| Canceled | Pedido cancelado |

---

# Produtos Disponíveis para Teste

Durante a inicialização da aplicação, são criados automaticamente produtos de exemplo.

Os produtos possuem IDs:

```text
1 até 20
```

Esses produtos podem ser utilizados diretamente nos testes dos endpoints de pedidos.

---

# Executando os Testes Unitários

O projeto possui testes unitários utilizando:

- NUnit
- Moq
- FluentAssertions

### Executar todos os testes

Na raiz da solution:

```bash
dotnet test
```

Ou diretamente na pasta do projeto de testes:

```bash
dotnet test
```

### Cobertura dos testes

A suíte cobre:

- Serviços de aplicação
- Entidades de domínio
- Fluxos de criação de pedidos
- Confirmação de pedidos
- Cancelamento de pedidos
- Atualização de estoque
- Casos de erro
- Casos de rollback transacional
- Casos de borda (edge cases)

---