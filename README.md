# DeepArchive-Bridge - Full Stack

Monorepo com backend em .NET 8 e frontend em Next.js 16 para gerenciamento de vendas com uma camada de arquivamento lógico sobre SQLite.

## Estrutura

```text
DeepArchive-Bridge/
├── backend/
│   ├── DeepArchiveBridge.sln
│   └── src/
│       ├── DeepArchiveBridge.API/
│       ├── DeepArchiveBridge.Core/
│       └── DeepArchiveBridge.Data/
└── frontend/
    ├── app/
    ├── components/
    ├── lib/
    └── types/
```

## Stack

Backend:
- ASP.NET Core 8
- C# 12
- Entity Framework Core 8
- SQLite
- FluentValidation
- JWT Bearer

Frontend:
- Next.js 16.2.3
- React 18
- TypeScript
- Tailwind CSS
- Axios

## Como Rodar

Backend:

```bash
cd backend/src/DeepArchiveBridge.API
dotnet watch run
```

API: `http://localhost:5000`

Frontend:

```bash
cd frontend
npm install
npm run dev
```

App: `http://localhost:3000`

No PowerShell, se `npm` for bloqueado pela Execution Policy, use `npm.cmd`:

```powershell
npm.cmd install
npm.cmd run dev
```

## Funcionalidades

- CRUD de vendas
- Busca por período, cliente, status e paginação
- Aprovação de venda pendente
- Autenticação JWT automática no frontend
- Health check da API
- Tela de arquivamento para identificar vendas antigas

## Arquivamento

O projeto usa atualmente um banco SQLite unificado. A camada de arquivamento identifica vendas com mais de 90 dias e valida que elas continuam disponíveis pelo serviço de Cold Storage, mas não remove registros da base ativa.

Isso evita perda de dados enquanto não houver dois armazenamentos fisicamente separados. Para uma arquitetura Hot/Cold real, o próximo passo seria configurar dois contextos/connections independentes, por exemplo `HotConnection` e `ColdConnection`, e só então mover registros entre eles.

## Configuração

A API lê a connection string `ConnectionStrings:SQLite`. Por compatibilidade, também aceita `ConnectionStrings:DefaultConnection`.

```json
{
  "ConnectionStrings": {
    "SQLite": "Data Source=archive.db;Cache=Shared"
  }
}
```

## Validação

Comandos principais:

```powershell
.\check.cmd
```

O script executa:

```text
dotnet restore
dotnet build
dotnet test
npm.cmd run type-check
npm.cmd run build
```
