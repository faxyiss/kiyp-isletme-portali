<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>

    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-lg overflow-hidden transform transition-all z-10 max-h-[90vh] flex flex-col"
    >
      <!-- Header -->
      <div
        class="flex items-center justify-between px-6 py-4 border-b border-slate-100 flex-shrink-0"
      >
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <ReceiptIcon :size="18" class="text-blue-600" />
          Yeni Gider Ekle
        </h3>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <!-- Form -->
      <form
        @submit.prevent="handleSubmit"
        class="p-6 space-y-4 overflow-y-auto custom-scrollbar flex-1"
      >
        <!-- Başlık -->
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Gider Başlığı</label
          >
          <input
            v-model="form.title"
            type="text"
            required
            :disabled="isLoading"
            placeholder="Örn: Ocak Kirası, Elektrik Faturası..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <!-- Kategori -->
        <div>
          <div class="flex items-center justify-between mb-1.5">
            <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide"
              >Kategori</label
            >
            <button
              type="button"
              @click="emit('open-category-modal')"
              :disabled="isLoading"
              class="text-xs font-medium text-blue-600 hover:text-blue-700 flex items-center gap-1 transition-colors disabled:opacity-50"
            >
              <PlusIcon :size="14" />
              Yeni Kategori
            </button>
          </div>

          <div class="relative">
            <button
              @click="isCategoryDropdownOpen = !isCategoryDropdownOpen"
              type="button"
              :disabled="isLoading"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20 disabled:opacity-60"
            >
              <span class="truncate font-medium" :class="{ 'text-slate-400': !form.categoryId }">
                {{ selectedCategoryName }}
              </span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200 flex-shrink-0"
                :class="{ 'rotate-180': isCategoryDropdownOpen }"
                :size="16"
              />
            </button>

            <div
              v-if="isCategoryDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isCategoryDropdownOpen = false"
            ></div>

            <div
              v-if="isCategoryDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-2 space-y-2 max-h-52 flex flex-col"
            >
              <div class="relative flex-shrink-0">
                <SearchIcon
                  class="absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400"
                  :size="14"
                />
                <input
                  v-model="categorySearch"
                  type="text"
                  placeholder="Kategori ara..."
                  class="w-full pl-8 pr-3 py-1.5 border border-slate-200 rounded-lg focus:outline-none focus:border-blue-500 text-xs transition-all"
                  @click.stop
                />
              </div>
              <div class="overflow-y-auto flex-1 space-y-0.5 custom-scrollbar">
                <button
                  v-for="cat in filteredCategories"
                  :key="cat.id"
                  @click="selectCategory(cat)"
                  type="button"
                  class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors truncate"
                  :class="
                    form.categoryId === cat.id
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

        <!-- Tutar -->
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Tutar (₺)</label
          >
          <input
            v-model.number="form.amount"
            type="number"
            min="0.01"
            step="0.01"
            required
            :disabled="isLoading"
            placeholder="0,00"
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <!-- Gider Tipi -->
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-2"
            >Gider Tipi</label
          >
          <div class="grid grid-cols-3 gap-2">
            <button
              v-for="type in expenseTypes"
              :key="type.value"
              type="button"
              :disabled="isLoading"
              @click="selectExpenseType(type.value)"
              class="flex flex-col items-center gap-1.5 px-3 py-3 rounded-xl border text-xs font-semibold transition-all"
              :class="
                form.expenseType === type.value
                  ? 'bg-blue-50 border-blue-400 text-blue-700'
                  : 'border-slate-200 text-slate-500 hover:border-slate-300 hover:bg-slate-50'
              "
            >
              <component :is="type.icon" :size="16" />
              {{ type.label }}
            </button>
          </div>
          <p class="text-xs text-slate-400 mt-1.5">{{ expenseTypeDescription }}</p>
        </div>

        <!-- Başlangıç Tarihi -->
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5">
            {{ startDateLabel }}
          </label>
          <input
            v-model="form.startDate"
            type="date"
            required
            :disabled="isLoading"
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <!-- Bitiş Tarihi — Sadece Dönemsel -->
        <div v-if="form.expenseType === 2">
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Bitiş Tarihi</label
          >
          <input
            v-model="form.endDate"
            type="date"
            :required="form.expenseType === 2"
            :disabled="isLoading"
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <!-- Tekrar Günü — Sadece Aylık -->
        <div v-if="form.expenseType === 1">
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Ayın Kaçıncı Günü?</label
          >
          <input
            v-model.number="form.recurringDay"
            type="number"
            min="1"
            max="31"
            :required="form.expenseType === 1"
            :disabled="isLoading"
            placeholder="Örn: 1, 15, 30..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <!-- Notlar -->
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Notlar (Opsiyonel)</label
          >
          <textarea
            v-model="form.notes"
            rows="2"
            :disabled="isLoading"
            placeholder="Gidere dair ek not..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60 resize-none"
          ></textarea>
        </div>
      </form>

      <!-- Footer -->
      <div
        class="flex items-center justify-end gap-3 px-6 py-4 border-t border-slate-100 flex-shrink-0"
      >
        <button
          type="button"
          @click="emit('close')"
          :disabled="isLoading"
          class="px-4 py-2 rounded-xl text-sm font-medium text-slate-600 hover:bg-slate-50 transition-colors disabled:opacity-50"
        >
          İptal
        </button>
        <button
          @click="handleSubmit"
          :disabled="isLoading || !form.categoryId || !form.title || !form.amount"
          class="px-5 py-2.5 rounded-xl text-sm font-medium bg-blue-600 hover:bg-blue-700 text-white shadow-sm shadow-blue-200 transition-colors flex items-center gap-1.5 disabled:opacity-60"
        >
          <CheckIcon v-if="!isLoading" :size="15" />
          {{ isLoading ? 'Kaydediliyor...' : 'Gideri Kaydet' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useAlert } from '@/composables/useAlert'
import {
  ReceiptIcon,
  XIcon,
  PlusIcon,
  ChevronDownIcon,
  SearchIcon,
  CheckIcon,
  ZapIcon,
  RepeatIcon,
  CalendarRangeIcon,
} from 'lucide-vue-next'

const { showError } = useAlert()

const props = defineProps<{
  isOpen: boolean
  categories: { id: string; name: string }[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void
  (e: 'open-category-modal'): void
}>()

const BASE_URL = '/api'
const isLoading = ref(false)
const isCategoryDropdownOpen = ref(false)
const categorySearch = ref('')

const defaultForm = () => ({
  title: '',
  categoryId: '',
  categoryName: '',
  amount: null as number | null,
  expenseType: 0 as 0 | 1 | 2,
  startDate: new Date().toISOString().slice(0, 10),
  endDate: '',
  recurringDay: null as number | null,
  notes: '',
})

const form = ref(defaultForm())

const expenseTypes: { value: 0 | 1 | 2; label: string; icon: unknown }[] = [
  { value: 0, label: 'Tek Seferlik', icon: ZapIcon },
  { value: 1, label: 'Aylık Sabit', icon: RepeatIcon },
  { value: 2, label: 'Dönemsel', icon: CalendarRangeIcon },
]

const expenseTypeDescription = computed(() => {
  if (form.value.expenseType === 0) return 'Bir kez gerçekleşen gider (ekipman, tamir vb.)'
  if (form.value.expenseType === 1) return 'Her ay belirli bir günde tekrarlayan gider (kira vb.)'
  return 'Belirli bir dönem aralığında geçerli gider'
})

const startDateLabel = computed(() => {
  if (form.value.expenseType === 0) return 'İşlem Tarihi'
  if (form.value.expenseType === 1) return 'Başlangıç Ayı (Referans Tarih)'
  return 'Dönem Başlangıç Tarihi'
})

const selectedCategoryName = computed(() => {
  if (!form.value.categoryId) return 'Kategori Seçiniz...'
  return form.value.categoryName || 'Kategori Seçiniz...'
})

const filteredCategories = computed(() => {
  const s = categorySearch.value.trim().toLowerCase()
  if (!s) return props.categories
  return props.categories.filter((c) => c.name.toLowerCase().includes(s))
})

const selectCategory = (cat: { id: string; name: string }) => {
  form.value.categoryId = cat.id
  form.value.categoryName = cat.name
  isCategoryDropdownOpen.value = false
  categorySearch.value = ''
}

const selectExpenseType = (type: 0 | 1 | 2) => {
  form.value.expenseType = type
  form.value.endDate = ''
  form.value.recurringDay = null
}

watch(
  () => props.isOpen,
  (val) => {
    if (!val) {
      form.value = defaultForm()
      isCategoryDropdownOpen.value = false
      categorySearch.value = ''
    }
  },
)

const handleSubmit = async () => {
  if (!form.value.categoryId || !form.value.title || !form.value.amount || isLoading.value) return

  isLoading.value = true
  try {
    const token = localStorage.getItem('token')

    const body: Record<string, unknown> = {
      categoryId: form.value.categoryId,
      title: form.value.title,
      amount: form.value.amount,
      expenseType: form.value.expenseType,
      startDate: new Date(form.value.startDate).toISOString(),
      notes: form.value.notes || null,
    }

    if (form.value.expenseType === 2 && form.value.endDate) {
      body.endDate = new Date(form.value.endDate).toISOString()
    }
    if (form.value.expenseType === 1 && form.value.recurringDay) {
      body.recurringDay = form.value.recurringDay
    }

    const response = await fetch(`${BASE_URL}/Expenses`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(body),
    })

    const data = await response.json()

    if (response.ok) {
      emit('success')
      emit('close')
    } else {
      showError(data.message || 'Gider kaydedilirken hata oluştu.')
    }
  } catch {
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
