<template>
  <div class="space-y-5 animate-fade-in">
    <!-- Başlık + Arama + Filtreler -->
    <div class="flex flex-col gap-4 p-5 bg-white rounded-2xl border border-slate-100 shadow-sm">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-3">
        <div class="flex items-center gap-3">
          <div class="bg-blue-600/10 p-2.5 rounded-xl">
            <ClipboardListIcon class="text-blue-600" :size="22" />
          </div>
          <div>
            <h2 class="text-xl font-bold text-slate-800">Reçeteler ve Üretim Emirleri</h2>
            <p class="text-sm text-slate-500 mt-0.5">
              Nihai ürünlerinizin üretim formüllerini yönetin.
            </p>
          </div>
        </div>
        <button
          @click="isAddRecipeModalOpen = true"
          class="flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-xl font-medium transition-colors text-sm shadow-sm shadow-blue-200 whitespace-nowrap"
        >
          <PlusIcon :size="16" /> Yeni Reçete Ekle
        </button>
      </div>

      <!-- Filtre Satırı -->
      <div class="flex flex-wrap gap-3">
        <!-- Ürün Adı Ara -->
        <div class="relative flex-1 min-w-[200px]">
          <SearchIcon class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" :size="16" />
          <input
            v-model="searchText"
            @input="handleSearch"
            type="text"
            placeholder="Ürün adı ile ara..."
            class="w-full pl-9 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all"
          />
        </div>

        <!-- Hammadde Sayısı Filtresi -->
        <CustomDropdown
          v-model="filterIngredientCount"
          :options="ingredientCountOptions"
          width-class="w-48"
          @update:model-value="applyFilters"
        />

        <!-- Sıralama -->
        <CustomDropdown
          v-model="sortBy"
          :options="sortOptions"
          width-class="w-52"
          @update:model-value="applyFilters"
        />

        <!-- Filtre temizle -->
        <button
          v-if="searchText || filterIngredientCount"
          @click="resetFilters"
          class="py-2.5 px-3.5 bg-slate-100 hover:bg-slate-200 border border-slate-200 rounded-xl text-sm font-bold text-slate-500 transition-all flex items-center gap-1.5"
        >
          <RotateCcwIcon :size="13" /> Sıfırla
        </button>
      </div>

      <!-- Sonuç sayısı -->
      <div
        v-if="searchText || filterIngredientCount"
        class="text-xs text-slate-500 font-medium -mt-1"
      >
        <span class="font-bold text-slate-700">{{ filteredRecipes.length }}</span> reçete bulundu
      </div>
    </div>

    <!-- Tablo -->
    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
      <div class="overflow-x-auto custom-scrollbar">
        <table class="w-full text-left border-collapse table-auto">
          <thead>
            <tr
              class="bg-slate-50/60 border-b border-slate-100 text-slate-500 text-[11px] font-bold uppercase tracking-wider"
            >
              <th class="py-4 px-6">Nihai Ürün</th>
              <th class="py-4 px-6">Kullanılan Hammaddeler</th>
              <th class="py-4 px-6 text-center">Hammadde Sayısı</th>
              <th class="py-4 px-6 text-center">Toplam Miktar</th>
              <th class="py-4 px-6">Oluşturulma</th>
              <th class="py-4 px-6 text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-slate-700 text-sm">
            <tr v-if="isLoading">
              <td colspan="6" class="py-12 text-center text-slate-400 font-medium">
                <div class="flex items-center justify-center gap-2">
                  <div
                    class="animate-spin rounded-full h-5 w-5 border-2 border-blue-600 border-t-transparent"
                  ></div>
                  Reçeteler yükleniyor...
                </div>
              </td>
            </tr>
            <tr v-else-if="filteredRecipes.length === 0">
              <td colspan="6" class="py-16 text-center">
                <div class="flex flex-col items-center justify-center text-gray-400">
                  <ClipboardListIcon :size="48" class="mb-3 text-gray-300" />
                  <p class="font-medium text-gray-600">
                    {{
                      searchText || filterIngredientCount
                        ? 'Arama sonucu bulunamadı.'
                        : 'Henüz hiç reçete tanımlanmamış.'
                    }}
                  </p>
                  <p class="text-xs mt-1 text-gray-400">
                    {{
                      searchText || filterIngredientCount
                        ? 'Farklı filtreler deneyin.'
                        : 'Üretime başlamak için yeni bir reçete ekleyin.'
                    }}
                  </p>
                </div>
              </td>
            </tr>

            <tr
              v-else
              v-for="recipe in filteredRecipes"
              :key="recipe.id"
              class="hover:bg-slate-50/40 transition-colors"
            >
              <!-- Ürün Adı -->
              <td class="py-4 px-6 align-middle">
                <div class="flex items-center gap-3">
                  <div
                    class="w-10 h-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center shrink-0"
                  >
                    <PackageIcon :size="20" />
                  </div>
                  <span class="font-bold text-gray-900 text-base">{{ recipe.productName }}</span>
                </div>
              </td>

              <!-- Hammaddeler -->
              <td class="py-4 px-6 align-middle max-w-xs">
                <div class="flex flex-wrap gap-1.5">
                  <span
                    v-for="(item, idx) in recipe.items"
                    :key="idx"
                    class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-semibold bg-gray-100 text-gray-700 border border-gray-200"
                  >
                    <LayersIcon :size="11" class="text-gray-500 shrink-0" />
                    <span class="truncate max-w-[100px]">{{ item.rawMaterialName }}</span>
                    <span
                      class="text-blue-600 bg-white px-1.5 rounded-md shadow-sm border border-gray-100 shrink-0"
                    >
                      {{ item.requiredQuantity }}
                    </span>
                  </span>
                </div>
              </td>

              <!-- Hammadde Sayısı -->
              <td class="py-4 px-6 align-middle text-center">
                <span
                  class="inline-flex items-center justify-center w-8 h-8 rounded-full bg-slate-100 text-slate-700 text-xs font-extrabold border border-slate-200"
                >
                  {{ recipe.items.length }}
                </span>
              </td>

              <!-- Toplam Miktar -->
              <td class="py-4 px-6 align-middle text-center">
                <span class="text-xs font-bold text-slate-600">
                  {{
                    recipe.items.reduce((s: number, i: any) => s + i.requiredQuantity, 0).toFixed(2)
                  }}
                  Ad
                </span>
              </td>

              <!-- Tarih -->
              <td
                class="py-4 px-6 align-middle whitespace-nowrap text-slate-500 font-medium text-xs"
              >
                {{ new Date(recipe.createdAt).toLocaleDateString('tr-TR') }}
              </td>

              <!-- İşlemler — her zaman görünür -->
              <td class="py-4 px-6 align-middle text-right whitespace-nowrap">
                <div class="flex items-center justify-end gap-2">
                  <button
                    @click="handleDeleteRecipe(recipe.id)"
                    class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-xl transition-colors"
                    title="Reçeteyi Sil"
                  >
                    <Trash2Icon :size="17" />
                  </button>
                  <div class="w-px h-6 bg-gray-200"></div>
                  <button
                    @click="openProduceModal(recipe)"
                    class="inline-flex items-center gap-1.5 bg-emerald-500 hover:bg-emerald-600 text-white px-4 py-2 rounded-xl text-sm font-bold transition-colors shadow-sm shadow-emerald-200"
                  >
                    <PlayIcon :size="15" class="fill-current" /> Üret
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Alt bilgi -->
      <div
        v-if="!isLoading && filteredRecipes.length > 0"
        class="px-6 py-3 border-t border-slate-100 bg-slate-50/30"
      >
        <p class="text-xs text-slate-400 font-medium">
          Toplam <span class="font-bold text-slate-600">{{ filteredRecipes.length }}</span> reçete
          listeleniyor
        </p>
      </div>
    </div>

    <AddRecipeModal
      :is-open="isAddRecipeModalOpen"
      @close="isAddRecipeModalOpen = false"
      @success="onRecipeAdded"
    />

    <ProduceModal
      :is-open="isProduceModalOpen"
      :recipe="selectedRecipe"
      @close="isProduceModalOpen = false"
      @success="onProduced"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  PlusIcon,
  PackageIcon,
  Trash2Icon,
  LayersIcon,
  PlayIcon,
  ClipboardListIcon,
  SearchIcon,
  RotateCcwIcon,
} from 'lucide-vue-next'

