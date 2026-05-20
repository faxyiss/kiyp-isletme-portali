<template>
  <Teleport to="body">
  <div
    v-if="isOpen"
    class="fixed inset-0 z-[50] flex items-center justify-center bg-gray-950/50 backdrop-blur-sm p-4 animate-fade-in"
  >
    <div
      class="relative w-full max-w-5xl bg-white rounded-2xl shadow-2xl border border-gray-100 overflow-hidden transform transition-all scale-100 flex flex-col h-[85vh] max-h-[85vh]"
    >
      <div
        class="flex items-center justify-between p-5 border-b border-gray-200 bg-gray-50/50 shrink-0"
      >
        <div class="flex items-center space-x-2.5">
          <div
            class="w-9 h-9 rounded-xl bg-blue-100 text-blue-600 flex items-center justify-center shadow-sm"
          >
            <ClipboardListIcon :size="20" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-gray-900">Yeni Üretim Reçetesi Tanımla</h3>
            <p class="text-xs text-gray-500">
              Ürünlerin bileşen formüllerini ve hammadde ihtiyaçlarını belirleyin.
            </p>
          </div>
        </div>
        <button
          @click="closeModal"
          type="button"
          class="text-gray-400 hover:bg-gray-100 hover:text-gray-900 rounded-xl w-9 h-9 flex justify-center items-center transition-colors border border-transparent hover:border-gray-200"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <div class="overflow-y-auto p-6 pb-56 space-y-6 custom-scrollbar flex-1 bg-slate-50/30">
        <div
          v-if="errorMessage"
          class="p-3.5 text-sm text-red-700 bg-red-50 border border-red-200 rounded-xl flex items-center gap-2 font-medium"
        >
          <span>⚠️</span> {{ errorMessage }}
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm space-y-4">
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div>
              <label class="block text-sm font-bold text-slate-800"
                >Üretilecek Nihai Ürün (Mamül)</label
              >
              <p class="text-xs text-slate-400 mt-0.5">
                Reçetesini yazmak istediğiniz ana ürünü seçin.
              </p>
            </div>

            <button
              @click="isAddProductModalOpen = true"
              type="button"
              class="text-xs font-bold text-blue-600 bg-blue-50 hover:bg-blue-100 border border-blue-200/60 px-3.5 py-2 rounded-xl flex items-center justify-center gap-1.5 transition-all self-end sm:self-auto shadow-sm"
            >
              <PlusIcon :size="14" /> Yeni Ürün Tanımla
            </button>
          </div>

          <!-- Çıktı miktarı -->
          <div class="flex items-center gap-4 p-3.5 bg-blue-50/60 border border-blue-100 rounded-xl">
            <div class="flex-1">
              <p class="text-xs font-bold text-slate-700">Bu reçete kaç birim üretir?</p>
              <p class="text-[11px] text-slate-400 mt-0.5">Girdiğiniz hammadde miktarları bu sayıya bölünerek 1 birim başına gereken miktara dönüştürülür.</p>
            </div>
            <input
              v-model.number="form.outputQuantity"
              type="number"
              min="0.001"
              step="1"
              class="w-24 px-3 py-2 border border-blue-200 rounded-xl text-sm font-bold text-slate-800 text-center bg-white focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400 transition-all"
              placeholder="1"
            />
          </div>

          <div class="relative">
            <button
              type="button"
              @click="isProductDropdownOpen = !isProductDropdownOpen"
              class="w-full flex items-center justify-between bg-slate-50 border text-sm rounded-xl p-3 transition-all outline-none font-medium"
              :class="isProductDropdownOpen
                ? 'border-blue-400 bg-white ring-4 ring-blue-500/10'
                : 'border-slate-200 hover:border-slate-300 text-slate-900'"
            >
              <span :class="form.productId ? 'text-slate-900 font-semibold' : 'text-slate-400'">
                {{ selectedProductLabel }}
              </span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200 shrink-0 ml-2"
                :class="{ 'rotate-180': isProductDropdownOpen }"
                :size="16"
              />
            </button>

            <div v-if="isProductDropdownOpen" class="fixed inset-0 z-40" @click="isProductDropdownOpen = false"></div>

            <div
              v-if="isProductDropdownOpen"
              class="absolute top-full left-0 mt-2 w-full bg-white border border-slate-200 rounded-2xl shadow-2xl z-50 p-2 flex flex-col gap-2 animate-fade-in origin-top"
            >
              <!-- Arama kutusu -->
              <div class="relative shrink-0">
                <SearchIcon class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" :size="14" />
                <input
                  v-model="productSearchText"
                  type="text"
                  placeholder="Ürün adı veya numarası ile ara..."
                  class="w-full pl-9 pr-3 py-2 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:border-blue-400 focus:bg-white text-xs transition-all font-medium"
                  autofocus
                  @click.stop
                />
              </div>
              <!-- Liste -->
              <div class="overflow-y-auto max-h-52 space-y-0.5 custom-scrollbar px-0.5">
                <button
                  v-for="prod in filteredFinalProducts"
                  :key="prod.id"
                  type="button"
                  @click="form.productId = prod.id; isProductDropdownOpen = false; productSearchText = ''"
                  class="w-full text-left px-3 py-2.5 rounded-xl text-sm transition-colors flex items-center justify-between gap-2"
                  :class="form.productId === prod.id
                    ? 'bg-blue-600 text-white'
                    : 'text-slate-700 hover:bg-slate-50'"
                >
                  <span class="font-semibold truncate">{{ prod.name }}</span>
                  <span
                    class="text-[10px] font-mono px-1.5 py-0.5 rounded shrink-0"
                    :class="form.productId === prod.id ? 'bg-blue-700 text-blue-100' : 'bg-slate-100 text-slate-400'"
                  >#{{ prod.productNo }}</span>
                </button>
                <div v-if="filteredFinalProducts.length === 0" class="text-center py-8 text-xs text-slate-400 font-medium">
                  Eşleşen ürün bulunamadı.
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-visible">
          <div class="p-5 border-b border-slate-100 flex items-center justify-between">
            <div class="flex items-center gap-2">
              <label class="block text-sm font-bold text-slate-800"
                >Gerekli Hammadde ve Malzemeler</label
              >
              <span
                class="text-xs bg-slate-100 text-slate-600 px-2.5 py-0.5 rounded-full font-bold border border-slate-200"
              >
                {{ form.items.length }} Satır
              </span>
            </div>

            <div class="flex items-center gap-2">
              <button
                @click="isAddRawMaterialModalOpen = true"
                type="button"
                class="text-xs font-bold text-emerald-600 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200/60 px-3.5 py-2 rounded-xl flex items-center gap-1.5 transition-all shadow-sm"
              >
                <PlusIcon :size="14" /> Yeni Hammadde Tanımla
              </button>

              <button
                @click="addRecipeItem"
                type="button"
                class="text-xs font-bold text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200/60 px-3.5 py-2 rounded-xl flex items-center gap-1.5 transition-all shadow-sm"
              >
                <PlusIcon :size="14" /> Satır Ekle
              </button>
            </div>
          </div>

          <div
            v-if="form.items.length > 0"
            class="px-5 py-3 bg-slate-50 border-b border-slate-100 grid grid-cols-12 gap-4 text-[11px] font-bold text-slate-400 uppercase tracking-wider"
          >
            <div class="col-span-8 px-1">Hammadde / Bileşen Seçimi</div>
            <div class="col-span-3 px-1">Gerekli Miktar (1 Birim İçin)</div>
            <div class="col-span-1 text-right">İşlem</div>
          </div>

          <div class="p-5 space-y-3">
            <div
              v-for="(item, index) in form.items"
              :key="index"
              class="grid grid-cols-12 gap-4 items-center bg-white p-2 rounded-xl border transition-all group relative"
              :class="
                openDropdownIndex === index
                  ? 'z-[60] border-blue-300 shadow-md'
                  : 'z-10 border-slate-200 hover:border-slate-300'
              "
            >
              <div class="col-span-8 relative">
                <button
                  @click="toggleDropdown(index)"
                  type="button"
                  class="w-full flex items-center justify-between bg-slate-50/60 border border-slate-200 text-slate-800 text-sm rounded-lg p-2.5 hover:border-slate-300 transition-all focus:outline-none focus:ring-4 focus:ring-blue-500/10 focus:bg-white"
                >
                  <span
                    class="truncate font-semibold"
                    :class="{ 'text-slate-400': !item.rawMaterialId }"
                  >
                    {{ getRawMaterialName(item.rawMaterialId) }}
                  </span>
                  <ChevronDownIcon
                    class="text-slate-400 transition-transform duration-200 shrink-0 ml-2"
                    :class="{ 'rotate-180': openDropdownIndex === index }"
                    :size="16"
                  />
                </button>

                <div
                  v-if="openDropdownIndex === index"
                  class="fixed inset-0 z-40"
                  @click.stop="openDropdownIndex = null"
                ></div>

                <div
                  v-if="openDropdownIndex === index"
                  class="absolute top-full left-0 mt-2 w-full bg-white border border-slate-200 rounded-xl shadow-2xl z-50 p-2 flex flex-col gap-2 animate-fade-in origin-top"
                >
                  <div class="relative flex-shrink-0">
                    <SearchIcon
                      class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
                      :size="14"
                    />
                    <input
                      v-model="rawMaterialSearchText"
                      type="text"
                      placeholder="Hammadde ismi veya no ile ara..."
                      class="w-full pl-9 pr-3 py-2 bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:border-blue-500 focus:bg-white text-xs transition-all font-medium"
                      autofocus
                      @click.stop
                    />
                  </div>
                  <div class="overflow-y-auto flex-1 p-0.5 space-y-0.5 max-h-44 custom-scrollbar">
                    <button
                      v-for="rm in filteredRawMaterials"
                      :key="rm.id"
                      @click="selectRawMaterial(index, rm.id)"
                      type="button"
                      class="w-full text-left px-3 py-2 rounded-lg text-xs font-bold transition-colors truncate flex justify-between items-center"
                      :class="
                        item.rawMaterialId === rm.id
                          ? 'bg-blue-600 text-white'
                          : 'text-slate-600 hover:bg-slate-50'
                      "
                    >
                      <span>{{ rm.name }}</span>
                      <span
                        class="text-[10px] font-mono px-1.5 py-0.5 rounded"
                        :class="
                          item.rawMaterialId === rm.id
                            ? 'bg-blue-700 text-blue-100'
                            : 'bg-slate-100 text-slate-400'
                        "
                      >
                        #{{ rm.productNo }}
                      </span>
                    </button>
                    <div
                      v-if="filteredRawMaterials.length === 0"
                      class="text-center py-6 text-xs text-slate-400 font-medium"
                    >
                      Eşleşen hammadde/malzeme bulunamadı.
                    </div>
                  </div>
                </div>
              </div>

              <div class="col-span-3">
                <div class="relative">
                  <input
                    v-model="item.requiredQuantity"
                    type="number"
                    step="1"
                    min="1"
                    class="bg-slate-50/60 border border-slate-200 text-slate-800 text-sm font-bold rounded-lg focus:ring-4 focus:ring-blue-500/10 focus:border-blue-500 focus:bg-white block w-full p-2.5 pr-12 transition-all outline-none"
                    placeholder="1"
                  />
                  <span
                    class="absolute right-3 top-1/2 -translate-y-1/2 text-[10px] text-slate-400 font-extrabold uppercase tracking-wider"
                    >Adet</span
                  >
                </div>
              </div>

              <div class="col-span-1 text-right">
                <button
                  @click="removeRecipeItem(index)"
                  type="button"
                  class="text-slate-400 hover:text-red-600 hover:bg-red-50 p-2 rounded-xl transition-colors inline-flex items-center justify-center"
                  title="Satırı Kaldır"
                >
                  <Trash2Icon :size="16" />
                </button>
              </div>
            </div>

            <div
              v-if="form.items.length === 0"
              class="text-center py-12 text-sm text-slate-400 border-2 border-dashed border-slate-200 rounded-2xl bg-slate-50/40"
            >
              <div class="text-2xl mb-1.5">📋</div>
              <p class="font-semibold text-slate-600">Reçete içeriği henüz boş.</p>
              <p class="text-xs text-slate-400 mt-0.5">
                Üretim formülünü oluşturmak için sağ üstten hammadde satırları ekleyin.
              </p>
            </div>
          </div>
        </div>
      </div>

      <div
        class="flex items-center justify-end space-x-3 p-5 border-t border-gray-100 bg-white shrink-0"
      >
        <button
          @click="closeModal"
          type="button"
          class="px-6 py-2.5 text-sm font-bold text-gray-500 bg-white hover:text-gray-700 transition-colors"
        >
          Vazgeç
        </button>

        <button
          @click="handleSubmit"
          :disabled="isSubmitting || form.items.length === 0 || !form.productId"
          class="px-8 py-3 text-sm font-bold text-white bg-blue-600 hover:bg-blue-700 shadow-lg shadow-blue-200/50 disabled:opacity-40 disabled:cursor-not-allowed disabled:shadow-none transition-all flex items-center gap-2 rounded-full"
        >
          <span v-if="isSubmitting" class="animate-pulse">Formül Kaydediliyor...</span>
          <span v-else>Reçeteyi Kaydet</span>
        </button>
      </div>
    </div>

    <AddRawMaterialModal
      v-if="isAddRawMaterialModalOpen"
      :is-open="isAddRawMaterialModalOpen"
      @close="isAddRawMaterialModalOpen = false"
      @success="onRawMaterialAdded"
    />

    <AddProductModal
      v-if="isAddProductModalOpen"
      :is-open="isAddProductModalOpen"
      :categories="categories.filter((c) => c.type === 0)"
      @close="isAddProductModalOpen = false"
      @add-category="(newCat) => categories.push(newCat)"
      @success="onProductAdded"
    />
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import {
  PlusIcon,
  Trash2Icon,
  XIcon,
  ClipboardListIcon,
  ChevronDownIcon,
  SearchIcon,
} from 'lucide-vue-next'

