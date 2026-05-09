import axios from 'axios'
import { ApiResponse } from '@/types'
import { API_BASE_URL } from './config'

interface TokenResponse {
  token: string
  expiresIn: number
  tokenType: string
}

let tokenPromise: Promise<string> | null = null

export async function getToken(): Promise<string> {
  if (tokenPromise) {
    return tokenPromise
  }

  tokenPromise = (async () => {
    try {
      if (typeof window !== 'undefined') {
        const stored = window.localStorage.getItem('api_token')
        const expiresAt = window.localStorage.getItem('api_token_expires')

        if (stored && expiresAt && Date.now() < parseInt(expiresAt, 10)) {
          tokenPromise = null
          return stored
        }
      }

      const response = await axios.post<ApiResponse<TokenResponse>>(
        `${API_BASE_URL}/auth/token?clienteId=app-frontend`,
        {},
        { timeout: 10000 }
      )

      if (response.data.sucesso && response.data.dados?.token) {
        const token = response.data.dados.token
        const expiresIn = response.data.dados.expiresIn || 86400

        if (typeof window !== 'undefined') {
          window.localStorage.setItem('api_token', token)
          window.localStorage.setItem('api_token_expires', String(Date.now() + expiresIn * 1000 - 60000))
        }

        tokenPromise = null
        return token
      }

      throw new Error('Falha ao gerar token')
    } catch (error) {
      tokenPromise = null
      throw error
    }
  })()

  return tokenPromise
}
