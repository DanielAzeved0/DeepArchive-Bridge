import axios from 'axios'
import { ApiResponse, Venda, ArquivamentoInfo, HealthStatus } from '@/types'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api'

interface TokenResponse {
  token: string
  expiresIn: number
  tokenType: string
}

// Armazenar referência para evitar múltiplas chamadas simultâneas
let tokenPromise: Promise<string> | null = null

// Função para obter token JWT
async function getToken(): Promise<string> {
  // Se já há uma solicitação de token em andamento, aguardar
  if (tokenPromise) {
    return tokenPromise
  }

  tokenPromise = (async () => {
    try {
      // Verificar se já há token no localStorage
      if (typeof window !== 'undefined') {
        const stored = window.localStorage.getItem('api_token')
        const expiresAt = window.localStorage.getItem('api_token_expires')
        
        if (stored && expiresAt && Date.now() < parseInt(expiresAt, 10)) {
          tokenPromise = null
          return stored
        }
      }

      // Gerar novo token
      const response = await axios.post<ApiResponse<TokenResponse>>(
        `${API_BASE_URL}/auth/token?clienteId=app-frontend`,
        {},
        { timeout: 10000 }
      )

      if (response.data.sucesso && response.data.dados?.token) {
        const token = response.data.dados.token
        const expiresIn = response.data.dados.expiresIn || 86400
        
        // Armazenar token no localStorage com tempo de expiração
        if (typeof window !== 'undefined') {
          window.localStorage.setItem('api_token', token)
          window.localStorage.setItem('api_token_expires', String(Date.now() + expiresIn * 1000 - 60000)) // -1min de buffer
        }

        tokenPromise = null
        return token
      }

      throw new Error('Falha ao gerar token')
    } catch (error) {
      tokenPromise = null
      console.error('Erro ao obter token:', error)
      throw error
    }
  })()

  return tokenPromise
}

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
})

// Interceptor para adicionar token no header
api.interceptors.request.use(
  async (config) => {
    try {
      const token = await getToken()
      config.headers.Authorization = `Bearer ${token}`
    } catch (error) {
      console.error('Erro ao adicionar token ao header:', error)
    }
    return config
  },
  (error) => Promise.reject(error)
)

// Interceptor para erros - incluindo tratamento de 401
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 || error.response?.status === 403) {
      // Token expirado - tentar limpar e redirecionar para login
      if (typeof window !== 'undefined') {
        window.localStorage.removeItem('api_token')
        window.localStorage.removeItem('api_token_expires')
        // Aguardar um pouco para permitir retry automático
        setTimeout(() => {
          window.location.href = '/login'
        }, 1000)
      }
    }
    console.error('API Error:', error.response?.status, error.response?.data?.mensagem || error.message)
    throw error
  }
)

// Serviço de Vendas
export const vendaService = {
  buscar: async (filtros: {
    dataInicio: string
    dataFim: string
    clienteId?: string
    status?: string
    skip: number
    take: number
  }) => {
    const response = await api.post<ApiResponse<Venda[]>>('/vendas/buscar', filtros)
    // Validar sempre sucesso antes de retornar dados
    if (response.data.sucesso && Array.isArray(response.data.dados)) {
      return response.data.dados
    }
    console.warn('API retornou sucesso=false ou dados inválidos:', response.data)
    return []
  },

  obterPorId: async (id: number) => {
    const response = await api.get<ApiResponse<Venda>>(`/vendas/${id}`)
    return response.data
  },

  criar: async (venda: Venda) => {
    console.log('Dados enviados para criar venda:', JSON.stringify(venda, null, 2))
    const response = await api.post<ApiResponse<number>>('/vendas', venda)
    return response.data
  },

  atualizar: async (id: number, venda: Venda) => {
    const response = await api.put<ApiResponse<object>>(`/vendas/${id}`, { ...venda, id })
    return response.data
  },

  aprovar: async (id: number) => {
    const response = await api.post<ApiResponse<object>>(`/vendas/${id}/aprovar`, {})
    return response.data
  },

  deletar: async (id: number) => {
    const response = await api.delete<ApiResponse<object>>(`/vendas/${id}`)
    return response.data
  },
}

// Serviço de Arquivamento
export const archivingService = {
  obterInfo: async () => {
    const response = await api.get<ApiResponse<ArquivamentoInfo>>('/arquivamento/info')
    return response.data
  },

  executar: async () => {
    const response = await api.post<ApiResponse<object>>('/arquivamento/executar')
    return response.data
  },

  executarAutomatico: async () => {
    const response = await api.post<ApiResponse<number>>('/arquivamento/executar-automatico')
    return response.data
  },
}

// Serviço de Health
export const healthService = {
  status: async () => {
    const response = await api.get<ApiResponse<HealthStatus>>('/health')
    return response.data
  },

  ping: async () => {
    const response = await api.get('/health/ping')
    return response.status === 200
  },
}

export default api
