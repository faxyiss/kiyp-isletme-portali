<template>
  <div v-if="isOpen" class="fixed inset-0 z-60 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-950/50 backdrop-blur-sm" @click="emit('close')"></div>

    <div
      class="bg-white rounded-xl shadow-2xl border border-slate-100 w-full max-w-sm overflow-hidden transform transition-all z-10"
    >
      <div
        class="flex items-center justify-between px-5 py-3.5 border-b border-slate-100 bg-slate-50/50"
      >
        <h4 class="text-sm font-bold text-slate-800 flex items-center gap-2">
          <FolderPlusIcon :size="16" class="text-blue-600" />
          Yeni Kategori Oluştur
        </h4>
        <button
          @click="emit('close')"
          class="p-1 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition-colors"
        >
          <XIcon :size="16" />
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="p-5 space-y-4">
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Kategori Adı</label
          >
          <input
            v-model="categoryName"
            type="text"
            required
            :disabled="isSaving"
            placeholder="Örn: Tekstil, Ambalaj, Kimya..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <div class="flex items-center justify-end gap-2.5 pt-2">
          <button
            type="button"
            @click="emit('close')"
            :disabled="isSaving"
            class="px-3 py-1.5 rounded-lg text-xs font-medium text-slate-600 hover:bg-slate-50 transition-colors disabled:opacity-50"
          >
            İptal
          </button>
          <button
            type="submit"
            :disabled="isSaving"
            class="px-4 py-1.5 rounded-lg text-xs font-medium bg-blue-600 hover:bg-blue-700 text-white shadow-sm transition-colors flex items-center gap-1 disabled:opacity-60"
          >
            {{ isSaving ? 'Kaydediliyor...' : 'Ekle' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { FolderPlusIcon, XIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const props = defineProps<{
  isOpen: boolean
}>()

const { showError } = useAlert()

const emit = defineEmits<{
  (e: 'close'): void
  // Artık sadece string değil, backend'den gelen tam kategori objesini fırlatıyoruz moruq!
  (e: 'add', category: any): void
}>()

const categoryName = ref('')
const isSaving = ref(false)
const BASE_URL = '/api'

watch(
  () => props.isOpen,
  (newVal) => {
    if (!newVal) categoryName.value = ''
  },
)

const handleSubmit = async () => {
  if (!categoryName.value.trim() || isSaving.value) return

  isSaving.value = true
  try {
    const token = localStorage.getItem('token')
    const response = await fetch(`${BASE_URL}/Categories/create`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: categoryName.value.trim(),
        type: 0, // CategoryType.Urun enum değerimiz
      }),
    })

    const data = await response.json()

    if (response.ok) {
      // Backend'den dönen kilitli { id, name, type } nesnesini fırlatıyoruz
      emit('add', data.category)
      emit('close')
    } else {
      showError(data.message || 'Kategori oluşturulurken bir hata oluştu.')
    }
  } catch (error) {
    console.error('Kategori API hatası:', error)
    showError('Sunucuyla bağlantı kurulamadı.')
  } finally {
    isSaving.value = false
  }
}
</script>
