<template>
  <div
    class="bg-white rounded-2xl border border-slate-100 shadow-sm p-6 flex flex-col h-[calc(100vh-220px)]"
  >
    <div class="border-b border-slate-100 pb-3 mb-4">
      <h3 class="font-bold text-slate-800 flex items-center gap-2 text-sm mb-3">
        <ReceiptIcon class="text-blue-500" :size="18" /> Satış Yönetimi
      </h3>
      <div class="flex bg-slate-100 p-1 rounded-xl border border-slate-200 text-xs font-bold">
        <button
          @click="detailTab = 'tektek'"
          :class="
            detailTab === 'tektek'
              ? 'bg-white shadow-xs text-blue-600'
              : 'text-slate-500 hover:text-slate-700'
          "
          class="flex-1 py-2 rounded-lg transition-all cursor-pointer text-center"
        >
          Tektek Satış
        </button>
        <button
          @click="detailTab = 'sepet'"
          :class="
            detailTab === 'sepet'
              ? 'bg-white shadow-xs text-blue-600'
              : 'text-slate-500 hover:text-slate-700'
          "
          class="flex-1 py-2 rounded-lg transition-all cursor-pointer text-center flex items-center justify-center gap-1"
        >
          Sepet Durumu
          <span
            v-if="cart.length > 0"
            class="bg-blue-600 text-white text-[10px] px-1.5 py-0.5 rounded-full font-black"
          >
            {{ cart.length }}
          </span>
        </button>
      </div>
    </div>

    <div v-if="detailTab === 'tektek'" class="flex-1 flex flex-col justify-between overflow-hidden">
      <div
        v-if="!selectedProduct"
        class="flex-1 flex flex-col items-center justify-center text-slate-400"
      >
        <PackageOpenIcon :size="48" class="mb-3 opacity-50" />
        <p class="text-sm font-medium">Lütfen sol taraftan bir ürün seçin.</p>
      </div>

      <div v-else class="flex-1 space-y-5 overflow-y-auto pr-1 custom-scrollbar pb-10">
        <div
          class="bg-blue-50/50 p-4 rounded-xl border border-blue-100 flex justify-between items-center"
        >
          <div>
            <p class="text-xs text-blue-500 font-bold mb-0.5">Seçilen Ürün</p>
            <h4 class="font-bold text-slate-800">{{ selectedProduct.name }}</h4>
          </div>
          <div class="text-right">
            <p class="text-xs text-slate-500 mb-0.5">Birim Fiyat</p>
            <span class="font-bold text-slate-700">{{
              formatCurrency(selectedProduct.unitPrice)
            }}</span>
          </div>
        </div>

        <div class="space-y-1.5 relative" :class="{ 'z-50': isCustomerDropdownOpen }">
          <label class="text-sm font-semibold text-slate-700 flex justify-between">
            Müşteri Seçimi
            <span class="text-[10px] text-slate-400 font-normal mt-0.5">(Opsiyonel)</span>
          </label>

          <div
            @click="isCustomerDropdownOpen = !isCustomerDropdownOpen"
            class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm bg-white text-slate-700 cursor-pointer flex justify-between items-center transition-all hover:border-blue-400"
            :class="{ 'border-blue-500 ring-2 ring-blue-50': isCustomerDropdownOpen }"
          >
            <span class="truncate font-medium">{{ selectedCustomerName }}</span>
            <ChevronDownIcon
              :size="16"
              class="text-slate-400 shrink-0 transition-transform"
              :class="{ 'rotate-180': isCustomerDropdownOpen }"
            />
          </div>

          <div
            v-if="isCustomerDropdownOpen"
            class="absolute top-[65px] left-0 w-full bg-white border border-slate-200 rounded-xl shadow-xl overflow-hidden flex flex-col max-h-56 z-50"
          >
            <div class="p-2 border-b border-slate-100 shrink-0 relative">
              <SearchIcon
                :size="14"
                class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"
              />
              <input
                v-model="customerSearch"
                type="text"
                placeholder="Müşteri ara..."
                class="w-full pl-8 pr-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs focus:outline-none focus:border-blue-500 focus:bg-white transition-colors"
                @click.stop
              />
            </div>
            <ul class="overflow-y-auto custom-scrollbar flex-1 p-1">
              <li
                @click="selectSingleCustomer(null)"
                class="px-3 py-2 text-xs font-bold rounded-lg cursor-pointer transition-colors"
                :class="
                  saleForm.customerId === null
                    ? 'bg-blue-50 text-blue-700'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                Genel Müşteri (Kayıtsız)
              </li>
              <li
                v-if="filteredCustomers.length === 0"
                class="px-3 py-4 text-xs text-center text-slate-400 font-medium"
              >
                Müşteri bulunamadı.
              </li>
              <li
                v-else
                v-for="customer in filteredCustomers"
                :key="customer.id"
                @click="selectSingleCustomer(customer.id)"
                class="px-3 py-2 text-xs font-medium rounded-lg cursor-pointer transition-colors"
                :class="
                  saleForm.customerId === customer.id
                    ? 'bg-blue-50 text-blue-700 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                {{ customer.firstName }} {{ customer.lastName }}
              </li>
            </ul>
          </div>
          <div
            v-if="isCustomerDropdownOpen"
            @click="closeSingleDropdown"
            class="fixed inset-0 z-40"
          ></div>
        </div>

        <div class="space-y-1.5">
          <label class="text-sm font-semibold text-slate-700 flex justify-between">
            Miktar
            <span class="text-[10px] text-emerald-500 font-bold mt-0.5"
              >Max: {{ selectedProduct.remainingQuantity }}</span
            >
          </label>
          <div class="flex items-center gap-2">
            <button
              @click="saleForm.quantity > 1 && saleForm.quantity--"
              class="w-10 h-10 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold cursor-pointer transition-colors"
            >
              -
            </button>
            <input
              v-model.number="saleForm.quantity"
              type="number"
              min="1"
              :max="selectedProduct.remainingQuantity"
              step="1"
              @keydown="preventDecimals"
              class="no-spinners flex-1 text-center py-2.5 rounded-xl border border-slate-200 font-bold focus:outline-none focus:border-blue-500"
            />
            <button
              @click="saleForm.quantity < selectedProduct.remainingQuantity && saleForm.quantity++"
              class="w-10 h-10 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold cursor-pointer transition-colors"
            >
              +
            </button>
          </div>
        </div>

        <div class="space-y-2">
          <label class="text-sm font-semibold text-slate-700">Ödeme Yöntemi</label>
          <div class="grid grid-cols-3 gap-2">
            <button
              @click="saleForm.paymentType = 0"
              :class="
                saleForm.paymentType === 0
                  ? 'bg-emerald-50 border-emerald-500 text-emerald-700'
                  : 'bg-white border-slate-200 text-slate-500 hover:bg-slate-50'
              "
              class="flex flex-col items-center justify-center p-3 rounded-xl border transition-all cursor-pointer"
            >
              <BanknoteIcon :size="20" class="mb-1" /><span class="text-xs font-bold">Nakit</span>
            </button>
            <button
              @click="saleForm.paymentType = 1"
              :class="
                saleForm.paymentType === 1
                  ? 'bg-blue-50 border-blue-500 text-blue-700'
                  : 'bg-white border-slate-200 text-slate-500 hover:bg-slate-50'
              "
              class="flex flex-col items-center justify-center p-3 rounded-xl border transition-all cursor-pointer"
            >
              <CreditCardIcon :size="20" class="mb-1" /><span class="text-xs font-bold"
                >Kredi K.</span
              >
            </button>
            <button
              @click="saleForm.paymentType = 2"
              :class="
                saleForm.paymentType === 2
                  ? 'bg-rose-50 border-rose-500 text-rose-700'
                  : 'bg-white border-slate-200 text-slate-500 hover:bg-slate-50'
              "
              class="flex flex-col items-center justify-center p-3 rounded-xl border transition-all cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
              :disabled="!saleForm.customerId"
            >
              <BookOpenIcon :size="20" class="mb-1" /><span class="text-xs font-bold"
                >Veresiye</span
              >
            </button>
          </div>
        </div>
      </div>

      <div class="mt-4 pt-4 border-t border-slate-100 space-y-4">
        <div class="flex justify-between items-end">
          <span class="text-sm font-semibold text-slate-500">Genel Toplam</span>
          <span class="text-3xl font-black text-slate-800">{{ formatCurrency(totalAmount) }}</span>
        </div>
        <button
          @click="submitSingleSale"
          :disabled="
            !selectedProduct ||
            saleForm.quantity > (selectedProduct?.remainingQuantity || 0) ||
            isSubmitting
          "
          class="w-full flex items-center justify-center gap-2 py-3.5 rounded-xl font-bold text-white transition-all shadow-md cursor-pointer disabled:opacity-50"
          :class="
            saleForm.paymentType === 0
              ? 'bg-emerald-600 hover:bg-emerald-700'
              : saleForm.paymentType === 1
                ? 'bg-blue-600 hover:bg-blue-700'
                : 'bg-rose-600 hover:bg-rose-700'
          "
        >
          <span v-if="isSubmitting" class="animate-pulse">İşleniyor...</span>
          <template v-else><CheckCircleIcon :size="20" /> Satışı Tamamla</template>
        </button>
      </div>
    </div>

    <div
      v-else-if="detailTab === 'sepet'"
      class="flex-1 flex flex-col justify-between overflow-hidden"
    >
      <div class="mb-3">
        <div
          v-if="selectedProduct"
          class="bg-blue-50/60 p-3 rounded-xl border border-blue-200 space-y-2 animate-fade-in"
        >
          <div class="flex justify-between items-center text-xs">
            <span class="font-bold text-slate-800 truncate">{{ selectedProduct.name }}</span>
            <span class="font-bold text-blue-600">{{
              formatCurrency(selectedProduct.unitPrice)
            }}</span>
          </div>
          <div class="flex items-center gap-2">
            <div
              class="flex items-center bg-white border border-slate-200 rounded-lg overflow-hidden h-9 flex-1"
            >
              <button
                @click="cartForm.quantity > 1 && cartForm.quantity--"
                type="button"
                class="px-2 text-slate-500 hover:bg-slate-50 font-bold h-full cursor-pointer"
              >
                -
              </button>
              <input
                v-model.number="cartForm.quantity"
                type="number"
                min="1"
                :max="selectedProduct.remainingQuantity"
                step="1"
                @keydown="preventDecimals"
                class="no-spinners w-full text-center text-xs font-bold focus:outline-none"
              />
              <button
                @click="
                  cartForm.quantity < selectedProduct.remainingQuantity && cartForm.quantity++
                "
                type="button"
                class="px-2 text-slate-500 hover:bg-slate-50 font-bold h-full cursor-pointer"
              >
                +
              </button>
            </div>
            <button
              @click="addToCart"
              type="button"
              class="bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold px-3 h-9 rounded-lg transition-all cursor-pointer flex items-center gap-1 shadow-sm"
            >
              <PlusIcon :size="14" /> Sepete Ekle
            </button>
          </div>
        </div>
      </div>

      <div
        class="flex-1 overflow-y-auto space-y-2 pr-1 custom-scrollbar bg-slate-50/30 p-1.5 rounded-xl border border-slate-100/80"
      >
        <div
          v-if="cart.length === 0"
          class="h-full flex flex-col items-center justify-center text-slate-400 py-8 text-xs text-center"
        >
          <ShoppingCartIcon :size="32" class="mb-2 opacity-40 mx-auto text-slate-400" />
          <span>Sepetiniz boş.</span>
        </div>
        <div
          v-else
          v-for="(item, index) in cart"
          :key="index"
          class="flex justify-between items-center bg-white border border-slate-100 p-2.5 rounded-xl shadow-xs"
        >
          <div class="flex-1 min-w-0 pr-2">
            <h5 class="text-xs font-bold text-slate-800 truncate">{{ item.name }}</h5>
            <p class="text-[10px] text-slate-400 font-medium">
              {{ item.quantity }} adet x {{ formatCurrency(item.unitPrice) }}
            </p>
          </div>
          <div class="text-right flex items-center gap-1.5">
            <span class="text-xs font-bold text-slate-700 mr-1">{{
              formatCurrency(item.total)
            }}</span>
            <button
              @click="removeFromCart(index)"
              class="text-slate-300 hover:text-red-500 p-1 rounded-md hover:bg-red-50 cursor-pointer transition-colors"
            >
              <Trash2Icon :size="14" />
            </button>
          </div>
        </div>
      </div>

      <div class="pt-3 border-t border-slate-100 space-y-3 mt-3">
        <div class="space-y-1 relative" :class="{ 'z-50': isBulkCustomerDropdownOpen }">
          <div
            @click="isBulkCustomerDropdownOpen = !isBulkCustomerDropdownOpen"
            class="w-full px-3 py-2 rounded-lg border border-slate-200 text-xs bg-white text-slate-700 cursor-pointer flex justify-between items-center transition-all hover:border-blue-400"
            :class="{ 'border-blue-500 ring-2 ring-blue-50': isBulkCustomerDropdownOpen }"
          >
            <span class="truncate font-medium">{{ selectedBulkCustomerName }}</span>
            <ChevronDownIcon
              :size="14"
              class="text-slate-400 shrink-0 transition-transform"
              :class="{ 'rotate-180': isBulkCustomerDropdownOpen }"
            />
          </div>

          <div
            v-if="isBulkCustomerDropdownOpen"
            class="absolute bottom-full mb-1 left-0 w-full bg-white border border-slate-200 rounded-xl shadow-[0_-10px_40px_rgba(0,0,0,0.1)] overflow-hidden flex flex-col max-h-56 z-50"
          >
            <div class="p-2 border-b border-slate-100 shrink-0 relative">
              <SearchIcon
                :size="12"
                class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
              />
              <input
                v-model="bulkCustomerSearch"
                type="text"
                placeholder="Müşteri ara..."
                class="w-full pl-7 pr-2 py-1.5 bg-slate-50 border border-slate-200 rounded-lg text-xs focus:outline-none focus:border-blue-500 focus:bg-white transition-colors"
                @click.stop
              />
            </div>
            <ul class="overflow-y-auto custom-scrollbar flex-1 p-1">
              <li
                @click="selectBulkCustomer(null)"
                class="px-2 py-1.5 text-xs font-bold rounded-lg cursor-pointer transition-colors"
                :class="
                  bulkSaleForm.customerId === null
                    ? 'bg-blue-50 text-blue-700'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                Genel Müşteri (Kayıtsız)
              </li>
              <li
                v-if="filteredBulkCustomers.length === 0"
                class="px-2 py-3 text-xs text-center text-slate-400 font-medium"
              >
                Bulunamadı.
              </li>
              <li
                v-else
                v-for="customer in filteredBulkCustomers"
                :key="customer.id"
                @click="selectBulkCustomer(customer.id)"
                class="px-2 py-1.5 text-xs font-medium rounded-lg cursor-pointer transition-colors"
                :class="
                  bulkSaleForm.customerId === customer.id
                    ? 'bg-blue-50 text-blue-700 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                {{ customer.firstName }} {{ customer.lastName }}
              </li>
            </ul>
          </div>
          <div
            v-if="isBulkCustomerDropdownOpen"
            @click="closeBulkDropdown"
            class="fixed inset-0 z-40"
          ></div>
        </div>

        <div class="grid grid-cols-3 gap-1 text-[10px] font-bold relative z-10">
          <button
            @click="bulkSaleForm.paymentType = 0"
            :class="
              bulkSaleForm.paymentType === 0
                ? 'bg-emerald-50 border-emerald-500 text-emerald-700'
                : 'bg-white border-slate-200 text-slate-500 hover:bg-slate-50'
            "
            class="flex flex-col items-center py-1.5 rounded-lg border transition-all cursor-pointer"
          >
            <BanknoteIcon :size="14" class="mb-0.5" /> Nakit
          </button>
          <button
            @click="bulkSaleForm.paymentType = 1"
            :class="
              bulkSaleForm.paymentType === 1
                ? 'bg-blue-50 border-blue-500 text-blue-700'
                : 'bg-white border-slate-200 text-slate-500 hover:bg-slate-50'
            "
            class="flex flex-col items-center py-1.5 rounded-lg border transition-all cursor-pointer"
          >
            <CreditCardIcon :size="14" class="mb-0.5" /> Kart
          </button>
          <button
            @click="bulkSaleForm.paymentType = 2"
            :class="
              bulkSaleForm.paymentType === 2
                ? 'bg-rose-50 border-rose-500 text-rose-700'
                : 'bg-white border-slate-200 text-slate-500 hover:bg-slate-50'
            "
            class="flex flex-col items-center py-1.5 rounded-lg border transition-all cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="!bulkSaleForm.customerId"
          >
            <BookOpenIcon :size="14" class="mb-0.5" /> Veresiye
          </button>
        </div>

        <div class="pt-2 flex justify-between items-end relative z-10">
          <span class="text-xs font-semibold text-slate-500">Toplam ({{ cartCount }} Ürün)</span>
          <span class="text-xl font-black text-slate-800">{{ formatCurrency(cartTotal) }}</span>
        </div>

        <button
          @click="submitBulkSale"
          :disabled="cart.length === 0 || isSubmitting"
          class="w-full flex items-center justify-center gap-1.5 py-2.5 rounded-xl font-bold text-xs text-white transition-all shadow-md cursor-pointer disabled:opacity-50 relative z-10"
          :class="
            bulkSaleForm.paymentType === 0
              ? 'bg-emerald-600 hover:bg-emerald-700'
              : bulkSaleForm.paymentType === 1
                ? 'bg-blue-600 hover:bg-blue-700'
                : 'bg-rose-600 hover:bg-rose-700'
          "
        >
          <span v-if="isSubmitting">İşleniyor...</span>
          <template v-else><CheckCircleIcon :size="16" /> Toplu Satışı Tamamla</template>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import axios from 'axios'
