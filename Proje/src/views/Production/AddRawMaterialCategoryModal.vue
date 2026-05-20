<template>
  <Teleport to="body">
  <div
    v-if="isOpen"
    class="fixed inset-0 z-[60] flex items-center justify-center bg-gray-950/50 backdrop-blur-sm p-4"
  >
    <div
      class="relative w-full max-w-md bg-white rounded-2xl shadow-2xl border border-gray-100 overflow-hidden"
    >
      <div class="flex items-center justify-between p-5 border-b border-gray-200 bg-gray-50/50">
        <div class="flex items-center space-x-2.5">
          <span class="text-xl">📁</span>
          <h3 class="text-lg font-bold text-gray-900">Yeni Hammadde Kategorisi</h3>
        </div>
        <button
          @click="closeModal"
          type="button"
          class="text-gray-400 bg-transparent hover:bg-gray-100 hover:text-gray-900 rounded-lg text-sm w-8 h-8 inline-flex justify-center items-center"
        >
          <svg
            class="w-3 h-3"
            aria-hidden="true"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 14 14"
          >
            <path
              stroke="currentColor"
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="m1 1 6 6m0 0 6 6M7 7l6-6M7 7l-6 6"
            />
          </svg>
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
        <div
          v-if="errorMessage"
          class="p-3 text-sm text-red-700 bg-red-50 border border-red-200 rounded-xl"
        >
          ⚠️ {{ errorMessage }}
        </div>

        <div>
          <label class="block mb-1.5 text-sm font-semibold text-gray-700">Kategori Adı</label>
          <input
            v-model="form.name"
            type="text"
            required
            class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5"
            placeholder="Örn: Ahşaplar, Kumaşlar, Metaller..."
          />
        </div>

        <div>
          <label class="block mb-1.5 text-sm font-semibold text-gray-700"
            >Açıklama (Opsiyonel)</label
          >
          <textarea
            v-model="form.description"
            rows="2"
            class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5"
            placeholder="Kategori açıklaması..."
          ></textarea>
        </div>

        <div class="flex items-center justify-end space-x-2 pt-4 border-t border-gray-100">
          <button
            @click="closeModal"
            type="button"
            class="px-4 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-200 rounded-xl hover:bg-gray-50"
          >
            Vazgeç
          </button>
          <button
            type="submit"
            :disabled="isSubmitting"
            class="px-5 py-2 text-sm font-medium text-white bg-blue-600 rounded-xl hover:bg-blue-700 disabled:opacity-50"
          >
            <span v-if="isSubmitting">Ekleniyor...</span>
            <span v-else>Kategoriyi Ekle</span>
          </button>
        </div>
      </form>
    </div>
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps({ isOpen: Boolean })
const emit = defineEmits(['close', 'success'])

const isSubmitting = ref(false)
const errorMessage = ref('')

const form = ref({
  name: '',
  description: '',
  type: 1,
})

watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      errorMessage.value = ''
      form.value = { name: '', description: '', type: 1 }
    }
  },
)

const handleSubmit = async () => {
  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const token = localStorage.getItem('token')
    // DİKKAT: IP ADRESİ GÜNCELLENDİ
    const response = await fetch('/api/Categories/create', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(form.value),
    })

    if (response.ok) {
      emit('success')
      closeModal()
    } else {
      const errData = await response.json()
      errorMessage.value = errData.message || 'Kategori eklenemedi.'
    }
  } catch (error) {
    errorMessage.value = 'Sunucu bağlantı hatası.'
  } finally {
    isSubmitting.value = false
  }
}

const closeModal = () => emit('close')
</script>
