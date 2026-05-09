import axios from 'axios'
import { API_BASE_URL } from './config'
import { getToken } from './auth'

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
})

api.interceptors.request.use(
  async (config) => {
    const token = await getToken()
    config.headers.Authorization = `Bearer ${token}`
    return config
  },
  (error) => Promise.reject(error)
)

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 || error.response?.status === 403) {
      if (typeof window !== 'undefined') {
        window.localStorage.removeItem('api_token')
        window.localStorage.removeItem('api_token_expires')
      }
    }

    throw error
  }
)

export default api
