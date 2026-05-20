<template>
  <Teleport to="body">
  <div
    v-if="isOpen"
    class="fixed inset-0 z-50 flex items-center justify-center bg-gray-950/50 backdrop-blur-sm p-4 animate-fade-in"
  >
    <div
      class="relative w-full max-w-lg bg-white rounded-2xl shadow-2xl border border-gray-100 overflow-hidden transform transition-all scale-100"
    >
      <div class="flex items-center justify-between p-5 border-b border-gray-200 bg-gray-50/50">
        <div class="flex items-center space-x-2.5">
          <span class="text-xl">➕</span>
          <h3 class="text-lg font-bold text-gray-900">Yeni Hammadde Tanımla</h3>
        </div>
        <button
          @click="closeModal"
          type="button"
          class="text-gray-400 bg-transparent hover:bg-gray-100 hover:text-gray-900 rounded-lg text-sm w-8 h-8 inline-flex justify-center items-center transition-colors"
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

      <form @submit.prevent="handleSubmit" class="p-6 space-y-4" @click="isDropdownOpen = false">
        <div
          v-if="errorMessage"
          class="p-3 text-sm text-red-700 bg-red-50 border border-red-200 rounded-xl"
          role="alert"
        >
          ⚠️ {{ errorMessage }}
        </div>

        <div class="grid gap-4 grid-cols-1 sm:grid-cols-2">
          <div class="sm:col-span-2">
            <label class="block mb-1.5 text-sm font-semibold text-gray-700">Hammadde Adı</label>
            <input
              v-model="form.name"
              type="text"
              required
              class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 block w-full p-2.5 transition-all focus:outline-none"
              placeholder="Örn: Meşe Kereste, Çelik Profil vb."
            />
          </div>

          <div class="sm:col-span-2">
            <label class="block mb-1.5 text-sm font-semibold text-gray-700"
              >Hammadde Kategorisi</label
            >

            <div class="relative w-full" @click.stop>
              <button
                @click="isDropdownOpen = !isDropdownOpen"
                type="button"
                class="w-full flex items-center justify-between bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl p-2.5 hover:border-gray-400 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500"
              >
                <span class="truncate font-medium" :class="{ 'text-gray-400': !form.categoryId }">
                  {{ selectedCategoryName }}
                </span>
                <ChevronDownIcon
                  class="text-gray-400 transition-transform duration-200"
                  :class="{ 'rotate-180': isDropdownOpen }"
                  :size="16"
                />
              </button>

              <div
                v-if="isDropdownOpen"
                class="absolute top-full left-0 mt-1.5 w-full bg-white border border-gray-200 rounded-xl shadow-xl z-50 p-2 flex flex-col gap-2 max-h-60 origin-top animate-fade-in"
              >
                <div class="relative flex-shrink-0">
                  <SearchIcon
                    class="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400"
                    :size="14"
                  />
                  <input
                    v-model="categorySearchText"
                    type="text"
                    placeholder="Kategori ara..."
                    class="w-full pl-8 pr-3 py-1.5 bg-gray-50 border border-gray-200 rounded-lg focus:outline-none focus:border-blue-500 text-xs transition-all"
                  />
                </div>

                <div class="overflow-y-auto flex-1 p-0.5 space-y-0.5 max-h-40 custom-scrollbar">
                  <button
                    v-for="cat in filteredCategories"
                    :key="cat.id"
                    @click="selectCategory(cat.id)"
                    type="button"
                    class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-semibold transition-colors flex items-center justify-between truncate"
                    :class="
                      form.categoryId === cat.id
                        ? 'bg-blue-50 text-blue-600 font-bold'
                        : 'text-gray-600 hover:bg-gray-50'
                    "
                  >
                    <span>{{ cat.name }}</span>
                    <CheckIcon
                      v-if="form.categoryId === cat.id"
                      :size="12"
                      class="text-blue-600 flex-shrink-0"
                    />
                  </button>

                  <div
                    v-if="filteredCategories.length === 0"
                    class="text-center py-4 text-xs text-gray-400 font-medium"
                  >
                    Eşleşen kategori bulunamadı.
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="sm:col-span-2">
            <label class="block mb-1.5 text-sm font-semibold text-gray-700"
              >Birim Alış Fiyatı (₺)</label
            >
            <input
              v-model="form.unitPrice"
              type="number"
              step="0.01"
              min="0"
              required
              class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 block w-full p-2.5 transition-all focus:outline-none"
              placeholder="0.00"
            />
          </div>

          <div class="sm:col-span-2">
            <label class="block mb-1.5 text-sm font-semibold text-gray-700"
              >Açıklama / Notlar</label
            >
            <textarea
              v-model="form.description"
              rows="3"
              class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 block w-full p-2.5 transition-all focus:outline-none animate-none"
              placeholder="Hammaddeye ait teknik detaylar veya notlar..."
            ></textarea>
          </div>
        </div>

        <div class="flex items-center justify-end space-x-2 pt-4 border-t border-gray-100">
          <button
            @click="closeModal"
            type="button"
            class="px-4 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors"
          >
            Vazgeç
          </button>
          <button
            type="submit"
            :disabled="isSubmitting"
            class="px-5 py-2 text-sm font-medium text-white bg-blue-600 rounded-xl hover:bg-blue-700 focus:ring-4 focus:outline-none focus:ring-blue-200 disabled:opacity-50 transition-colors"
          >
            <span v-if="isSubmitting">Kaydediliyor...</span>
            <span v-else>Hammaddeyi Kaydet</span>
          </button>
        </div>
      </form>
    </div>
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { ChevronDownIcon, SearchIcon, CheckIcon } from 'lucide-vue-next'

