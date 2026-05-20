import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './assets/main.css'

import App from './App.vue'
import router from './router'
import VueApexCharts from 'vue3-apexcharts' // 1. EKLENEN SATIR
import axios from 'axios' // Bunu Ekle
axios.defaults.baseURL = import.meta.env.VITE_API_BASE_URL

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(VueApexCharts) // 2. EKLENEN SATIR
app.mount('#app')
