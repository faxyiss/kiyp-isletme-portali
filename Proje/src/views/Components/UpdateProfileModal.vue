<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>

    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-md overflow-hidden transform transition-all z-10"
    >
      <div
        class="flex items-center justify-between px-6 py-4 border-b border-slate-100 bg-slate-50/50"
      >
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <UserIcon :size="18" class="text-blue-600" />
          Profil Bilgilerini Güncelle
        </h3>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Ad Soyad</label
          >
          <input
            v-model="formData.fullName"
            type="text"
            required
            :disabled="isLoading"
            placeholder="Adınızı ve soyadınızı girin..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Şirket / İşletme Adı</label
          >
          <input
            v-model="formData.companyName"
            type="text"
            required
            :disabled="isLoading"
            placeholder="Şirket adını girin..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-100 mt-6">
          <button
            type="button"
            @click="emit('close')"
            :disabled="isLoading"
            class="px-4 py-2 rounded-xl text-sm font-medium text-slate-600 hover:bg-slate-50 transition-colors disabled:opacity-50"
          >
            İptal
          </button>
          <button
            type="submit"
            :disabled="isLoading"
            class="px-5 py-2.5 rounded-xl text-sm font-medium bg-blue-600 hover:bg-blue-700 text-white shadow-sm shadow-blue-200 transition-colors flex items-center gap-1 disabled:opacity-60"
          >
            {{ isLoading ? 'Güncelleniyor...' : 'Bilgileri Kaydet' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { UserIcon, XIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const props = defineProps<{
  isOpen: boolean
  currentData: { fullName: string; companyName: string }
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void // Güncelleme başarılı olunca Layout'u yeniletmek için
}>()

const { showSuccess, showError } = useAlert()

const isLoading = ref(false)
const BASE_URL = '/api'

const formData = ref({
  fullName: '',
  companyName: '',
})

// Modal açıldığında mevcut kullanıcı verilerini otomatik doldur moruq
watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal && props.currentData) {
      formData.value = {
        fullName: props.currentData.fullName,
        companyName: props.currentData.companyName,
      }
    }
  },
)

const handleSubmit = async () => {
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')

    // Backend tarafındaki BusinessProfile güncelleme endpoint'ine PUT isteği atıyoruz
    const response = await fetch(`${BASE_URL}/BusinessProfile/update`, {
      method: 'PUT',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(formData.value),
    })

    if (response.ok) {
      showSuccess('Profil bilgileriniz başarıyla güncellendi.')
      emit('success')
      emit('close')
    } else {
      const err = await response.json()
      showError(err.message || 'Güncelleme sırasında bir hata oluştu.')
    }
  } catch (error) {
    console.error('Profil güncelleme hatası:', error)
    showError('Sunucuya bağlanılamadı.')
  } finally {
    isLoading.value = false
  }
}
</script>
