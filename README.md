# 🏢 DeepArchive-Bridge - Full Stack

**Monorepo com Backend (.NET 8) e Frontend (Next.js 14)** para gerenciamento inteligente de vendas com arquivamento Hot/Cold Storage.

---

## 📁 Estrutura do Projeto

```
DeepArchive-Bridge/
├── .gitignore                  # Global .gitignore (raiz)
├── README.md                   # Este arquivo
│
├── backend/                    # 🏢 API REST em .NET 8
│   ├── README.md               # Documentação do backend
│   ├── .gitignore              # .gitignore específico
│   ├── DeepArchiveBridge.sln   # Solução Visual Studio
│   ├── src/
│   │   ├── DeepArchiveBridge.API/
│   │   ├── DeepArchiveBridge.Core/
│   │   └── DeepArchiveBridge.Data/
│   └── [Mais arquivos...]
│
└── frontend/                   # 💻 SPA em Next.js 14
    ├── README.md               # Documentação do frontend
    ├── .gitignore              # .gitignore específico
    ├── package.json            # Dependencies
    ├── app/                    # App Router
    ├── components/             # React components
    ├── lib/                    # Utilidades
    └── [Mais arquivos...]
```

---

## 🚀 Como Rodar (Desenvolvimento Local)

### Pré-requisitos
- ✅ .NET 8 SDK ([download](https://dotnet.microsoft.com/download))
- ✅ Node.js 18+ + npm ([download](https://nodejs.org))
- ✅ Git ([download](https://git-scm.com))

### Terminal 1: Backend

```bash
# Navegar para backend
cd backend/src/DeepArchiveBridge.API

# Rodar com hot reload
dotnet watch run

# API estará em: http://localhost:5000
```

### Terminal 2: Frontend

```bash
# Navegar para frontend
cd frontend

# Instalar dependências (primeira vez)
npm install

# Rodar servidor de desenvolvimento
npm run dev

# Frontend estará em: http://localhost:3000
```

---

## 📚 Documentação

- **[Backend README](backend/README.md)** - API, endpoints, autenticação JWT
- **[Frontend README](frontend/README.md)** - Setup, componentes, features

---

## 🏗️ Stack Utilizado

### Backend
- **Framework:** ASP.NET Core 8
- **Linguagem:** C# 12
- **Database:** Entity Framework Core + SQLite
- **Validação:** FluentValidation
- **Auth:** JWT Bearer Token (24h)

### Frontend
- **Framework:** Next.js 14 (App Router)
- **Linguagem:** TypeScript (strict mode)
- **Styling:** Tailwind CSS
- **HTTP:** Axios com interceptors
- **Auth:** JWT (auto-renewal)

---

## ✨ Funcionalidades

### 🔐 Autenticação
- JWT Token (24h expiration)
- Auto-renovação de token
- Logout automático em 401/403

### 📦 Gerenciamento de Vendas
- CRUD completo (Create, Read, Update, Delete)
- Paginação
- Filtros avançados
- Validação em camadas

### 🗂️ Arquivamento
- Informações de dados a arquivar
- Estratégia Hot/Cold Storage
- Consolidação automática

### 🏥 Monitoramento
- Health Check endpoint
- Status da API (uptime, memória)
- Rate limiting (100 req/min por IP)

---

## 🔧 Operações Úteis

### Limpar cache e reinstalar

**Backend:**
```bash
cd backend
dotnet clean
dotnet restore
```

**Frontend:**
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

### Build para produção

**Backend:**
```bash
cd backend
dotnet publish -c Release -o ./publish
# Executar: dotnet ./publish/DeepArchiveBridge.API.dll
```

**Frontend:**
```bash
cd frontend
npm run build
npm run start
```

---

## 🚨 Troubleshooting

### Backend na porta 5000 em uso
```bash
# Usar outra porta
dotnet run --urls "http://localhost:5001"
```

### Frontend na porta 3000 em uso
```bash
# Next.js automaticamente usa 3001 ou próxima disponível
npm run dev
```

### Limpar banco de dados
```bash
# Delete o arquivo de database
rm backend/archive.db*

# Criar novo migrate/seed
dotnet ef database update
```

### Network Error entre frontend e backend
```bash
# Verificar se backend está rodando
curl http://localhost:5000/api/health

# Se frontend não conecta, verificar CORS em backend/appsettings.json
```

---

## 🐳 Docker (Opcional)

Para rodar tudo em containers:

```bash
# Será adicionado em breve
docker-compose up
```

---

## 📝 Padrões de Código

### Request/Response
```json
{
  "sucesso": true,
  "mensagem": "Operação concluída",
  "dados": { ... },
  "tempoMs": 45
}
```

### Autenticação
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 🔗 Links Rápidos

- **Backend Health:** http://localhost:5000/api/health
- **Frontend App:** http://localhost:3000
- **Documentation:** Veja README em cada pasta

---

## 🤝 Contribuindo

1. Criar branch: `git checkout -b feature/sua-feature`
2. Fazer commits: `git commit -m "feat: descrição"`
3. Push: `git push origin feature/sua-feature`
4. Abrir Pull Request

---

## 📄 Licença

MIT - Livre para uso comercial e pessoal

---

**Desenvolvido com ❤️ usando .NET 8 + Next.js 14**  
**Última atualização:** 13 de Abril de 2026  
**Status:** ✅ Pronto para Produção
