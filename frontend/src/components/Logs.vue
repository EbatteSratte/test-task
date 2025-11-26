<template>
  <div class="page">
    <div class="page-header">
      <h2 class="page-title">📊 Логи ошибок</h2>
      <button @click="loadLogs" class="btn btn-secondary">
        🔄 Обновить
      </button>
    </div>

    <!-- Filter Controls -->
    <div class="search-controls">
      <input
        v-model="filterEntityId"
        @input="applyFilters"
        type="text"
        placeholder="Номер документа"
        class="form-control"
        style="max-width: 200px;"
      />
      
      <input
        v-model="dateFrom"
        @change="applyFilters"
        type="date"
        placeholder="Дата с"
        class="form-control"
        style="max-width: 150px;"
      />
      
      <input
        v-model="dateTo"
        @change="applyFilters"
        type="date"
        placeholder="Дата до"
        class="form-control"
        style="max-width: 150px;"
      />
      
      <button @click="clearFilters" class="btn btn-secondary">
        🗑️ Очистить фильтры
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading">
      Загрузка логов...
    </div>

    <!-- Error -->
    <div v-if="error" class="error">
      {{ error }}
    </div>

    <!-- Logs Table -->
    <div v-if="!loading && !error">
      <table class="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Дата и время</th>
            <th>Тип ошибки</th>
            <th>Сообщение</th>
            <th>Действие пользователя</th>
            <th>IP адрес</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="log in logs" :key="log.id">
            <td>{{ log.id }}</td>
            <td>{{ formatDateTime(log.timestamp) }}</td>
            <td>
              <span 
                :class="['error-type', getErrorTypeClass(log.errorType)]"
              >
                {{ log.errorType || 'Unknown' }}
              </span>
            </td>
            <td class="error-message">
              {{ truncateText(log.errorMessage, 100) }}
            </td>
            <td>{{ log.userAction || '-' }}</td>
            <td>{{ cleanIpAddress(log.ipAddress) || '-' }}</td>
            <td>
              <button 
                @click="openLogDetails(log)" 
                class="btn btn-sm btn-primary"
              >
                ℹ️ Инфо
              </button>
            </td>
          </tr>
          <tr v-if="logs.length === 0">
            <td colspan="7" style="text-align: center; padding: 2rem; color: #666;">
              {{ filteredEmpty ? 'Логи не найдены по заданным фильтрам' : 'Логи отсутствуют' }}
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination -->
      <div class="pagination" v-if="totalPages > 1">
        <button 
          @click="changePage(currentPage - 1)"
          :disabled="currentPage <= 1"
        >
          ‹ Назад
        </button>
        
        <button
          v-for="page in visiblePages"
          :key="page"
          @click="changePage(page)"
          :class="{ active: page === currentPage }"
        >
          {{ page }}
        </button>
        
        <button 
          @click="changePage(currentPage + 1)"
          :disabled="currentPage >= totalPages"
        >
          Вперед ›
        </button>
      </div>
    </div>

    <!-- Log Details Modal -->
    <div v-if="showDetailsModal" class="modal-overlay" @click="closeDetailsModal">
      <div class="modal" @click.stop style="max-width: 800px;">
        <div class="modal-header">
          <h3 class="modal-title">
            Подробности ошибки #{{ selectedLog?.id }}
          </h3>
          <button @click="closeDetailsModal" class="close-btn">&times;</button>
        </div>
        
        <div v-if="selectedLog" class="log-details">
          <div class="detail-row">
            <label><strong>Дата и время:</strong></label>
            <span>{{ formatDateTime(selectedLog.timestamp) }}</span>
          </div>
          
          <div class="detail-row">
            <label><strong>Тип ошибки:</strong></label>
            <span :class="['error-type', getErrorTypeClass(selectedLog.errorType)]">
              {{ selectedLog.errorType || 'Unknown' }}
            </span>
          </div>
          
          <div class="detail-row">
            <label><strong>Сообщение:</strong></label>
            <div class="error-message-full">{{ selectedLog.errorMessage }}</div>
          </div>
          
          <div class="detail-row" v-if="selectedLog.userAction">
            <label><strong>Действие пользователя:</strong></label>
            <span>{{ selectedLog.userAction }}</span>
          </div>
          
          <div class="detail-row" v-if="selectedLog.entityType">
            <label><strong>Тип сущности:</strong></label>
            <span>{{ selectedLog.entityType }}</span>
          </div>
          
          <div class="detail-row" v-if="selectedLog.entityId">
            <label><strong>Номер сущности:</strong></label>
            <span>{{ selectedLog.entityId }}</span>
          </div>
          
          <div class="detail-row" v-if="selectedLog.ipAddress">
            <label><strong>IP адрес:</strong></label>
            <span>{{ cleanIpAddress(selectedLog.ipAddress) }}</span>
          </div>
          
          <div class="detail-row" v-if="selectedLog.stackTrace">
            <label><strong>Stack Trace:</strong></label>
            <pre class="stack-trace">{{ selectedLog.stackTrace }}</pre>
          </div>
        </div>
        
        <div class="form-actions">
          <button @click="closeDetailsModal" class="btn btn-secondary">
            Закрыть
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { errorLogsApi } from '../services/api.js'

