<template>
  <div class="space-y-6 animate-fade-in">
    <div
      class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-100 shadow-sm"
    >
      <div>
        <h2 class="text-xl font-bold text-slate-800">Müşteri ve Borç Yönetimi</h2>
        <p class="text-sm text-slate-500 mt-1">
          Müşterilerinizi listeleyebilir, mevcut borç durumlarını takip edebilir ve tahsilat
          alabilirsiniz.
        </p>
      </div>
      <button
        @click="isAddModalOpen = true"
        type="button"
        class="flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-xl font-medium text-sm shadow-md shadow-blue-200 transition-all cursor-pointer"
      >
        <UserPlusIcon :size="18" /> Yeni Müşteri Ekle
      </button>
    </div>

    <div
      class="bg-white p-4 rounded-2xl border border-slate-100 shadow-sm flex flex-col md:flex-row gap-3"
    >
      <div class="relative flex-1">
        <SearchIcon class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" :size="18" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Müşteri adı, soyadı veya telefon numarası ile ara..."
          class="w-full pl-11 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-50 transition-all text-slate-700"
        />
      </div>
      <div class="flex gap-2 w-full md:w-auto">
        <CustomDropdown
          v-model="debtFilter"
          :options="debtFilterOptions"
          width-class="w-full md:w-56"
          @update:model-value="onDebtFilterChange"
        />
      </div>
    </div>

    <div
      class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden flex flex-col"
    >
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr
              class="bg-slate-50 border-b border-slate-100 text-xs font-bold uppercase tracking-wider text-slate-500"
            >
              <th class="py-4 px-6">Müşteri Bilgisi</th>
              <th class="py-4 px-6">Telefon Numarası</th>
              <th class="py-4 px-6">Kayıt Tarihi</th>
              <th class="py-4 px-6 text-right">Mevcut Borç</th>
              <th class="py-4 px-6 text-center">İşlemler</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-sm text-slate-600">
            <tr v-if="isLoading">
              <td colspan="5" class="py-12 text-center text-slate-400 font-medium">
                Yükleniyor...
              </td>
            </tr>

            <tr v-else-if="customers.length === 0">
              <td colspan="5" class="py-12 text-center text-slate-400">
                <UsersIcon :size="48" class="mx-auto mb-3 text-slate-300 stroke-1" />
                <p class="font-medium text-base text-slate-500">Müşteri bulunamadı.</p>
              </td>
            </tr>

            <tr
              v-for="customer in customers"
              :key="customer.id"
              class="hover:bg-blue-50/30 transition-colors group"
            >
              <td class="py-4 px-6">
                <div class="flex items-center gap-3">
                  <div
                    class="w-10 h-10 rounded-xl bg-slate-100 text-slate-600 font-bold text-sm flex items-center justify-center group-hover:bg-blue-100 group-hover:text-blue-600 transition-colors border border-slate-200 group-hover:border-blue-200"
                  >
                    {{ customer.firstName.charAt(0) }}{{ customer.lastName.charAt(0) }}
                  </div>
                  <div>
                    <h4 class="font-semibold text-slate-800 text-sm">
                      {{ customer.firstName }} {{ customer.lastName }}
                    </h4>
                    <span class="text-[11px] text-slate-400"
                      >ID: #{{ customer.id.substring(0, 6) }}</span
                    >
                  </div>
                </div>
              </td>
              <td class="py-4 px-6 font-medium text-slate-600">
                <div class="flex items-center gap-2">
                  <PhoneIcon :size="14" class="text-slate-400" />
                  {{ customer.phoneNumber || 'Belirtilmemiş' }}
                </div>
              </td>
              <td class="py-4 px-6 text-xs text-slate-500 font-medium">
                {{ formatDate(customer.createdAt) }}
              </td>
              <td class="py-4 px-6 text-right">
                <span
                  class="font-bold text-sm px-3 py-1 rounded-lg inline-block border"
                  :class="
                    customer.currentBalance > 0
                      ? 'bg-red-50 text-red-600 border-red-100'
                      : 'bg-emerald-50 text-emerald-600 border-emerald-100'
                  "
                >
                  {{ formatCurrency(customer.currentBalance) }}
                </span>
              </td>
              <td class="py-4 px-6">
                <div class="flex items-center justify-center gap-1.5">
                  <button
                    @click="openPaymentModal(customer)"
                    type="button"
                    title="Tahsilat Al / Borç Düş"
                    class="p-2 rounded-lg transition-colors cursor-pointer"
                    :class="
                      customer.currentBalance > 0
                        ? 'text-emerald-500 hover:bg-emerald-50 hover:text-emerald-600'
                        : 'text-slate-300 cursor-not-allowed'
                    "
                    :disabled="customer.currentBalance <= 0"
                  >
                    <WalletIcon :size="18" />
                  </button>

                  <button
                    @click="openHistoryModal(customer)"
                    type="button"
                    title="Alışveriş Geçmişi"
                    class="p-2 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors cursor-pointer"
                  >
                    <HistoryIcon :size="18" />
                  </button>

                  <button
                    @click="openEditModal(customer)"
                    type="button"
                    title="Düzenle"
                    class="p-2 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors cursor-pointer"
                  >
                    <Edit2Icon :size="18" />
                  </button>

                  <div class="w-px h-4 bg-slate-200 mx-1"></div>

                  <button
                    @click="openDeleteModal(customer.id)"
                    type="button"
                    title="Sil"
                    class="p-2 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
                  >
                    <Trash2Icon :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div
        v-if="pagination.totalPages > 0"
        class="p-4 border-t border-slate-100 flex items-center justify-between bg-slate-50/50"
      >
        <span class="text-xs text-slate-500">
          Toplam <span class="font-bold text-slate-700">{{ pagination.totalItems }}</span> müşteri
        </span>
        <div class="flex items-center gap-2">
          <button
            @click="changePage(pagination.page - 1)"
            :disabled="pagination.page === 1"
            class="px-3 py-1.5 rounded-lg border border-slate-200 bg-white text-slate-600 text-xs font-bold hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer transition-colors"
          >
            Önceki
          </button>
          <span class="text-xs font-bold text-slate-700"
            >Sayfa {{ pagination.page }} / {{ pagination.totalPages }}</span
          >
          <button
            @click="changePage(pagination.page + 1)"
            :disabled="pagination.page === pagination.totalPages"
            class="px-3 py-1.5 rounded-lg border border-slate-200 bg-white text-slate-600 text-xs font-bold hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer transition-colors"
          >
            Sonraki
          </button>
        </div>
      </div>
    </div>

    <AddCustomerModal
      :is-open="isAddModalOpen"
      @close="isAddModalOpen = false"
      @save="handleAddCustomer"
    />
    <CustomerPaymentModal
      :is-open="isPaymentModalOpen"
      :customer="selectedCustomer"
      @close="isPaymentModalOpen = false"
      @save="handlePayment"
    />
    <EditCustomerModal
      :is-open="isEditModalOpen"
      :customer="selectedCustomer"
      @close="isEditModalOpen = false"
      @save="handleUpdateCustomer"
    />
    <DeleteCustomerModal
      :is-open="isDeleteModalOpen"
      @close="isDeleteModalOpen = false"
      @confirm="confirmDeleteCustomer"
    />

    <CustomerHistoryModal
      :is-open="isHistoryModalOpen"
      :customer="selectedCustomerForHistory"
      @close="isHistoryModalOpen = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import axios from 'axios'