import {
  ReceiptIcon,
  PackageOpenIcon,
  BanknoteIcon,
  CreditCardIcon,
  BookOpenIcon,
  CheckCircleIcon,
  PlusIcon,
  Trash2Icon,
  ShoppingCartIcon,
  ChevronDownIcon,
  SearchIcon,
} from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { showSuccess, showError, showWarning } = useAlert()

interface Product {
  id: string
  productNo: number
  name: string
  unitPrice: number
  remainingQuantity: number
}
interface Customer {
  id: string
  firstName: string
  lastName: string
}
interface CartItem {
  id: string
  productNo: number
  name: string
  unitPrice: number
  quantity: number
  total: number
}

const props = defineProps<{
  selectedProduct: Product | null
  customersList: Customer[]
}>()

const emit = defineEmits(['saleCompleted', 'clearSelection'])

const token = localStorage.getItem('token')
const config = { headers: { Authorization: `Bearer ${token}` } }

const detailTab = ref<'tektek' | 'sepet'>('tektek')
const isSubmitting = ref(false)

// ======== TEKİL SATIŞ STATE & HESAPLAMALAR ========
const saleForm = ref({ customerId: null as string | null, quantity: 1, paymentType: 0 })
const customerSearch = ref('')
const isCustomerDropdownOpen = ref(false)