import AddRawMaterialModal from './AddRawMaterialModal.vue'
import AddProductModal from '../stocks/AddProductModal.vue'

const props = defineProps({ isOpen: Boolean })
const emit = defineEmits(['close', 'success'])

const isSubmitting = ref(false)
const errorMessage = ref('')

const finalProducts = ref<any[]>([])
const rawMaterials = ref<any[]>([])
const categories = ref<any[]>([])

const isAddRawMaterialModalOpen = ref(false)
const isAddProductModalOpen = ref(false)

const openDropdownIndex = ref<number | null>(null)
const rawMaterialSearchText = ref('')
const isProductDropdownOpen = ref(false)
const productSearchText = ref('')

const form = ref({
  productId: '',
  outputQuantity: 1,
  items: [] as { rawMaterialId: string; requiredQuantity: number }[],
})

const fetchAllData = async () => {
  try {
    const token = localStorage.getItem('token')

    const prodRes = await fetch('/api/Products/search?PageSize=1000', {
      headers: { Authorization: `Bearer ${token}` },
    })
    const prodData = await prodRes.json()
    finalProducts.value = prodData.items

    const rawRes = await fetch('/api/Products/raw-materials?PageSize=1000', {
      headers: { Authorization: `Bearer ${token}` },
    })
    const rawData = await rawRes.json()
    rawMaterials.value = rawData.items
  } catch (error) {
    console.error('Veri yükleme hatası:', error)
  }
}

watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      errorMessage.value = ''
      form.value = { productId: '', outputQuantity: 1, items: [] }
      openDropdownIndex.value = null
      isProductDropdownOpen.value = false
      productSearchText.value = ''
      fetchAllData()
    }
  },
)

const toggleDropdown = (index: number) => {
  openDropdownIndex.value = openDropdownIndex.value === index ? null : index
  rawMaterialSearchText.value = ''
}

const selectRawMaterial = (itemIndex: number, rmId: string) => {
  const item = form.value.items[itemIndex]
  if (item) item.rawMaterialId = rmId
  openDropdownIndex.value = null
}

const getRawMaterialName = (id: string) => {
  if (!id) return 'Lütfen bir bileşen/hammadde seçin...'
  const rm = rawMaterials.value.find((r) => r.id === id)
  return rm ? rm.name : 'Lütfen bir bileşen/hammadde seçin...'
}

const filteredFinalProducts = computed(() => {
  const q = productSearchText.value.trim().toLowerCase()
  if (!q) return finalProducts.value
  return finalProducts.value.filter(
    (p) => p.name.toLowerCase().includes(q) || (p.productNo && p.productNo.toString().includes(q)),
  )
})

const selectedProductLabel = computed(() => {
  if (!form.value.productId) return 'Üretilecek nihai ürünü seçin...'
  const p = finalProducts.value.find((p) => p.id === form.value.productId)
  return p ? `${p.name}  —  #${p.productNo}` : 'Üretilecek nihai ürünü seçin...'
})