export default {
  name: 'Logs',
  data() {
    return {
      logs: [],
      loading: false,
      error: null,
      
      // Pagination
      currentPage: 1,
      pageSize: 20,
      totalCount: 0,
      
      // Filters
      filterEntityId: '',
      dateFrom: '',
      dateTo: '',
      filteredEmpty: false,
      
      // Modal
      showDetailsModal: false,
      selectedLog: null
    }
  },
  
  computed: {
    totalPages() {
      return Math.ceil(this.totalCount / this.pageSize)
    },
    
    visiblePages() {
      const pages = []
      const start = Math.max(1, this.currentPage - 2)
      const end = Math.min(this.totalPages, this.currentPage + 2)
      
      for (let i = start; i <= end; i++) {
        pages.push(i)
      }
      
      return pages
    }
  },
  
  mounted() {
    this.loadLogs()
  },
  
  methods: {
    async loadLogs() {
      this.loading = true
      this.error = null
      
      try {
        const response = await errorLogsApi.getAll({
          page: this.currentPage,
          pageSize: this.pageSize,
          entityId: this.filterEntityId || undefined,
          fromDate: this.dateFrom || undefined,
          toDate: this.dateTo || undefined
        })
        
        if (Array.isArray(response.data)) {
          this.logs = response.data
          this.totalCount = response.data.length
        } else if (response.data.items) {
          this.logs = response.data.items
          this.totalCount = response.data.totalCount || response.data.items.length
        } else {
          this.logs = []
          this.totalCount = 0
        }
        
      } catch (error) {
        this.error = 'Ошибка загрузки логов: ' + (error.response?.data?.message || error.message)
        console.error('Load logs error:', error)
      } finally {
        this.loading = false
      }
    },
    
    applyFilters() {
      this.currentPage = 1
      this.loadLogs()
    },
    
    clearFilters() {
      this.filterEntityId = ''
      this.dateFrom = ''
      this.dateTo = ''
      this.currentPage = 1
      this.loadLogs()
    },
    
    changePage(page) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page
        this.loadLogs()
      }
    },
    
    openLogDetails(log) {
      this.selectedLog = log
      this.showDetailsModal = true
    },
    
    closeDetailsModal() {
      this.showDetailsModal = false
      this.selectedLog = null
    },
    
    cleanIpAddress(ipAddress) {
      if (!ipAddress) return '-'
      return ipAddress.replace(/^::ffff:/, '')
    },
    
    formatDateTime(dateString) {
      if (!dateString) return '-'
      return new Date(dateString).toLocaleString('ru-RU', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      })
    },
    
    truncateText(text, maxLength) {
      if (!text) return '-'
      if (text.length <= maxLength) return text
      return text.substring(0, maxLength) + '...'
    },
    
    getErrorTypeClass(errorType) {
      if (!errorType) return 'unknown'
      
      if (errorType.includes('Validation')) return 'validation'
      if (errorType.includes('Database')) return 'database'
      if (errorType.includes('Api')) return 'api'
      if (errorType.includes('System')) return 'system'
      
      return 'unknown'
    }
  }
}
</script>

<style scoped>
.error-type {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 500;
}

.error-type.validation {
  background: #fff3cd;
  color: #856404;
  border: 1px solid #ffeaa7;
}

.error-type.database {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.error-type.api {
  background: #d1ecf1;
  color: #0c5460;
  border: 1px solid #bee5eb;
}

.error-type.system {
  background: #f4cccc;
  color: #5f2c3e;
  border: 1px solid #e8b4cb;
}

.error-type.unknown {
  background: #e2e3e5;
  color: #383d41;
  border: 1px solid #d6d8db;
}

.error-message {
  max-width: 250px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.log-details {
  max-height: 70vh;
  overflow-y: auto;
}

.detail-row {
  margin-bottom: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.detail-row label {
  color: #666;
  font-size: 0.9rem;
}

.error-message-full {
  background: #f8f9fa;
  padding: 1rem;
  border-radius: 4px;
  border: 1px solid #dee2e6;
  white-space: pre-wrap;
  word-break: break-word;
}

.stack-trace {
  background: #f8f8f8;
  padding: 1rem;
  border-radius: 4px;
  border: 1px solid #ddd;
  font-size: 0.8rem;
  max-height: 300px;
  overflow-y: auto;
  white-space: pre-wrap;
  word-break: break-all;
}

.search-controls select {
  appearance: none;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M6 8l4 4 4-4'/%3e%3c/svg%3e");
  background-position: right 0.5rem center;
  background-repeat: no-repeat;
  background-size: 1.5em 1.5em;
  padding-right: 2.5rem;
}
</style>