import {
  UserPlusIcon,
  SearchIcon,
  UsersIcon,
  PhoneIcon,
  Trash2Icon,
  WalletIcon,
  Edit2Icon,
  HistoryIcon,
} from 'lucide-vue-next'

import CustomDropdown from '@/components/CustomDropdown.vue'
import AddCustomerModal from './AddCustomerModal.vue'
import CustomerPaymentModal from './CustomerPaymentModal.vue'
import EditCustomerModal from './EditCustomerModal.vue'
import DeleteCustomerModal from './DeleteCustomerModal.vue'
import CustomerHistoryModal from './CustomerHistoryModal.vue'
import { useAlert } from '@/composables/useAlert'

const { showError } = useAlert()

const customers = ref<any[]>([])
const isLoading = ref(false)

const isAddModalOpen = ref(false)
const isPaymentModalOpen = ref(false)
const isEditModalOpen = ref(false)
const isDeleteModalOpen = ref(false)
const isHistoryModalOpen = ref(false) // YENİ EKLENDİ

const selectedCustomer = ref<any>(null)
const selectedCustomerForHistory = ref<any>(null) // YENİ EKLENDİ
const customerToDeleteId = ref<string | null>(null)

const searchQuery = ref('')
const debtFilter = ref('all')

const onDebtFilterChange = () => {
  pagination.value.page = 1
  fetchCustomers()
}

