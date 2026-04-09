# 📦 DeepArchive-Bridge

**DeepArchive-Bridge** é uma API robusta e moderna desenvolvida em **.NET 8** que implementa um sistema inteligente de **armazenamento em camadas (Hot/Cold Storage)** com **boas práticas de backend** de nível empresarial. O sistema automatiza o arquivamento de dados antigos, mantendo dados recentes em acesso otimizado através de **SQLite consolidado**.

A solução é ideal para empresas que precisam:
- 🔥 Manter dados recentes rápidos e acessíveis
- ❄️ Arquivar dados históricos com segurança
- 📊 Reduzir custos operacionais de banco de dados
- 🏗️ Implementar padrões profissionais de backend
- 🔄 Automatizar o processo de arquivamento

---

## ✨ Funcionalidades Principais

- ✅ **Gerenciamento automático de vendas** - CRUD completo com validação rigorosa
- ✅ **Arquivamento inteligente** - Movimentação automática de dados com mais de 90 dias
- ✅ **Armazenamento consolidado em SQLite** - Única fonte de verdade, performance otimizada
- ✅ **API RESTful** - Endpoints bem documentados com Swagger/OpenAPI
- ✅ **Validação robusta** - FluentValidation para todas as requisições
- ✅ **Tratamento global de exceções** - Middleware centralizado com HTTP status corretos
- ✅ **Health Check endpoint** - Monitoramento de saúde da API
- ✅ **Configuration Pattern** - Gerenciamento profissional de settings
- ✅ **CancellationToken support** - Operações async seguras e cancellables
- ✅ **Injeção de Dependências** - Arquitetura desacoplada e testável
- ✅ **Clean Architecture** - Separação clara de responsabilidades

---

## 🧱 Stack Utilizada

| Camada | Tecnologia | Versão |
|--------|-----------|--------|
| **API & Backend** | .NET | 8.0 |
| **Linguagem** | C# | Latest |
| **Banco de Dados** | SQLite | 3.x |
| **ORM** | Entity Framework Core | 8.0.5 |
| **Validação** | FluentValidation | 11.11.0 |
| **Health Checks** | Microsoft.Extensions.Diagnostics.HealthChecks | 8.0.0 |
| **Documentação** | Swagger UI | Built-in |
| **Logging** | Serilog Ready | Structured Logging |

---

## 📦 Arquitetura - Clean Architecture

O projeto segue o padrão **Clean Architecture** com separação clara de responsabilidades:

```
src/
├── DeepArchiveBridge.Core/                    # Núcleo da aplicação
│   ├── Exceptions/
│   │   └── DomainExceptions.cs               # Custom exceptions (NotFoundException, ValidationException, etc)
│   ├── Interfaces/                            # Contratos de serviços
│   │   ├── IArchivingService.cs              # Interface de arquivamento
│   │   ├── IColdStorageService.cs            # Interface de Cold Storage
│   │   └── IVendaRepository.cs               # Interface de repositório (com CancellationToken)
│   └── Models/                                # Entidades de domínio
│       ├── Venda.cs                          # Modelo de venda
│       ├── VendaItem.cs                      # Itens da venda
│       ├── Dtos.cs                           # DTOs de transferência
│       └── ApplicationOptions.cs             # Settings (ArchivingOptions, LoggingOptions, ApiOptions)
│
├── DeepArchiveBridge.Data/                    # Camada de dados
│   ├── Context/
│   │   └── VendaDbContext.cs                 # DbContext SQLite único
│   ├── Repositories/
│   │   └── VendaRepository.cs                # Implementação com async/await e CancellationToken
│   └── Services/
│       ├── ColdStorageService.cs             # Serviço de Cold Storage
│       └── ArchivingService.cs               # Lógica de arquivamento automático
│
└── DeepArchiveBridge.API/                     # API RESTful
    ├── Controllers/                           # Controladores com validação integrada
    │   ├── VendasController.cs                # Endpoints de vendas (CRUD)
    │   ├── ArquivamentoController.cs          # Endpoints de arquivamento
    │   └── HealthController.cs                # Health check endpoints
    ├── Middleware/
    │   └── GlobalExceptionHandlerMiddleware.cs # Tratamento centralizado de exceções
    ├── Validators/
    │   └── RequestValidators.cs               # FluentValidation rules
    ├── Program.cs                             # Configuração DI e middleware
    └── appsettings*.json                      # Configurações (produção e desenvolvimento)
```

---

## 🛠️ Como Rodar o Projeto

### 1. Pré-requisitos

Certifique-se de ter instalado:

