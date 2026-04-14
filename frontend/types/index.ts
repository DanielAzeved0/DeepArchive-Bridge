// tipos.ts - Definições de tipos para toda a aplicação

export interface ApiResponse<T> {
  sucesso: boolean
  dados?: T
  mensagem?: string
  origem?: string
  tempoMs?: number
}

export interface HealthStatus {
  status: string
  timestamp: string
  apiVersion: string
  uptime: number
  memoryMB: number
  checkDurationMs?: number
  dependenciesHealthy?: number
  dependenciesUnhealthy?: number
}

export interface VendaItem {
  id?: number
  descricao: string
  quantidade: number
  preco: number
  subtotal?: number
}

export interface Venda {
  id: number
  clienteId: string
  clienteNome: string
  valor: number
  status: 'Pendente' | 'Processada' | 'Arquivada'
  dataCriacao: string
  dataModificacao?: string
  itens: VendaItem[]
  observacoes?: string
  rowVersion?: string
}

export interface BuscaVendaRequest {
  dataInicio: string
  dataFim: string
  clienteId?: string
  skip: number
  take: number
}

export interface CreateVendaRequest {
  clienteNome: string
  clienteId?: string
  valor: number
  itens: VendaItem[]
  observacoes?: string
}

export interface UpdateVendaRequest {
  id: number
  clienteNome: string
  valor: number
  itens: VendaItem[]
  observacoes?: string
}

export interface ArquivamentoInfo {
  vendaRelativosArmazem: number
  vendaRelativosArquivo: number
  ultimoProcessamento?: string
  proximoProcessamento?: string
  statusProcessamento: 'Ativo' | 'Pausado' | 'Erro'
}

export interface LoginRequest {
  clienteId: string
  senha: string
}

export interface AuthResponse {
  token: string
  expiresIn: number
  tokenType: string
  usuarioId: string
  usuarioNome: string
  role: 'User' | 'Admin' | 'Manager'
}

export interface PaginationParams {
  skip: number
  take: number
  total?: number
}

export interface ErrorResponse {
  mensagem: string
  erros?: string[]
  statusCode?: number
}
