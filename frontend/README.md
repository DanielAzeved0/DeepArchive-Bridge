# DeepArchive-Bridge Frontend

Frontend em Next.js 16, React 18, TypeScript e Tailwind CSS para consumir a API do DeepArchive-Bridge.

## Scripts

```bash
npm install
npm run dev
npm run type-check
npm run build
```

No PowerShell, use `npm.cmd` caso a execução de `npm.ps1` esteja bloqueada:

```powershell
npm.cmd run type-check
npm.cmd run build
```

## Variáveis de Ambiente

Crie `.env.local` se quiser apontar para outra URL de API:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

Se a variável não existir, o frontend usa `http://localhost:5000/api`.

## Páginas

- `/`: dashboard
- `/vendas`: listagem de vendas
- `/vendas/novo`: criação de venda
- `/vendas/[id]`: detalhes de venda
- `/vendas/[id]/editar`: edição de venda
- `/arquivamento`: gerenciamento de arquivamento
- `/health` e `/admin/health`: status da API

## Contrato com a API

O frontend espera o formato padrão:

```json
{
  "sucesso": true,
  "mensagem": "Operação concluída",
  "dados": {},
  "tempoMs": 45
}
```

Vendas usam os campos principais:

```typescript
interface Venda {
  id: number
  clienteId: string
  clienteNome: string
  valor: number
  status: number | string
  dataVenda: string
  dataCriacao: string
  itens: VendaItem[]
}
```

## Observação Sobre Arquivamento

A interface mostra vendas elegíveis para arquivamento, mas a implementação atual do backend usa SQLite unificado. Portanto, a operação valida disponibilidade em Cold Storage lógico e não remove registros.
