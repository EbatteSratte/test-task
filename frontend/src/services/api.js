import axios from 'axios'

const API_BASE_URL = '/api/v1'

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

api.interceptors.request.use(
  config => {
    console.log('API Request:', config.method?.toUpperCase(), config.url)
    return config
  },
  error => {
    console.error('Request error:', error)
    return Promise.reject(error)
  }
)

api.interceptors.response.use(
  response => {
    console.log('API Response:', response.status, response.config.url)
    return response
  },
  error => {
    console.error('Response error:', error.response?.status, error.response?.data)
    
    if (error.response?.status === 401) {
    } else if (error.response?.status >= 500) {
    }
    
    return Promise.reject(error)
  }
)

export const mastersApi = {
  getAll: (search = '', sortBy = 'date', sortOrder = 'desc') => {
    const params = new URLSearchParams()
    if (search) params.append('search', search)
    params.append('sortBy', sortBy)
    params.append('sortOrder', sortOrder)
    
    return api.get(`/master?${params}`)
  },
  
  getById: (id) => api.get(`/master/${id}`),
  
  create: (data) => api.post('/master', data),
  
  update: (id, data) => api.put(`/master/${id}`, data),
  
  updateBasic: (id, data) => api.put(`/master/${id}/basic`, data),
  
  delete: (id) => api.delete(`/master/${id}`)
}

export const detailsApi = {
  getAll: () => api.get('/detail'),
  
  getById: (id) => api.get(`/detail/${id}`),
  
  create: (masterId, data) => api.post(`/detail/${masterId}`, data),
  
  update: (id, data) => api.put(`/detail/${id}`, data),
  
  delete: (id) => api.delete(`/detail/${id}`)
}

export const errorLogsApi = {
  getAll: (params = {}) => {
    const searchParams = new URLSearchParams()
    
    if (params.page) searchParams.append('page', params.page)
    if (params.pageSize) searchParams.append('pageSize', params.pageSize)
    if (params.entityId) searchParams.append('entityId', params.entityId)
    if (params.fromDate) searchParams.append('fromDate', params.fromDate)
    if (params.toDate) searchParams.append('toDate', params.toDate)
    
    return api.get(`/error-log?${searchParams}`)
  },
  
  delete: (id) => api.delete(`/error-log/${id}`)
}

export default api