const debtFilterOptions = [
  { label: 'Tüm Müşteriler', value: 'all' },
  { label: 'Borcu Olanlar', value: 'has_debt' },
  { label: 'Borcu Olmayanlar (Temiz)', value: 'no_debt' },
]
const pagination = ref({ page: 1, pageSize: 10, totalPages: 0, totalItems: 0 })

const API_URL = 'Customers'

const getAuthHeaders = () => {
  return { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
}

const fetchCustomers = async () => {
  isLoading.value = true
  try {
    const params = {
      page: pagination.value.page,
      pageSize: pagination.value.pageSize,
      search: searchQuery.value,
      debtFilter: debtFilter.value,
    }
    const response = await axios.get(API_URL, { params, ...getAuthHeaders() })
    customers.value = response.data.items
    pagination.value.totalPages = response.data.totalPages
    pagination.value.totalItems = response.data.totalItems
  } catch (error) {
    console.error('Müşteriler çekilirken hata oluştu:', error)
  } finally {
    isLoading.value = false
  }
}

const changePage = (newPage: number) => {
  if (newPage > 0 && newPage <= pagination.value.totalPages) {
    pagination.value.page = newPage
    fetchCustomers()
  }
}

let searchTimeout: ReturnType<typeof setTimeout>
watch(searchQuery, () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    pagination.value.page = 1
    fetchCustomers()
  }, 500)
})


const handleAddCustomer = async (formData: any) => {
  try {
    await axios.post(API_URL, formData, getAuthHeaders())
    isAddModalOpen.value = false
    await fetchCustomers()
  } catch (error) {
    showError('Müşteri eklenemedi!')
  }
}

const handleUpdateCustomer = async ({ id, payload }: any) => {
  try {
    await axios.put(`${API_URL}/${id}`, payload, getAuthHeaders())
    isEditModalOpen.value = false
    await fetchCustomers()
  } catch (error) {
    console.error('Müşteri güncellenemedi:', error)
    showError('Müşteri güncellenirken bir hata oluştu!')
  }
}

const handlePayment = async ({ customerId, payload }: any) => {
  try {
    await axios.post(`${API_URL}/${customerId}/payment`, payload, getAuthHeaders())
    isPaymentModalOpen.value = false
    await fetchCustomers()
  } catch (error) {
    showError('Tahsilat eklenirken hata oluştu!')
  }
}

const openPaymentModal = (customer: any) => {
  if (customer.currentBalance > 0) {
    selectedCustomer.value = customer
    isPaymentModalOpen.value = true
  }
}

const openEditModal = (customer: any) => {
  selectedCustomer.value = { ...customer }
  isEditModalOpen.value = true
}

const openDeleteModal = (id: string) => {
  customerToDeleteId.value = id
  isDeleteModalOpen.value = true
}

// YENİ EKLENDİ: Geçmiş modalını açan fonksiyon
const openHistoryModal = (customer: any) => {
  selectedCustomerForHistory.value = customer
  isHistoryModalOpen.value = true
}

const confirmDeleteCustomer = async () => {
  if (!customerToDeleteId.value) return

  try {
    await axios.delete(`${API_URL}/${customerToDeleteId.value}`, getAuthHeaders())
    isDeleteModalOpen.value = false
    customerToDeleteId.value = null
    await fetchCustomers()
  } catch (error) {
    console.error('Silme hatası:', error)
    showError('Müşteri silinemedi!')
  }
}

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value)
const formatDate = (dateString: string) =>
  new Intl.DateTimeFormat('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' }).format(
    new Date(dateString),
  )

onMounted(() => {
  fetchCustomers()
})
</script>