// Virgül, Nokta, E harfi vb. yazılmasını tamamen engelleyen fonksiyon
const preventDecimals = (e: KeyboardEvent) => {
  if (['.', ',', 'e', 'E', '+', '-'].includes(e.key)) {
    e.preventDefault()
  }
}

const filteredCustomers = computed(() => {
  if (!customerSearch.value) return props.customersList
  const s = customerSearch.value.toLowerCase()
  return props.customersList.filter(
    (c) => c.firstName.toLowerCase().includes(s) || c.lastName.toLowerCase().includes(s),
  )
})

const selectedCustomerName = computed(() => {
  if (!saleForm.value.customerId) return 'Genel Müşteri (Kayıtsız)'
  const c = props.customersList.find((x) => x.id === saleForm.value.customerId)
  return c ? `${c.firstName} ${c.lastName}` : 'Genel Müşteri (Kayıtsız)'
})

const selectSingleCustomer = (id: string | null) => {
  saleForm.value.customerId = id
  isCustomerDropdownOpen.value = false
  customerSearch.value = ''
}

const closeSingleDropdown = () => {
  isCustomerDropdownOpen.value = false
  customerSearch.value = ''
}

watch(
  () => props.selectedProduct,
  (newVal) => {
    if (newVal) {
      saleForm.value.quantity = 1
      cartForm.value.quantity = 1
    }
  },
)

