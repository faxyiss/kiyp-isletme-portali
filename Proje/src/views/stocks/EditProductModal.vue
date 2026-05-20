<template>
  <Teleport to="body">
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>

    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-md overflow-hidden transform transition-all z-10"
    >
      <div class="flex items-center justify-between px-6 py-4 border-b border-slate-100">
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <EditIcon :size="18" class="text-amber-500" />
          Ürün Bilgilerini Güncelle (#{{ product?.productNo }})
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
            >Ürün Adı</label
          >
          <input
            v-model="productForm.name"
            type="text"
            required
            :disabled="isLoading"
            placeholder="Ürün adı giriniz..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Kategori</label
          >
          <div class="relative w-full">
            <button
              @click="isDropdownOpen = !isDropdownOpen"
              type="button"
              :disabled="isLoading"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20 disabled:opacity-60"
            >
              <span class="truncate font-medium">
                {{ selectedCategoryName }}
              </span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isDropdownOpen }"
                :size="16"
              />
            </button>

            <div
              v-if="isDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isDropdownOpen = false"
            ></div>

            <div
              v-if="isDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-2 space-y-2 max-h-60 flex flex-col"
            >
              <div class="relative flex-shrink-0">
                <SearchIcon
                  class="absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400"
                  :size="14"
                />
                <input
                  v-model="categorySearchText"
                  type="text"
                  placeholder="Kategori ara..."
                  class="w-full pl-8 pr-3 py-1.5 border border-slate-200 rounded-lg focus:outline-none focus:border-blue-500 text-xs transition-all"
                  @click.stop
                />
              </div>

              <div class="overflow-y-auto flex-1 space-y-0.5 max-h-40 custom-scrollbar">
                <button
                  v-for="cat in filteredCategories"
                  :key="cat.id"
                  @click="selectCategory(cat.id)"
                  type="button"
                  class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors truncate"
                  :class="
                    productForm.categoryId === cat.id
                      ? 'bg-blue-50 text-blue-600 font-bold'
                      : 'text-slate-600 hover:bg-slate-50'
                  "
                >
                  {{ cat.name }}
                </button>
                <div
                  v-if="filteredCategories.length === 0"
                  class="text-center py-3 text-xs text-slate-400"
                >
                  Kategori bulunamadı.
                </div>
              </div>
            </div>
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Ürün Açıklaması (Opsiyonel)</label
          >
          <textarea
            v-model="productForm.description"
            rows="3"
            :disabled="isLoading"
            placeholder="Ürüne dair kısa bir not yazın..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          ></textarea>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Satış Fiyatı (₺)</label
          >
          <input
            v-model.number="productForm.unitPrice"
            type="number"
            min="0"
            step="0.01"
            required
            :disabled="isLoading"
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
            class="px-5 py-2.5 rounded-xl text-sm font-medium bg-amber-500 hover:bg-amber-600 text-white shadow-sm transition-colors flex items-center gap-1 disabled:opacity-60"
          >
            {{ isLoading ? 'Güncelleniyor...' : 'Değişiklikleri Kaydet' }}
          </button>
        </div>
      </form>
    </div>
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { EditIcon, XIcon, ChevronDownIcon, SearchIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const props = defineProps<{
  isOpen: boolean
  categories: any[]
  product: any // Düzenlenecek olan ham ürün objesi
}>()

const { showSuccess, showError } = useAlert()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void
}>()

const isDropdownOpen = ref(false)
const categorySearchText = ref('')
const isLoading = ref(false)
const BASE_URL = '/api'

// Backend ProductUpdateDto yapısıyla birebir uyumlu form şeması
const productForm = ref({
  name: '',
  categoryId: '',
  description: '',
  unitPrice: 0,
})

// Modal her açıldığında seçilen satırdaki ürün bilgilerini forma prefill (ön yükleme) yapıyoruz moruq
watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal && props.product) {
      productForm.value = {
        name: props.product.name,
        // Dikkat: Bizim backend listelemede categoryName dönüyor, seçimi eşleştirmek için categories içinden id'sini cımbızlıyoruz
        categoryId: props.categories.find((c) => c.name === props.product.categoryName)?.id || '',
        description: props.product.description || '',
        unitPrice: props.product.unitPrice,
      }
    } else {
      isDropdownOpen.value = false
      categorySearchText.value = ''
    }
  },
)

const selectedCategoryName = computed(() => {
  if (!productForm.value.categoryId) return 'Kategori Seçiniz...'
  const found = props.categories.find((c) => c.id === productForm.value.categoryId)
  return found ? found.name : 'Kategori Seçiniz...'
})

const filteredCategories = computed(() => {
  const search = categorySearchText.value.trim().toLowerCase()
  if (!search) return props.categories
  return props.categories.filter((c) => c.name.toLowerCase().includes(search))
})

const selectCategory = (catId: string) => {
  productForm.value.categoryId = catId
  isDropdownOpen.value = false
  categorySearchText.value = ''
}

const handleSubmit = async () => {
  if (!productForm.value.categoryId || isLoading.value) return

  isLoading.value = true
  try {
    const token = localStorage.getItem('token')

    // 🎯 Backend Güncelleme Endpoint'i: PUT /api/Products/update/{id}
    const response = await fetch(`${BASE_URL}/Products/update/${props.product.id}`, {
      method: 'PUT',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: productForm.value.name,
        description: productForm.value.description,
        categoryId: productForm.value.categoryId,
        unitPrice: productForm.value.unitPrice,
      }),
    })

    const data = await response.json()

    if (response.ok) {
      showSuccess('Ürün bilgileri başarıyla güncellendi.')
      emit('success')
      emit('close')
    } else {
      showError(data.message || 'Güncelleme sırasında bir hata oluştu.')
    }
  } catch (error) {
    console.error('Ürün güncelleme hatası:', error)
    showError('Sunucuyla bağlantı kurulamadı.')
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: #f1f5f9;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
</style>
