<template>
  <div class="page">
    <div class="page-header">
      <h2 class="page-title">📄 Управление документами</h2>
      <button @click="openCreateModal" class="btn btn-primary">
        + Создать документ
      </button>
    </div>

    <!-- Search and Controls -->
    <div class="search-controls">
      <div class="search-input">
        <input
          v-model="searchQuery"
          @input="handleSearch"
          type="text"
          placeholder="Поиск по номеру или примечанию..."
          class="form-control"
        />
      </div>
      <button @click="clearFilters" class="btn btn-secondary" style="margin-right: 0.5rem;">
        🗑️ Очистить фильтры
      </button>
      <button @click="loadMasters" class="btn btn-secondary">
        🔄 Обновить
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading">
      Загрузка документов...
    </div>

    <!-- Error -->
    <div v-if="error" class="error">
      {{ error }}
    </div>

    <!-- Documents Table -->
    <div v-if="!loading && !error">
      <table class="table">
        <thead>
          <tr>
            <th 
              @click="sortBy === 'id' ? toggleSortOrder() : setSortBy('id')"
              class="sortable"
              :class="{ active: sortBy === 'id' }"
            >
              ID
              <span v-if="sortBy === 'id'" class="sort-indicator">
                {{ sortOrder === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th 
              @click="sortBy === 'number' ? toggleSortOrder() : setSortBy('number')"
              class="sortable"
              :class="{ active: sortBy === 'number' }"
            >
              Номер
              <span v-if="sortBy === 'number'" class="sort-indicator">
                {{ sortOrder === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th 
              @click="sortBy === 'date' ? toggleSortOrder() : setSortBy('date')"
              class="sortable"
              :class="{ active: sortBy === 'date' }"
            >
              Дата
              <span v-if="sortBy === 'date'" class="sort-indicator">
                {{ sortOrder === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th 
              @click="sortBy === 'amount' ? toggleSortOrder() : setSortBy('amount')"
              class="sortable"
              :class="{ active: sortBy === 'amount' }"
            >
              Сумма
              <span v-if="sortBy === 'amount'" class="sort-indicator">
                {{ sortOrder === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="master in masters" :key="master.id">
            <td>{{ master.id }}</td>
            <td>{{ master.number }}</td>
            <td>{{ formatDate(master.date) }}</td>
            <td>{{ formatCurrency(master.amount) }}</td>
            <td>
              <button 
                @click="openInfoModal(master)" 
                class="btn btn-sm btn-primary"
                style="margin-right: 0.5rem;"
              >
                ℹ️ Инфо
              </button>
              <button 
                @click="openEditModal(master)" 
                class="btn btn-sm btn-primary"
                style="margin-right: 0.5rem;"
              >
                ✏️ Изменить
              </button>
              <button 
                @click="deleteMaster(master.id)" 
                class="btn btn-sm btn-danger"
              >
                🗑️ Удалить
              </button>
            </td>
          </tr>
          <tr v-if="masters.length === 0">
            <td colspan="5" style="text-align: center; padding: 2rem; color: #666;">
              Документы не найдены
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

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click="closeModal">
      <div class="modal" @click.stop>
        <div class="modal-header">
          <h3 class="modal-title">
            {{ editingMaster ? 'Редактировать документ' : 'Создать документ' }}
          </h3>
          <button @click="closeModal" class="close-btn">&times;</button>
        </div>
        
        <form @submit.prevent="saveMaster">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Номер документа *</label>
              <input
                v-model="formData.number"
                type="text"
                class="form-control"
                required
                placeholder="DOC-001"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">Дата *</label>
              <input
                v-model="formData.date"
                type="date"
                class="form-control"
                required
              />
            </div>
          </div>
          
          <div class="form-group">
            <label class="form-label">Примечание</label>
            <textarea
              v-model="formData.note"
              class="form-control"
              rows="3"
              placeholder="Дополнительная информация..."
            ></textarea>
          </div>

          <!-- Details Section -->
          <div style="margin-top: 2rem;">
            <h4 style="margin-bottom: 1rem; display: flex; justify-content: space-between; align-items: center;">
              Спецификации
              <button 
                type="button"
                @click="addDetail" 
                class="btn btn-sm btn-success"
              >
                + Добавить
              </button>
            </h4>
            
            <div v-for="(detail, index) in formData.details" :key="detail.id || index" class="form-row" style="margin-bottom: 1rem; padding: 1rem; border: 1px solid #ddd; border-radius: 4px;">
              <div class="form-group">
                <label class="form-label">Название *</label>
                <input
                  v-model="detail.name"
                  type="text"
                  class="form-control"
                  required
                  placeholder="Название спецификации"
                />
              </div>
              
              <div class="form-group">
                <label class="form-label">Сумма *</label>
                <input
                  v-model.number="detail.amount"
                  type="number"
                  step="0.01"
                  min="0.01"
                  class="form-control"
                  required
                  placeholder="0.00"
                  @input="validateAmount(detail)"
                />
                <span v-if="detail.amountError" class="error-text" style="color: red; font-size: 0.875rem;">
                  Сумма должна быть больше 0
                </span>
              </div>
              
              <button 
                type="button"
                @click="removeDetail(index)" 
                class="btn btn-sm btn-danger"
                style="margin-top: 1.5rem; height: fit-content;"
              >
                🗑️
              </button>
            </div>
          </div>

          <div class="form-actions">
            <button type="button" @click="closeModal" class="btn btn-secondary">
              Отмена
            </button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Сохранение...' : 'Сохранить' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Details Modal -->
    <!-- Info Modal -->
    <div v-if="showInfoModal" class="modal-overlay" @click="closeInfoModal">
      <div class="modal" @click.stop>
        <div class="modal-header">
          <h3 class="modal-title">
            Информация о документе {{ selectedMaster?.number }}
          </h3>
          <button @click="closeInfoModal" class="close-btn">&times;</button>
        </div>
        
        <div v-if="selectedMaster" class="info-content">
          <div class="info-row">
            <label><strong>ID:</strong></label>
            <span>{{ selectedMaster.id }}</span>
          </div>
          
          <div class="info-row">
            <label><strong>Номер:</strong></label>
            <span>{{ selectedMaster.number }}</span>
          </div>
          
          <div class="info-row">
            <label><strong>Дата:</strong></label>
            <span>{{ formatDate(selectedMaster.date) }}</span>
          </div>
          
          <div class="info-row">
            <label><strong>Общая сумма:</strong></label>
            <span>{{ formatCurrency(selectedMaster.amount) }}</span>
          </div>
          
          <div class="info-row" v-if="selectedMaster.note">
            <label><strong>Примечание:</strong></label>
            <span>{{ selectedMaster.note }}</span>
          </div>
          
          <div class="info-row">
            <label><strong>Дата создания:</strong></label>
            <span>{{ formatDateTime(selectedMaster.createdAt) }}</span>
          </div>
          
          <div class="info-row" v-if="selectedMaster.updatedAt !== selectedMaster.createdAt">
            <label><strong>Последнее изменение:</strong></label>
            <span>{{ formatDateTime(selectedMaster.updatedAt) }}</span>
          </div>
          
          <!-- Specifications List -->
          <div v-if="selectedMaster.details?.length > 0" style="margin-top: 1.5rem;">
            <h4 style="margin-bottom: 1rem;">Спецификации:</h4>
            <table class="table" style="font-size: 0.9rem;">
              <thead>
                <tr>
                  <th>Название</th>
                  <th>Сумма</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="detail in selectedMaster.details" :key="detail.id">
                  <td>{{ detail.name }}</td>
                  <td>{{ detail.amount }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        
        <div class="form-actions">
          <button @click="closeInfoModal" class="btn btn-secondary">
            Закрыть
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mastersApi, detailsApi } from '../services/api.js'

export default {
  name: 'Documents',
  data() {
    return {
      masters: [],
      loading: false,
      error: null,
      saving: false,
      
      // Pagination
      currentPage: 1,
      pageSize: 10,
      totalCount: 0,
      
      // Search
      searchQuery: '',
      searchTimeout: null,
      sortBy: 'date',
      sortOrder: 'desc',
      
      // Modal
      showModal: false,
      showInfoModal: false,
      editingMaster: null,
      selectedMaster: null,
      
      // Form
      formData: {
        number: '',
        date: new Date().toISOString().split('T')[0],
        note: '',
        details: []
      }
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
    this.loadMasters()
  },
  
  methods: {
    async loadMasters() {
      this.loading = true
      this.error = null
      
      try {
        const response = await mastersApi.getAll(this.searchQuery, this.sortBy, this.sortOrder)
        
        if (Array.isArray(response.data)) {
          this.masters = response.data
          this.totalCount = response.data.length
        } else {
          this.masters = []
          this.totalCount = 0
        }
        
      } catch (error) {
        this.error = 'Ошибка загрузки документов: ' + (error.response?.data?.message || error.message)
        console.error('Load masters error:', error)
      } finally {
        this.loading = false
      }
    },
    
    handleSearch() {
      if (this.searchTimeout) {
        clearTimeout(this.searchTimeout)
      }
      
      this.searchTimeout = setTimeout(() => {
        this.currentPage = 1
        this.loadMasters()
      }, 500)
    },
    
    clearFilters() {
      this.searchQuery = ''
      this.sortBy = 'date'
      this.sortOrder = 'desc'
      this.currentPage = 1
      this.loadMasters()
    },
    
    setSortBy(field) {
      this.sortBy = field
      this.sortOrder = 'desc'
      this.currentPage = 1
      this.loadMasters()
    },
    
    toggleSortOrder() {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc'
      this.currentPage = 1
      this.loadMasters()
    },
    
    changePage(page) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page
        this.loadMasters()
      }
    },
    
    openCreateModal() {
      this.editingMaster = null
      this.formData = {
        number: '',
        date: new Date().toISOString().split('T')[0],
        note: '',
        details: []
      }
      this.showModal = true
    },
    
    async openEditModal(master) {
      this.editingMaster = master
      
      try {
        const response = await mastersApi.getById(master.id)
        const fullMaster = response.data
        
        this.formData = {
          number: fullMaster.number,
          date: fullMaster.date.split('T')[0],
          note: fullMaster.note || '',
          details: fullMaster.details ? fullMaster.details.map(d => ({
            id: d.id,
            name: d.name,
            amount: d.amount
          })) : []
        }
        
        this.showModal = true
      } catch (error) {
        this.error = 'Ошибка загрузки документа: ' + (error.response?.data?.message || error.message)
      }
    },
    
    closeModal() {
      this.showModal = false
      this.editingMaster = null
      this.formData = {
        number: '',
        date: new Date().toISOString().split('T')[0],
        note: '',
        details: []
      }
    },
    
    async openInfoModal(master) {
      try {
        const response = await mastersApi.getById(master.id)
        this.selectedMaster = response.data
        this.showInfoModal = true
      } catch (error) {
        this.error = 'Ошибка загрузки информации о документе: ' + (error.response?.data?.message || error.message)
      }
    },
    
    closeInfoModal() {
      this.showInfoModal = false
      this.selectedMaster = null
    },
    
    addDetail() {
      this.formData.details.push({
        name: '',
        amount: 0.01
      })
    },
    
    async removeDetail(index) {
      const detail = this.formData.details[index]
      
      if (detail.id) {
        try {
          await detailsApi.delete(detail.id)
        } catch (error) {
          this.error = 'Ошибка удаления спецификации: ' + (error.response?.data?.message || error.message)
          return
        }
      }
      
      this.formData.details.splice(index, 1)
    },
    
    validateAmount(detail) {
      if (detail.amount <= 0) {
        detail.amountError = true
      } else {
        detail.amountError = false
      }
    },
    
    async saveMaster() {
      this.saving = true
      this.error = null
      
      const hasInvalidAmounts = this.formData.details.some(detail => !detail.amount || detail.amount <= 0)
      if (hasInvalidAmounts) {
        this.error = 'Сумма в спецификациях должна быть больше 0'
        this.saving = false
        return
      }
      
      try {
        const masterData = {
          number: this.formData.number,
          date: this.formData.date,
          note: this.formData.note || null
        }
        
        if (this.editingMaster) {
          const existingDetails = this.formData.details.filter(detail => detail.id)
          const newDetails = this.formData.details.filter(detail => !detail.id)
          
          await mastersApi.update(this.editingMaster.id, masterData)
          
          for (const existingDetail of existingDetails) {
            await detailsApi.update(existingDetail.id, {
              name: existingDetail.name,
              amount: existingDetail.amount
            })
          }
          
          if (newDetails.length > 0) {
            const payload = {
              details: newDetails.map(detail => ({
                name: detail.name,
                amount: parseFloat(detail.amount) || 0
              }))
            }
            console.log('Creating details payload:', JSON.stringify(payload))
            await detailsApi.create(this.editingMaster.id, payload)
          }
        } else {
          await mastersApi.create({
            ...masterData,
            details: this.formData.details.map(detail => ({
              name: detail.name,
              amount: parseFloat(detail.amount) || 0
            }))
          })
        }
        
        this.closeModal()
        await this.loadMasters()
        
      } catch (error) {
        this.error = 'Ошибка сохранения: ' + (error.response?.data?.message || error.message)
        console.error('Save master error:', error)
      } finally {
        this.saving = false
      }
    },
    
    async deleteMaster(id) {
      if (!confirm('Вы уверены, что хотите удалить этот документ?')) {
        return
      }
      
      try {
        await mastersApi.delete(id)
        await this.loadMasters()
      } catch (error) {
        this.error = 'Ошибка удаления: ' + (error.response?.data?.message || error.message)
        console.error('Delete master error:', error)
      }
    },
    
    formatDate(dateString) {
      if (!dateString) return '-'
      return new Date(dateString).toLocaleDateString('ru-RU')
    },
    
    formatDateTime(dateString) {
      if (!dateString) return '-'
      return new Date(dateString).toLocaleString('ru-RU', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      })
    },
    
    formatCurrency(amount) {
      if (amount == null) return '-'
      return new Intl.NumberFormat('ru-RU', {
        style: 'currency',
        currency: 'RUB'
      }).format(amount)
    }
  }
}
</script>

<style scoped>
.info-content {
  max-height: 70vh;
  overflow-y: auto;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 0;
  border-bottom: 1px solid #eee;
}

.info-row:last-child {
  border-bottom: none;
}

.info-row label {
  color: #666;
  font-weight: 500;
  min-width: 180px;
}

.info-row span {
  text-align: right;
  flex: 1;
}

/* Sortable table headers */
.sortable {
  cursor: pointer;
  user-select: none;
  position: relative;
  padding-right: 20px !important;
}

.sortable:hover {
  background-color: #f8f9fa;
}

.sortable.active {
  background-color: #e9ecef;
}

.sort-indicator {
  position: absolute;
  right: 5px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 12px;
  color: #007bff;
}
</style>