- ✅ **.NET 8 SDK** → [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ **Git** → [Download](https://git-scm.com/)

### 2. Clone o Repositório

```bash
git clone https://github.com/seu-usuario/DeepArchive-Bridge.git
cd DeepArchive-Bridge
```

### 3. Instale as Dependências

```bash
dotnet restore
```

### 4. Executar a API

```bash
cd src/DeepArchiveBridge.API
dotnet run
```

A API estará disponível em: **http://localhost:5000**  
Swagger UI disponível em: **http://localhost:5000/swagger**

---

## 🚀 Estrutura de Configuração

### appsettings.json (Produção)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=archive.db;Cache=Shared"
  },
  "ArchivingSettings": {
    "RetentionDaysHot": 90,
    "DefaultPageSize": 100,
    "MaxPageSize": 500,
    "CommandTimeout": 30
  },
  "LoggingSettings": {
    "UseStructuredLogging": true,
    "LogHttpRequests": true,
    "MinimumLogLevel": "Information"
  },
  "ApiSettings": {
    "EnableCors": false,
    "AllowedOrigins": [],
    "EnableHealthCheck": true,
    "ApiVersion": "1.0.0"
  }
}
```

### appsettings.Development.json (Desenvolvimento)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=archive.db;Cache=Shared"
  },
  "ArchivingSettings": {
    "RetentionDaysHot": 30,
    "DefaultPageSize": 50,
    "MaxPageSize": 200,
    "CommandTimeout": 60
  },
  "LoggingSettings": {
    "UseStructuredLogging": true,
    "LogHttpRequests": true,
    "MinimumLogLevel": "Debug"
  },
  "ApiSettings": {
    "EnableCors": true,
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5000"],
    "EnableHealthCheck": true,
    "ApiVersion": "1.0.0-dev"
  }
}
```

**Configurações disponíveis:**
- `ArchivingSettings` - Controla retenção, paginação e timeouts
- `LoggingSettings` - Habilita logging estruturado
- `ApiSettings` - CORS, Health Check e versionamento

---

## 📚 Endpoints Principais

### Health Check

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/health` | Status completo com uptime e versão |
| `GET` | `/api/health/ping` | Simples ping de verificação |

### Vendas

| Método | Endpoint | Descrição | Status Esperado |
|--------|----------|-----------|-----------------|
| `GET` | `/api/vendas/{id}` | Obtém detalhes de uma venda | 200, 404 |
| `POST` | `/api/vendas/buscar` | Busca com filtros e paginação | 200, 400 |
| `POST` | `/api/vendas` | Cria uma nova venda | 201, 400 |
| `PUT` | `/api/vendas/{id}` | Atualiza uma venda | 200, 400, 404 |
| `DELETE` | `/api/vendas/{id}` | Deleta uma venda | 200, 404 |

### Arquivamento

| Método | Endpoint | Descrição | Status Esperado |
|--------|----------|-----------|-----------------|
| `GET` | `/api/arquivamento/info` | Info sobre dados a arquivar | 200, 500 |
| `POST` | `/api/arquivamento/executar` | Executa com confirmação prévia | 200, 500 |
| `POST` | `/api/arquivamento/executar-automatico` | Para agendamento automático | 200, 500 |

### Swagger UI

Acesse a documentação **interativa** em: **http://localhost:5000/swagger**

---

## 🏗️ Boas Práticas Implementadas

### 1. **Tratamento Global de Exceções**
```csharp
NotFoundException        → HTTP 404
ValidationException     → HTTP 400
ConflictException       → HTTP 409
UnauthorizedException   → HTTP 401
TimeoutException        → HTTP 504
Exception genérica      → HTTP 500
```

### 2. **Validação com FluentValidation**
- **BuscaVendaRequest**: Valida datas, ranges de paginação
- **VendaValidator**: Valida dados obrigatórios, formatos
- **VendaItemValidator**: Valida quantidades e preços

### 3. **Configuration Pattern (Options)**
- `ArchivingOptions` - Settings de arquivamento
- `LoggingOptions` - Configurações de logging
- `ApiOptions` - Configurações da API

### 4. **CancellationToken**
Todos os métodos async suportam cancelamento seguro:
```csharp
public async Task<List<Venda>> BuscarAsync(
    BuscaVendaRequest request, 
    EstrategiaArmazenamento estrategia = EstrategiaArmazenamento.Auto,
    CancellationToken cancellationToken = default)
```

### 5. **Health Check**
Monitoramento de saúde com uptime e versionamento automático

### 6. **Logging Estruturado**
Pronto para integração com Serilog e JSON estruturado

---

## 📂 Banco de Dados - SQLite

### Arquivo de Banco de Dados

```
archive.db          # Banco de dados SQLite único (gerado automaticamente)
```

### Criação Automática

O banco de dados é criado automaticamente na primeira execução via EF Core.

---

## 🔄 Fluxo de Arquivamento

```
┌──────────────────┐
│   Nova Venda     │
└────────┬─────────┘
         │
         ↓
┌───────────────────────────┐
│ SQLite (Hot Storage)      │ ← Dados recentes, acesso rápido
│ (vendas < 90 dias)        │
└───────────────────────────┘
         │
         │ (após 90 dias, automático)
         ↓
