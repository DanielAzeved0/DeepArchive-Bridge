# 📦 DeepArchive-Bridge

**DeepArchive-Bridge** é uma API robusta desenvolvida em **.NET 8** que implementa um sistema inteligente de **armazenamento em camadas (Hot/Cold Storage)** para otimizar o gerenciamento de dados de vendas. O sistema automatiza o arquivamento de dados antigos para armazenamento em frio, mantendo dados recentes em acesso rápido no banco de dados quente.

A solução é ideal para empresas que precisam:
- 🔥 Manter dados recentes rápidos e acessíveis
- ❄️ Arquivar dados históricos com segurança
- 📊 Reduzir custos operacionais de banco de dados
- 🔄 Automatizar o processo de arquivamento

---

## 🚀 Funcionalidades

- ✅ **Gerenciamento automático de vendas** - CRUD completo de transações
- ✅ **Arquivamento inteligente** - Movimentação automática de dados com mais de 90 dias
- ✅ **Duas camadas de armazenamento**:
  - 🔥 **Hot Storage**: PostgreSQL com acesso rápido para dados recentes
  - ❄️ **Cold Storage**: Armazenamento de arquivo para dados históricos
- ✅ **API RESTful** - Endpoints bem documentados com Swagger
- ✅ **Injeção de Dependências** - Arquitetura desacoplada e testável
- ✅ **Docker Compose** - Ambiente pronto para desenvolvimento

---

## 🧱 Stack Utilizada

| Camada | Tecnologia | Versão |
|--------|-----------|--------|
| **API & Backend** | .NET | 8.0 |
| **Linguagem** | C# | - |
| **Banco de Dados** | PostgreSQL | 16 |
| **ORM** | Entity Framework Core | Latest |
| **Containerização** | Docker & Docker Compose | - |
| **Documentação** | Swagger UI | Built-in |

---

## 📦 Arquitetura

O projeto segue o padrão **Clean Architecture** com separação clara de responsabilidades:

```
src/
├── DeepArchiveBridge.Core/          # Núcleo da aplicação
│   ├── Interfaces/                  # Contratos de serviços
│   │   ├── IArchivingService.cs    # Interface de arquivamento
│   │   ├── IStorageServices.cs     # Interfaces de armazenamento
│   │   └── IVendaRepository.cs     # Interface de repositório
│   └── Models/                      # Entidades de domínio
│       ├── Venda.cs                # Modelo de venda
│       ├── VendaItem.cs            # Itens da venda
│       └── Dtos.cs                 # DTOs de transferência
│
├── DeepArchiveBridge.Data/          # Camada de dados
│   ├── Context/                     # Contexto do EF Core
│   ├── Repositories/                # Padrão repositório
│   └── Services/                    # Serviços de dados
│
└── DeepArchiveBridge.API/           # API RESTful
    ├── Controllers/                 # Controladores
    │   ├── ArquivamentoController.cs # Endpoints de arquivamento
    │   └── VendasController.cs       # Endpoints de vendas
    ├── Program.cs                   # Configuração da aplicação
    └── appsettings.json             # Configurações
```

---

## 🛠️ Como Rodar o Projeto (Desenvolvedor)

### 1. Pré-requisitos

Certifique-se de ter instalado:

- ✅ **.NET 8 SDK** → [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ **PostgreSQL 16** → [Download](https://www.postgresql.org/download/)
- ✅ **Docker & Docker Compose** *(opcional, mas recomendado)* → [Download](https://www.docker.com/)
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

### 4. Configure o Banco de Dados

#### Opção A: Com Docker Compose (Recomendado)

Execute o PostgreSQL em um container:

```bash
docker-compose up -d
```

#### Opção B: PostgreSQL Local

Crie um banco de dados manualmente:

```sql
CREATE DATABASE deeparchive_db;
CREATE USER deeparchive_user WITH PASSWORD 'secure_password_123';
ALTER ROLE deeparchive_user SET client_encoding TO 'utf8';
GRANT ALL PRIVILEGES ON DATABASE deeparchive_db TO deeparchive_user;
```

### 5. Configure a Connection String

Edite `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=deeparchive_db;Username=deeparchive_user;Password=secure_password_123"
  }
}
```

### 6. Aplique as Migrations

```bash
cd src/DeepArchiveBridge.API
dotnet ef database update
```

### 7. Execute a API

```bash
dotnet run
```

A API estará disponível em: **http://localhost:5000**

---

## 🐳 Docker & Docker Compose

### Executar com Docker Compose

O projeto inclui um `docker-compose.yml` pré-configurado com PostgreSQL:

```bash
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