const totalAmount = computed(() =>
  props.selectedProduct ? props.selectedProduct.unitPrice * saleForm.value.quantity : 0,
)

const submitSingleSale = async () => {
  if (!props.selectedProduct) return
  isSubmitting.value = true
  try {
    const payload = {
      productId: props.selectedProduct.id,
      quantity: saleForm.value.quantity,
      salePrice: props.selectedProduct.unitPrice,
      paymentType: saleForm.value.paymentType,
      customerId: saleForm.value.customerId,
    }
    await axios.post('/Sales', payload, config)
    saleForm.value.quantity = 1
    saleForm.value.customerId = null
    emit('clearSelection')
    emit('saleCompleted')
    showSuccess('Satış başarıyla tamamlandı!')
  } catch (error) {
    if (axios.isAxiosError(error)) showError(error.response?.data?.message || 'Satış hatası!')
    else showError('Bilinmeyen bir hata oluştu.')
  } finally {
    isSubmitting.value = false
  }
}

// ======== SEPET İŞLEMLERİ STATE & HESAPLAMALAR ========
const cart = ref<CartItem[]>([])
const cartForm = ref({ quantity: 1 })
const bulkSaleForm = ref({ customerId: null as string | null, paymentType: 0 })

const bulkCustomerSearch = ref('')
const isBulkCustomerDropdownOpen = ref(false)