const props = defineProps({ isOpen: Boolean })
const emit = defineEmits(['close', 'success'])

const isSubmitting = ref(false)
const errorMessage = ref('')
const hammaddeCategories = ref<any[]>([])

// Dropdown Arama Reaktif Durumları
const isDropdownOpen = ref(false)
const categorySearchText = ref('')

const form = ref({
  name: '',
  description: '',
  unitPrice: 0,
  categoryId: '',
})

// Seçilen Kategori İsmini Butonun İçinde Gösteren Hesaplama
const selectedCategoryName = computed(() => {
  if (!form.value.categoryId) return 'Kategori Seçiniz...'
  const found = hammaddeCategories.value.find((c) => c.id === form.value.categoryId)
  return found ? found.name : 'Kategori Seçiniz...'
})

// Yazılan Karakterlere Göre Kategori Listesini Süzme Mantığı
const filteredCategories = computed(() => {
  const query = categorySearchText.value.trim().toLowerCase()
  if (!query) return hammaddeCategories.value
  return hammaddeCategories.value.filter((c) => c.name.toLowerCase().includes(query))
})

const fetchHammaddeCategories = async () => {
  try {
    const token = localStorage.getItem('token')
    const response = await fetch('/api/Categories', {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (response.ok) {
      const data = await response.json()
      hammaddeCategories.value = data.filter((c: any) => c.type === 1)
    }
  } catch (error) {
    console.error('Kategoriler çekilemedi:', error)
  }
}

// Listeden Seçim Yapıldığında Tetiklenen Fonksiyon
const selectCategory = (id: string) => {
  form.value.categoryId = id
  isDropdownOpen.value = false
  categorySearchText.value = ''
}

watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      errorMessage.value = ''
      isDropdownOpen.value = false
      categorySearchText.value = ''
      form.value = { name: '', description: '', unitPrice: 0, categoryId: '' }
      fetchHammaddeCategories()
    }
  },
)

const handleSubmit = async () => {
  if (!form.value.categoryId) {
    errorMessage.value = 'Lütfen bir kategori seçiniz.'
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const token = localStorage.getItem('token')
    const response = await fetch('/api/Products/create', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(form.value),
    })

    if (response.ok) {
      emit('success')
    } else {
      const errData = await response.json()
      errorMessage.value = errData.message || 'Kayıt sırasında hata oluştu.'
    }
  } catch (error) {
    errorMessage.value = 'Sunucu bağlantı hatası.'
  } finally {
    isSubmitting.value = false
  }
}

const closeModal = () => emit('close')
</script>

<style scoped>
/* Özel mini kaydırma çubuğu ambalajı */
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

/* Modal kutu içi animasyonu */
.animate-fade-in {
  animation: fadeIn 0.12s ease-out forwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-4px) scale(0.99);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
</style>
