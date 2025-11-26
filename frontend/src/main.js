import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue'
import Documents from './components/Documents.vue'
import Logs from './components/Logs.vue'
import './style.css'

const routes = [
  { path: '/', redirect: '/documents' },
  { path: '/documents', component: Documents },
  { path: '/logs', component: Logs }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

const app = createApp(App)
app.use(router)
app.mount('#app')