const filteredBulkCustomers = computed(() => {
  if (!bulkCustomerSearch.value) return props.customersList
  const s = bulkCustomerSearch.value.toLowerCase()
  return props.customersList.filter(
    (c) => c.firstName.toLowerCase().includes(s) || c.lastName.toLowerCase().includes(s),
  )
})

const selectedBulkCustomerName = computed(() => {
  if (!bulkSaleForm.value.customerId) return 'Genel Müşteri (Kayıtsız)'
  const c = props.customersList.find((x) => x.id === bulkSaleForm.value.customerId)
  return c ? `${c.firstName} ${c.lastName}` : 'Genel Müşteri (Kayıtsız)'
})

const selectBulkCustomer = (id: string | null) => {
  bulkSaleForm.value.customerId = id
  isBulkCustomerDropdownOpen.value = false
  bulkCustomerSearch.value = ''
}

const closeBulkDropdown = () => {
  isBulkCustomerDropdownOpen.value = false
  bulkCustomerSearch.value = ''
}

const addToCart = () => {
  if (!props.selectedProduct) return
  const existingItem = cart.value.find((item) => item.id === props.selectedProduct?.id)

  if (existingItem) {
    if (existingItem.quantity + cartForm.value.quantity > props.selectedProduct.remainingQuantity) {
      showWarning('Bu ürün için stok sınırını aşıyorsunuz!')
      return
    }
    existingItem.quantity += cartForm.value.quantity
    existingItem.total = existingItem.quantity * existingItem.unitPrice
  } else {
    cart.value.push({
      id: props.selectedProduct.id,
      productNo: props.selectedProduct.productNo,
      name: props.selectedProduct.name,
      unitPrice: props.selectedProduct.unitPrice,
      quantity: cartForm.value.quantity,
      total: props.selectedProduct.unitPrice * cartForm.value.quantity,
    })
  }
  cartForm.value.quantity = 1
  emit('clearSelection')
}