import CustomDropdown from '@/components/CustomDropdown.vue'
import AddRecipeModal from './AddRecipeModal.vue'
import ProduceModal from './ProduceModal.vue'
import { useAlert } from '@/composables/useAlert'

const { showError, showConfirm } = useAlert()

const recipes = ref<any[]>([])
const isLoading = ref(false)

const isAddRecipeModalOpen = ref(false)
const isProduceModalOpen = ref(false)
const selectedRecipe = ref<any>(null)

const searchText = ref('')
const filterIngredientCount = ref('')
const sortBy = ref('date_desc')

const ingredientCountOptions = [
  { label: 'Tüm Reçeteler', value: '' },
  { label: '1 Hammadde', value: '1' },
  { label: '2 Hammadde', value: '2' },
  { label: '3+ Hammadde', value: '3+' },
]

const sortOptions = [
  { label: 'İsme Göre (A→Z)', value: 'name_asc' },
  { label: 'İsme Göre (Z→A)', value: 'name_desc' },
  { label: 'En Yeni Önce', value: 'date_desc' },
  { label: 'En Eski Önce', value: 'date_asc' },
  { label: 'En Çok Hammadde', value: 'ingredient_desc' },
]

let searchTimer: ReturnType<typeof setTimeout> | null = null

const filteredRecipes = computed(() => {
  let list = [...recipes.value]

  // Arama
  if (searchText.value.trim()) {
    const q = searchText.value.trim().toLowerCase()
    list = list.filter((r) => r.productName.toLowerCase().includes(q))
  }

  // Hammadde sayısı filtresi
  if (filterIngredientCount.value) {
    if (filterIngredientCount.value === '3+') {
      list = list.filter((r) => r.items.length >= 3)
    } else {
      const count = parseInt(filterIngredientCount.value)
      list = list.filter((r) => r.items.length === count)
    }
  }

  // Sıralama
  list.sort((a, b) => {
    switch (sortBy.value) {
      case 'name_asc':
        return a.productName.localeCompare(b.productName, 'tr')
      case 'name_desc':
        return b.productName.localeCompare(a.productName, 'tr')
      case 'date_asc':
        return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
      case 'ingredient_desc':
        return b.items.length - a.items.length
      default:
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime() // date_desc
    }
  })

  return list
})

