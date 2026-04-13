# 🔒 GUIA DE SEGURANÇA E BOAS PRÁTICAS

## Implementações de Segurança Realizadas

### ✅ Backend (.NET 8)

#### 1. Autenticação JWT
- Implementado Bearer Token Authentication
- Tokens com expiração configurável (padrão: 24h)
- Validação de issuer, audience e assinatura

**Uso:**
```bash
# Gerar token (via controller de autenticação - a implementar)
POST /api/auth/token
```

#### 2. Autorização baseada em Roles
- Todos os endpoints críticos requerem `[Authorize]`
- Health check permite acesso anônimo com `[AllowAnonymous]`
- Suporte a roles específicas (Admin, User)

#### 3. Rate Limiting
- Limite: 100 requisições por minuto por IP
- Middleware customizado previne força bruta e DDoS
- Configurável em `appsettings.json`

#### 4. CORS Restritivo
- Apenas métodos específicos: GET, POST, PUT, DELETE
- Headers restringidos: Content-Type, Authorization
- Credenciais apenas para origens permitidas

#### 5. Exception Handling Seguro
- GlobalExceptionHandler evita exposição de stack traces em produção
- Apenas error ID é retornado para erros 500
- Detalhes técnicos apenas em Development

#### 6. Validação de Entrada
- FluentValidation em todos os requests
- Validação de timestamps (DataVenda não pode ser >2 anos atrás)
- Sanitização de ClienteId (apenas alphanumería e hífen)

#### 7. Logging Seguro
- Não loga dados sensíveis (PII - Personally Identifiable Information)
- Loga apenas IDs e contagens
- Diferencia produção vs desenvolvimento

---

### ✅ Frontend (Next.js)

#### 1. Error Boundary Component
- Captura erros em tempo de renderização
- Exibe fallback UI em vez de quebrar toda a página
- Integrado em `ErrorBoundary.tsx`

#### 2. Tratamento de Erros API
- 401/403: Redireciona para `/login` automaticamente
- Valida `sucesso` flag antes de usar dados
- Logs informativos sem exposição de dados

#### 3. Type Safety
- Tipos TypeScript explícitos (sem `any`)
- DTOs definidos em `types/index.ts`
- Conversão segura de dados

#### 4. Proteção contra XSS
- Next.js escapa HTML por padrão em `{}`
- Componentes sanitizam dados dinâmicos
- Input validation em formulários

#### 5. Gerenciamento de Estado
- Desabilita interações durante requisições
- Loading states implementados
- Evita requisições duplicadas

---

## 🔐 Checklist de Produção

### Antes de Deploy, Verifique:

- [ ] **JWT Secret Key** - Mudar em produção (mínimo 32 caracteres)
  ```json
  "JwtSettings": {
    "SecretKey": "MUDE-PARA-CHAVE-MUITO-LONGA-E-SEGURA-EM-PRODUCAO"
  }
  ```

- [ ] **CORS Origins** - Atualizar para domínios reais
  ```json
  "ApiSettings": {
    "AllowedOrigins": "https://seu-dominio.com,https://app.seu-dominio.com"
  }
  ```

- [ ] **HTTPS Habilitado**
  ```csharp
  app.UseHttpsRedirection(); // ✅ Já habilitado
  ```

- [ ] **Variáveis de Ambiente**
  - Não versione `.env.local` ou `appsettings.Development.json`
  - Use secrets management do host (AWS Secrets Manager, Azure Key Vault, etc)

- [ ] **Database Backup**
  - Configure backup automático do banco SQLite
  - Mínimo: backup diário

- [ ] **Logging**
  ```json
  "Logging": {
    "LogLevel": {
      "Default": "Information", // ✅ Reduzir verbosidade em produção
      "Microsoft.AspNetCore": "Warning"
    }
  }
  ```

- [ ] **Rate Limiting** - Ajustar conforme necessário
  ```json
  "RateLimitRequestsPerMinute": 100
  ```

- [ ] **CORS AllowCredentials** - Use apenas se necessário
  ```csharp
  .AllowCredentials() // ⚠️ Verificar necessidade
  ```

---

## 🚀 Variáveis de Ambiente Necessárias

### Backend (appsettings.json - Produção)
```json
{
  "JwtSettings": {
    "SecretKey": "CHAVE-MUITO-GRANDE-MUDE-EM-PRODUCAO-MINIMO-32-CHARS",
    "Issuer": "DeepArchiveBridge",
    "Audience": "DeepArchiveBridge-API",
    "ExpirationHours": 24
  },
  "ApiSettings": {
    "AllowedOrigins": "https://seu-dominio.com,https://app.seu-dominio.com",
    "RateLimitRequestsPerMinute": 100
  }
}
```

### Frontend (.env.local - Desenvolvimento APENAS)
```bash
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

### Frontend (build time - Produção)
```bash
# Via CI/CD, não versione credenciais
NEXT_PUBLIC_API_URL=https://api.seu-dominio.com/api
```

---

## 📝 Padrões de Segurança Implementados

### 1. Princípio do Menor Privilégio
- Endpoints requerem autenticação
- Roles definem permissions granulares
- Dados retornam apenas campos necessários

### 2. Defense in Depth
- Validação no client (UX)
- Validação no servidor (obrigatória)
- Tratamento de exceção centralizado
- Rate limiting na camada HTTP

### 3. Secure by Default
- HTTPS forçado em produção
- Cookies seguros (HttpOnly, Secure, SameSite)
- CSP headers recomendados (adicionar)
- X-Frame-Options: DENY (adicionar)

---

## ⚠️ Vulnerabilidades Mitigadas

| Vulnerabilidade | Mitigação | Status |
|-----------------|-----------|--------|
| SQL Injection | EF Core parameterizado | ✅ |
| XSS | Escaping automático Next.js | ✅ |
| CSRF | Token JWT (sem cookies simples) | ✅ |
| Força Bruta | Rate Limiting | ✅ |
| DDoS | Rate Limiting + Load Balancer | ✅ |
| Exposição de Info | Exception Handler | ✅ |
| Acesso Não-Autorizado | JWT + [Authorize] | ✅ |
| PII em Logs | Logging seguro | ✅ |

---

## 🔄 Próximos Passos Recomendados

### Curto Prazo (Sprint 1)
- [ ] Implementar endpoint de autenticação (`POST /api/auth/token`)
- [ ] Adicionar refresh token mechanism
- [ ] Implementar logout e revogação de tokens
- [ ] Testes de segurança básicos

### Médio Prazo (Sprint 2-3)
- [ ] Implementar 2FA (autenticação de múltiplos fatores)
- [ ] Audit logging (quem/quando/o quê)
- [ ] Criptografia de dados sensíveis em repouso
- [ ] OWASP ZAP scanning

### Longo Prazo
- [ ] Penetration testing profissional
- [ ] Bug bounty program
- [ ] Compliance (GDPR, LGPD, etc)
- [ ] Security headers (CSP, HSTS, etc)

---

## 📚 Referências

- [OWASP Top 10 2021](https://owasp.org/Top10/)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)
- [Microsoft Security Best Practices](https://docs.microsoft.com/security)
- [Next.js Security](https://nextjs.org/docs/advanced-features/security)

