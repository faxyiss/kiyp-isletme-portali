<template>
  <div v-if="isOpen" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>

    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-sm overflow-hidden transform transition-all z-10"
    >
      <div class="flex items-center justify-between px-6 py-4 border-b border-slate-100">
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <TagIcon :size="18" class="text-blue-600" />
          Yeni Gider Kategorisi
        </h3>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Kategori Adı</label
          >
          <input
            v-model="categoryName"
            type="text"
            required
            :disabled="isLoading"
            placeholder="Örn: Kira, Faturalar, Maaşlar..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-100 mt-2">
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
            :disabled="isLoading || !categoryName.trim()"
            class="px-5 py-2.5 rounded-xl text-sm font-medium bg-blue-600 hover:bg-blue-700 text-white shadow-sm shadow-blue-200 transition-colors flex items-center gap-1.5 disabled:opacity-60"
          >
            <CheckIcon v-if="!isLoading" :size="15" />
            {{ isLoading ? 'Kaydediliyor...' : 'Kategori Ekle' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { TagIcon, XIcon, CheckIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const props = defineProps<{
  isOpen: boolean
}>()

const { showError } = useAlert()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success', category: { id: string; name: string }): void
}>()

const BASE_URL = '/api'
const isLoading = ref(false)
const categoryName = ref('')

watch(
  () => props.isOpen,
  (val) => {
    if (!val) categoryName.value = ''
  },
)

const handleSubmit = async () => {
  if (!categoryName.value.trim() || isLoading.value) return

  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const response = await fetch(`${BASE_URL}/Expenses/categories`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ name: categoryName.value.trim() }),
    })

    const data = await response.json()

    if (response.ok) {
      emit('success', data.category)
      emit('close')
    } else {
      showError(data.message || 'Kategori eklenirken hata oluştu.')
    }
  } catch {
    showError('Sunucuyla bağlantı kurulamadı.')
  } finally {
    isLoading.value = false
  }
}
</script>