const handleSearch = () => {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {}, 0) // frontend filtre, API çağrısı yok
}

const applyFilters = () => {} // computed hallediyor

const resetFilters = () => {
  searchText.value = ''
  filterIngredientCount.value = ''
  sortBy.value = 'date_desc'
}

const fetchRecipes = async () => {
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const res = await fetch('/api/Production/recipes', {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (res.ok) recipes.value = await res.json()
  } catch (e) {
    console.error('Reçeteler getirilemedi:', e)
  } finally {
    isLoading.value = false
  }
}

const handleDeleteRecipe = async (id: string) => {
  if (!(await showConfirm('Bu reçeteyi silmek istediğinize emin misiniz?'))) return
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`/api/Production/recipe/${id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
    })
    if (res.ok) fetchRecipes()
    else {
      const data = await res.json()
      showError(data.message || 'Silme işlemi başarısız.')
    }
  } catch {
    showError('Sunucu bağlantı hatası.')
  }
}

const openProduceModal = (recipe: any) => {
  selectedRecipe.value = recipe
  isProduceModalOpen.value = true
}

const onRecipeAdded = () => {
  isAddRecipeModalOpen.value = false
  fetchRecipes()
}

const onProduced = () => {
  isProduceModalOpen.value = false
  fetchRecipes()
}

onMounted(() => fetchRecipes())
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 5px;
  height: 5px;
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