const filteredRawMaterials = computed(() => {
  const query = rawMaterialSearchText.value.trim().toLowerCase()
  if (!query) return rawMaterials.value
  return rawMaterials.value.filter(
    (rm) =>
      rm.name.toLowerCase().includes(query) ||
      (rm.productNo && rm.productNo.toString().includes(query)),
  )
})

const onProductAdded = () => {
  isAddProductModalOpen.value = false
  fetchAllData()
}

const onRawMaterialAdded = () => {
  isAddRawMaterialModalOpen.value = false
  fetchAllData()
}

const addRecipeItem = () => {
  form.value.items.push({ rawMaterialId: '', requiredQuantity: 1 })
}

const removeRecipeItem = (index: number) => {
  form.value.items.splice(index, 1)
  openDropdownIndex.value = null
}

const handleSubmit = async () => {
  if (form.value.items.some((i) => !i.rawMaterialId || i.requiredQuantity <= 0)) {
    errorMessage.value =
      'Lütfen reçetedeki tüm hammadde seçimlerini ve miktarlarını eksiksiz doldurun.'
    return
  }

  const output = Math.max(form.value.outputQuantity || 1, 0.001)

  // Hammadde miktarlarını 1 birim başına düşür (girilen miktar / çıktı adedi)
  const normalizedItems = form.value.items.map((i) => ({
    rawMaterialId: i.rawMaterialId,
    requiredQuantity: Math.round((i.requiredQuantity / output) * 1000000) / 1000000,
  }))

  isSubmitting.value = true
  errorMessage.value = ''
  try {
    const token = localStorage.getItem('token')
    const response = await fetch('/api/Production/recipe', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
      body: JSON.stringify({ productId: form.value.productId, items: normalizedItems }),
    })

    if (response.ok) emit('success')
    else {
      const err = await response.json()
      errorMessage.value = err.message || 'Reçete kaydedilirken sunucudan bir hata döndü.'
    }
  } catch (error) {
    errorMessage.value = 'Sunucuyla bağlantı kurulamadı. Lütfen ağınızı kontrol edin.'
  } finally {
    isSubmitting.value = false
  }
}

const closeModal = () => emit('close')
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 5px;
  height: 5px;
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
.animate-fade-in {
  animation: fadeIn 0.15s ease-out forwards;
}
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: scale(0.98) translateY(-4px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}
</style>