const removeFromCart = (index: number) => cart.value.splice(index, 1)
const cartTotal = computed(() => cart.value.reduce((sum, item) => sum + item.total, 0))
const cartCount = computed(() => cart.value.reduce((sum, item) => sum + item.quantity, 0))

const submitBulkSale = async () => {
  if (cart.value.length === 0) return
  isSubmitting.value = true
  try {
    const payload = cart.value.map((item) => ({
      productId: item.id,
      quantity: item.quantity,
      salePrice: item.unitPrice,
      paymentType: bulkSaleForm.value.paymentType,
      customerId: bulkSaleForm.value.customerId,
    }))

    await axios.post('/Sales/bulk', payload, config)

    cart.value = []
    bulkSaleForm.value.customerId = null
    emit('saleCompleted')
    showSuccess('Toplu sepet satışı başarıyla tamamlandı!')
  } catch (error) {
    if (axios.isAxiosError(error))
      showError(error.response?.data?.message || 'Sepet satışı sırasında bir hata oluştu!')
    else showError('Bilinmeyen bir hata oluştu.')
  } finally {
    isSubmitting.value = false
  }
}

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value)
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 4px;
}

/* YENİ EKLENEN: Native miktar oklarını gizleme (Tüm Tarayıcılar İçin) */
input.no-spinners::-webkit-outer-spin-button,
input.no-spinners::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
input.no-spinners[type='number'] {
  -moz-appearance: textfield;
}
</style>
