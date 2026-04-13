# 🔐 GUIA: Implementando Login e Autenticação JWT

## Próximas Etapas Recomendadas

Todas as correções de segurança foram implementadas. Para completar o sistema com login, siga este guia.

---

## 1. Criar Controller de Autenticação

### Backend: `Controllers/AuthController.cs` (NOVO)

```csharp
using DeepArchiveBridge.API.Services;
using DeepArchiveBridge.Core.Exceptions;
using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeepArchiveBridge.API.Controllers;

[ApiController]
[Route("api/[controller}")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Gera um token JWT para autenticação
    /// </summary>
    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("Credenciais inválidas");

        // TODO: Validar credenciais em banco de usuários
        // Por enquanto, usar para testes
        if (request.Username == "admin" && request.Password == "admin123")
        {
            var token = _authService.GenerateToken("1", "admin", "Admin");
            return Ok(new ApiResponse<object>
            {
                Sucesso = true,
                Dados = new { token, expiresIn = 86400 }, // 24h em segundos
                Mensagem = "Token gerado com sucesso"
            });
        }

        throw new UnauthorizedException("Credenciais inválidas");
    }

    /// <summary>
    /// Valida um token JWT
    /// </summary>
    [HttpPost("validate")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        return Ok(new ApiResponse<object>
        {
            Sucesso = true,
            Mensagem = "Token válido"
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

---

## 2. Criar Página de Login (Frontend)

### Frontend: `app/login/page.tsx` (NOVO)

```tsx
'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import axios from 'axios'

export default function LoginPage() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const [carregando, setCarregando] = useState(false)
  const router = useRouter()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!username || !password) {
      setErro('Preencha usuário e senha')
      return
    }

    try {
      setCarregando(true)
      setErro(null)

      const response = await axios.post(
        `${process.env.NEXT_PUBLIC_API_URL}/auth/token`,
        { username, password }
      )

      if (response.data.sucesso) {
        // Salvar token no localStorage (NÃO em cookies por simplicidade)
        localStorage.setItem('authToken', response.data.dados.token)
        
        // Adicionar token aos headers futuros
        axios.defaults.headers.common['Authorization'] = `Bearer ${response.data.dados.token}`
        
        // Redirecionar para dashboard
        router.push('/')
      } else {
        setErro(response.data.mensagem || 'Erro ao fazer login')
      }
    } catch (error: any) {
      setErro(error.response?.data?.mensagem || 'Falha na autenticação')
    } finally {
      setCarregando(false)
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-600 to-blue-800 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-xl p-8 max-w-md w-full">
        <h1 className="text-3xl font-bold text-center text-gray-900 mb-2">
          📦 DeepArchive
        </h1>
        <p className="text-center text-gray-600 mb-8">
          Bridge - Sistema de Vendas
        </p>

        {erro && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800">❌ {erro}</p>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Usuário
            </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Seu usuário..."
              disabled={carregando}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Senha
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Sua senha..."
              disabled={carregando}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
            />
          </div>

          <button
            type="submit"
            disabled={carregando}
            className="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white rounded-lg font-bold transition"
          >
            {carregando ? '⏳ Autenticando...' : '🔓 Entrar'}
          </button>
        </form>

        <p className="text-center text-sm text-gray-600 mt-6">
          Para testes: admin / admin123
        </p>
      </div>
    </div>
  )
}
```

---

## 3. Adicionar Interceptor de Token no Client

### Atualizar: `lib/api.ts`

```ts
// Adicione ao iniciar a aplicação
if (typeof window !== 'undefined') {
  const token = localStorage.getItem('authToken')
  if (token) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token}`
  }
}

// E adicione listener para armazenar token
if (typeof window !== 'undefined') {
  window.addEventListener('storage', () => {
    const token = localStorage.getItem('authToken')
    if (token) {
      api.defaults.headers.common['Authorization'] = `Bearer ${token}`
    } else {
      delete api.defaults.headers.common['Authorization']
    }
  })
}
```

---

## 4. Criar Serviço de Autenticação (Frontend)

### Frontend: `lib/auth.ts` (NOVO)

```ts
export const authService = {
  login: async (username: string, password: string) => {
    // Implementado em AuthController
    const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auth/token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    })
    return response.json()
  },

  logout: () => {
    localStorage.removeItem('authToken')
    delete axios.defaults.headers.common['Authorization']
  },

  getToken: () => localStorage.getItem('authToken'),

  isAuthenticated: () => !!localStorage.getItem('authToken'),
}
```

---

## 5. Proteger Rotas Restritas (Frontend)

### Middleware: `middleware.ts` (NOVO)

```ts
import { NextRequest, NextResponse } from 'next/server'

export function middleware(request: NextRequest) {
  const publicRoutes = ['/login', '/health']
  const pathname = request.nextUrl.pathname

  // Permitir rotas públicas
  if (publicRoutes.some((route) => pathname.startsWith(route))) {
    return NextResponse.next()
  }

  // Verificar token em rotas protegidas
  const token = request.cookies.get('authToken')?.value
  if (!token && !pathname.startsWith('/login')) {
    return NextResponse.redirect(new URL('/login', request.url))
  }

  return NextResponse.next()
}

export const config = {
  matcher: [
    '/((?!_next/static|_next/image|favicon.ico).*)',
  ],
}
```

---

## 6. Adicionar Banco de Usuários (Backend - Futuro)

Para produção, use Entity Framework para gerenciar usuários:

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Configurar em DbContext
builder.Entity<User>().HasKey(u => u.Id);
```

---

## 7. Hash de Senhas (Backend - Segurança)

Nunca guarde senhas em plain text! Use bcrypt:

```bash
dotnet add package BCrypt.Net-Next
```

```csharp
using BCrypt.Net;

// Ao criar usuário
var hash = BCrypt.Net.BCrypt.HashPassword(senha);

// Ao validar
bool isValid = BCrypt.Net.BCrypt.Verify(senhaInformada, senhaHash);
```

---

## ✅ Checklist de Implementação

- [ ] Criar `AuthController.cs`
- [ ] Testar `POST /api/auth/token` 
- [ ] Criar página `/login`
- [ ] Adicionar interceptor de token
- [ ] Testar autenticação end-to-end
- [ ] Adicionar banco de usuários
- [ ] Implementar hash de senhas
- [ ] Adicionar refresh token
- [ ] Implementar logout
- [ ] Testar revogação de permissões

---

## 🧪 Testes Manuais

```bash
# Testar login
curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# Copiar token da resposta e testar acesso protegido
curl -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  http://localhost:5000/api/vendas/buscar
```

---

## 📚 Referências

- [JWT.io](https://jwt.io/)
- [Microsoft JWT Bearer Docs](https://docs.microsoft.com/aspnet/core/security/authentication/jwt-use)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

