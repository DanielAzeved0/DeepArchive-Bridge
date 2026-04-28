# DeepArchive-Bridge Backend

API REST em ASP.NET Core 8 para gerenciamento de vendas, autenticação JWT, health checks e arquivamento lógico sobre SQLite.

## Projetos

```text
src/
├── DeepArchiveBridge.API   # Controllers, middleware, autenticação, Swagger
├── DeepArchiveBridge.Core  # Modelos, DTOs, interfaces e exceções
└── DeepArchiveBridge.Data  # EF Core, DbContext, repositórios e serviços
```

## Configuração

A aplicação usa SQLite por padrão:

```json
{
  "ConnectionStrings": {
    "SQLite": "Data Source=archive.db;Cache=Shared"
  }
}
```

O `Program.cs` também aceita `DefaultConnection` como fallback de compatibilidade.

## Como Rodar

```bash
cd src/DeepArchiveBridge.API
dotnet run
```

API: `http://localhost:5000`

Swagger, em ambiente de desenvolvimento: `http://localhost:5000/swagger`

## Endpoints

Autenticação:
- `POST /api/auth/token?clienteId=app-frontend`

Health:
- `GET /api/health`
- `GET /api/health/ping`

Vendas:
- `POST /api/vendas/buscar`
- `GET /api/vendas/{id}`
- `POST /api/vendas`
- `PUT /api/vendas/{id}`
- `POST /api/vendas/{id}/aprovar`
- `DELETE /api/vendas/{id}`

Arquivamento:
- `GET /api/arquivamento/info`
- `POST /api/arquivamento/executar`
- `POST /api/arquivamento/executar-automatico`

## Arquivamento

O código atual usa um SQLite unificado. O serviço de arquivamento identifica vendas com mais de 90 dias e valida que elas estão disponíveis para consulta pelo serviço de Cold Storage lógico.

Ele não remove registros da base ativa, porque ainda não há dois armazenamentos físicos separados. Isso evita perda de dados.

Para evoluir para Hot/Cold real, crie contextos separados para origem e destino, configure connection strings independentes e só então habilite remoção da origem após confirmação de gravação no destino.

## Validação

```bash
dotnet restore DeepArchiveBridge.sln --configfile ../NuGet.config
dotnet build DeepArchiveBridge.sln --no-restore
dotnet test DeepArchiveBridge.sln --no-build --no-restore
```

O projeto `src/DeepArchiveBridge.Tests` contém testes xUnit reais para autenticação JWT, fluxo de vendas e arquivamento seguro em SQLite.