┌──────────────────────────┐
│ Cold Storage              │ ← Arquivado, compactado
│ (vendas > 90 dias)       │
└──────────────────────────┘
```

---

## 🚀 Build para Produção

### Gerar Release Build

```bash
dotnet build --configuration Release
```

### Publicar

```bash
dotnet publish --configuration Release --output ./publish
```

### Executar em Produção

```bash
cd publish
dotnet DeepArchiveBridge.API.dll --urls "http://0.0.0.0:80"
```

---

## 🧪 Testando a API

### Via Swagger UI

1. Acesse `http://localhost:5000/swagger`
2. Clique em qualquer endpoint
3. Clique em "Try It Out"
4. Preencha os parâmetros
5. Clique em "Execute"

### Via Client HTTP do Visual Studio

Crie um arquivo `.http`:

```http
@baseUrl = http://localhost:5000/api

### Health Check
GET {{baseUrl}}/health

### Buscar vendas
POST {{baseUrl}}/vendas/buscar
Content-Type: application/json

{
  "dataInicio": "2026-01-01",
  "dataFim": "2026-12-31",
  "skip": 0,
  "take": 10
}

### Informações de arquivamento
GET {{baseUrl}}/arquivamento/info

### Executar arquivamento
POST {{baseUrl}}/arquivamento/executar-automatico
```

### Via cURL

```bash
# Health Check
curl http://localhost:5000/api/health

# Buscar vendas
curl -X POST http://localhost:5000/api/vendas/buscar \
  -H "Content-Type: application/json" \
  -d '{"dataInicio":"2026-01-01","dataFim":"2026-12-31","skip":0,"take":10}'
```

---

## 🤝 Contribuindo

1. **Fork** o repositório
2. Crie uma **branch** para sua feature (`git checkout -b feature/AmazingFeature`)
3. **Commit** suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. **Push** para a branch (`git push origin feature/AmazingFeature`)
5. Abra um **Pull Request**

---

## 📝 Licença

Este projeto está licenciado sob a **MIT License** - veja o arquivo LICENSE para detalhes.

---

## 👨‍💻 Autor

Desenvolvido com ❤️ usando **.NET 8**
# Iniciar os serviços
docker-compose up -d

# Verificar logs
docker-compose logs -f postgres

# Parar os serviços
docker-compose down
```

### Variáveis de Ambiente do PostgreSQL

```yaml
POSTGRES_USER: deeparchive_user
POSTGRES_PASSWORD: secure_password_123
POSTGRES_DB: deeparchive_db
```

---

## 📚 Endpoints Principais

### Arquivamento

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/arquivamento/info` | Obtém informações sobre arquivamento |
| `POST` | `/api/arquivamento/arquivar` | Executa arquivamento automático |

### Vendas

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/vendas` | Lista todas as vendas |
| `GET` | `/api/vendas/{id}` | Obtém detalhes de uma venda |
| `POST` | `/api/vendas` | Cria uma nova venda |
| `PUT` | `/api/vendas/{id}` | Atualiza uma venda |
| `DELETE` | `/api/vendas/{id}` | Deleta uma venda |

### Swagger UI

Acesse a documentação interativa em: **http://localhost:5000/swagger**

---

## 🔄 Fluxo de Arquivamento

```
┌─────────────────┐
│   Nova Venda    │
└────────┬────────┘
         │
         ↓
┌─────────────────────────────┐
│  Armazenamento Quente (DB)  │ ← Acesso rápido
└─────────────────────────────┘
         │
         │ (após 90 dias)
         ↓
┌──────────────────────────┐
│ Armazenamento Frio (.zip)│ ← Arquivo compactado
└──────────────────────────┘
```

---

## 📂 Estrutura de Configuração

### appsettings.json

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=deeparchive_db;Username=deeparchive_user;Password=secure_password_123"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

---

## 🧪 Testes

Execute os testes do projeto:

```bash
dotnet test
```

---

## 🚀 Build para Produção

### Gerar Release Build

```bash
dotnet build --configuration Release
```

### Publicar

```bash
dotnet publish --configuration Release --output ./publish
```

---

## 📋 Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | `Development` |
| `ConnectionStrings__PostgreSQL` | String de conexão | Veja appsettings.json |
| `ASPNETCORE_URLS` | URLs da aplicação | `http://localhost:5000` |

---

## 🤝 Contribuindo

1. **Fork** o repositório
2. Crie uma **branch** para sua feature (`git checkout -b feature/AmazingFeature`)
3. **Commit** suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. **Push** para a branch (`git push origin feature/AmazingFeature`)
5. Abra um **Pull Request**

---

## 📝 Licença

Este projeto está licenciado sob a **MIT License** - veja o arquivo LICENSE para detalhes.

---

## 👨‍💻 Contato & Suporte

Se você tiver dúvidas, sugestões ou encontrar algum problema:

- 📧 **Email**: daniel.azevedo081205@gmail.com
- 🐛 **Issues**: [Reporte um bug](https://github.com/seu-usuario/DeepArchive-Bridge/issues)
- 💬 **Discussões**: [Participe da comunidade](https://github.com/seu-usuario/DeepArchive-Bridge/discussions)

---

<div align="center">

**Desenvolvido com ❤️ usando .NET 8**

[⭐ Se este projeto foi útil para você, considere dar uma estrela!](https://github.com/seu-usuario/DeepArchive-Bridge)

</div>