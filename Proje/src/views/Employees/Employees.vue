<template>
  <div class="space-y-6">
    <!-- Özet Kartlar -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div
        class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center gap-4"
      >
        <div class="bg-blue-50 p-3 rounded-xl flex-shrink-0">
          <UsersIcon :size="20" class="text-blue-500" />
        </div>
        <div>
          <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">Aktif Personel</p>
          <p class="text-xl font-bold text-slate-800 mt-0.5">{{ summary.activeEmployees }}</p>
        </div>
      </div>

      <div
        class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center gap-4"
      >
        <div class="bg-emerald-50 p-3 rounded-xl flex-shrink-0">
          <WalletIcon :size="20" class="text-emerald-500" />
        </div>
        <div>
          <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">
            Toplam Net Maaş
          </p>
          <p class="text-xl font-bold text-slate-800 mt-0.5">
            {{ formatCurrency(summary.totalNetSalary) }}
          </p>
        </div>
      </div>

      <div
        class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center gap-4"
      >
        <div class="bg-red-50 p-3 rounded-xl flex-shrink-0">
          <TrendingUpIcon :size="20" class="text-red-500" />
        </div>
        <div>
          <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">
            İşveren Maliyeti
          </p>
          <p class="text-xl font-bold text-slate-800 mt-0.5">
            {{ formatCurrency(summary.totalEmployerCost) }}
          </p>
        </div>
      </div>

      <div
        class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center gap-4"
      >
        <div class="bg-amber-50 p-3 rounded-xl flex-shrink-0">
          <ReceiptIcon :size="20" class="text-amber-500" />
        </div>
        <div>
          <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">
            Vergi & SGK Yükü
          </p>
          <p class="text-xl font-bold text-slate-800 mt-0.5">
            {{ formatCurrency(summary.totalTaxAndSGK) }}
          </p>
        </div>
      </div>
    </div>

    <!-- Filtre & Aksiyon Barı -->
    <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <!-- Arama -->
        <div class="relative w-full md:w-80">
          <SearchIcon class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" :size="18" />
          <input
            v-model="searchText"
            type="text"
            placeholder="Personel adı veya görevi ara..."
            class="w-full pl-10 pr-4 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm"
          />
        </div>

        <div class="flex flex-wrap items-center gap-3 w-full md:w-auto z-30">
          <!-- Durum Filtresi -->
          <div class="relative w-full sm:w-44">
            <button
              @click="toggleDropdown('status')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ selectedStatusLabel }}</span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isStatusDropdownOpen }"
                :size="16"
              />
            </button>
            <div
              v-if="isStatusDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isStatusDropdownOpen = false"
            ></div>
            <div
              v-if="isStatusDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 space-y-0.5"
            >
              <button
                v-for="opt in statusOptions"
                :key="String(opt.value)"
                @click="selectStatus(opt.value)"
                type="button"
                class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                :class="
                  filterStatus === opt.value
                    ? 'bg-blue-50 text-blue-600 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                {{ opt.label }}
              </button>
            </div>
          </div>

          <!-- Ay Seçici (özet için) -->
          <div class="relative w-full sm:w-36">
            <button
              @click="toggleDropdown('month')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ selectedMonthLabel }}</span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isMonthDropdownOpen }"
                :size="16"
              />
            </button>
            <div
              v-if="isMonthDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isMonthDropdownOpen = false"
            ></div>
            <div
              v-if="isMonthDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 space-y-0.5 max-h-60 overflow-y-auto custom-scrollbar"
            >
              <button
                v-for="m in months"
                :key="m.value"
                @click="selectMonth(m.value)"
                type="button"
                class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                :class="
                  summaryMonth === m.value
                    ? 'bg-blue-50 text-blue-600 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                {{ m.label }}
              </button>
            </div>
          </div>

          <!-- Yıl Seçici -->
          <div class="relative w-full sm:w-28">
            <button
              @click="toggleDropdown('year')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ summaryYear }}</span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isYearDropdownOpen }"
                :size="16"
              />
            </button>
            <div
              v-if="isYearDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isYearDropdownOpen = false"
            ></div>
            <div
              v-if="isYearDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 space-y-0.5"
            >
              <button
                v-for="y in years"
                :key="y"
                @click="selectYear(y)"
                type="button"
                class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                :class="
                  summaryYear === y
                    ? 'bg-blue-50 text-blue-600 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                {{ y }}
              </button>
            </div>
          </div>
        </div>

        <!-- Yeni Personel Butonu -->
        <button
          @click="isAddEmployeeOpen = true"
          class="w-full md:w-auto flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-xl font-medium transition-colors text-sm shadow-sm shadow-blue-200"
        >
          <PlusIcon :size="18" />
          Personel Ekle
        </button>
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
              <th class="py-4 px-4 min-w-[220px]">Personel</th>
              <th class="py-4 px-4 min-w-[130px]">Görevi</th>
              <th class="py-4 px-4 min-w-[120px]">Telefon</th>
              <th class="py-4 px-4 min-w-[110px]">Başlangıç</th>
              <th class="py-4 px-4 min-w-[120px]">Brüt Maaş</th>
              <th class="py-4 px-4 min-w-[120px] text-emerald-700">Net Maaş</th>
              <th class="py-4 px-4 min-w-[130px] text-red-600">İşv. Maliyeti</th>
              <th class="py-4 px-4 min-w-[90px]">Durum</th>
              <th class="py-4 px-4 text-right min-w-[260px]">İşlemler</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-slate-700 text-sm">
            <tr v-if="isLoading">
              <td colspan="9" class="py-12 text-center text-slate-400 font-medium">
                Veriler sunucudan çekiliyor...
              </td>
            </tr>

            <tr v-else-if="filteredEmployees.length === 0">
              <td colspan="9" class="py-16 text-center">
                <div class="flex flex-col items-center gap-3 text-slate-400">
                  <div class="bg-slate-100 rounded-2xl p-5">
                    <UsersIcon :size="32" class="text-slate-300" />
                  </div>
                  <p class="font-semibold text-slate-500 text-sm">Personel bulunamadı</p>
                  <p class="text-xs text-slate-400">Yeni personel ekleyerek başlayın.</p>
                </div>
              </td>
            </tr>

            <tr
              v-else
              v-for="emp in filteredEmployees"
              :key="emp.id"
              class="hover:bg-slate-50/40 transition-colors"
            >
              <!-- Personel Bilgisi -->
              <td class="py-4 px-4 align-middle">
                <div class="flex items-center gap-3">
                  <div
                    class="flex-shrink-0 w-9 h-9 rounded-xl bg-linear-to-r from-blue-500 to-indigo-500 text-white flex items-center justify-center font-bold text-sm shadow-sm shadow-blue-100"
                  >
                    {{ emp.fullName.charAt(0).toUpperCase() }}
                  </div>
                  <div class="flex flex-col min-w-0">
                    <span class="font-bold text-slate-800 text-sm truncate">{{
                      emp.fullName
                    }}</span>
                    <span class="text-[11px] text-slate-400 font-medium mt-0.5">{{
                      emp.position
                    }}</span>
                  </div>
                </div>
              </td>

              <!-- Görevi -->
              <td class="py-4 px-4 align-middle whitespace-nowrap">
                <span
                  class="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-semibold bg-blue-50/70 text-blue-600 border border-blue-100/50"
                >
                  {{ emp.position }}
                </span>
              </td>

              <!-- Telefon -->
              <td
                class="py-4 px-4 align-middle text-slate-500 text-xs font-medium whitespace-nowrap"
              >
                {{ emp.phone || '—' }}
              </td>

              <!-- Başlangıç -->
              <td
                class="py-4 px-4 align-middle text-slate-500 text-xs font-medium whitespace-nowrap"
              >
                <div class="flex items-center gap-1">
                  <ClockIcon :size="12" class="text-slate-400" />
                  {{ formatDate(emp.startDate) }}
                </div>
              </td>

              <!-- Brüt Maaş -->
              <td class="py-4 px-4 align-middle font-bold text-slate-800 whitespace-nowrap">
                {{ formatCurrency(emp.grossSalary) }}
              </td>

              <!-- Net Maaş -->
              <td class="py-4 px-4 align-middle whitespace-nowrap">
                <div
                  class="px-2 py-0.5 rounded-md bg-emerald-50 border border-emerald-100 text-emerald-700 font-bold text-xs inline-block"
                >
                  {{ formatCurrency(emp.netSalary) }}
                </div>
              </td>

              <!-- İşveren Maliyeti -->
              <td class="py-4 px-4 align-middle whitespace-nowrap">
                <div
                  class="px-2 py-0.5 rounded-md bg-red-50 border border-red-100 text-red-600 font-bold text-xs inline-block"
                >
                  {{ formatCurrency(emp.totalEmployerCost) }}
                </div>
              </td>

              <!-- Durum -->
              <td class="py-4 px-4 align-middle whitespace-nowrap">
                <span
                  class="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-bold border"
                  :class="
                    emp.isActive
                      ? 'bg-emerald-50 text-emerald-600 border-emerald-100'
                      : 'bg-slate-100 text-slate-500 border-slate-200'
                  "
                >
                  {{ emp.isActive ? 'Aktif' : 'Pasif' }}
                </span>
              </td>

              <!-- İşlemler -->
              <td class="py-4 px-4 align-middle text-right whitespace-nowrap">
                <div class="flex items-center justify-end gap-1.5">
                  <button
                    @click="openPaySalary(emp)"
                    class="inline-flex items-center gap-1 bg-emerald-600 hover:bg-emerald-700 text-white px-2.5 py-1.5 rounded-lg text-xs font-bold transition-colors shadow-sm"
                  >
                    <WalletIcon :size="13" /> Maaş Öde
                  </button>

                  <button
                    @click="openLeave(emp)"
                    class="inline-flex items-center gap-1 bg-amber-500 hover:bg-amber-600 text-white px-2.5 py-1.5 rounded-lg text-xs font-bold transition-colors shadow-sm"
                  >
                    <CalendarIcon :size="13" /> İzin
                  </button>

                  <div class="h-4 w-[1px] bg-slate-200 mx-0.5"></div>

                  <button
                    @click="openEdit(emp)"
                    class="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                    title="Düzenle"
                  >
                    <EditIcon :size="15" />
                  </button>

                  <button
                    @click="handleDelete(emp)"
                    class="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                    title="Sil"
                  >
                    <Trash2Icon :size="15" />
                  </button>

                  <div class="h-4 w-[1px] bg-slate-200 mx-0.5"></div>

                  <button
                    @click="openDetail(emp)"
                    class="inline-flex items-center gap-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 border border-slate-200 px-2.5 py-1.5 rounded-lg text-xs font-bold transition-colors"
                  >
                    <ListIcon :size="13" /> Detay
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Footer -->
      <div
        class="border-t border-slate-100 px-6 py-4 flex items-center justify-between text-xs text-slate-400 bg-slate-50/30 font-medium"
      >
        <span
          >Toplam <b class="text-slate-600">{{ employees.length }}</b> personel,
          <b class="text-slate-600">{{ filteredEmployees.length }}</b> tanesi listeleniyor.
        </span>
      </div>
    </div>

    <!-- Modaller -->
    <AddEmployeeModal
      :is-open="isAddEmployeeOpen"
      @close="isAddEmployeeOpen = false"
      @success="fetchEmployees"
    />

    <EditEmployeeModal
      :is-open="isEditEmployeeOpen"
      :employee="selectedEmployee"
      @close="isEditEmployeeOpen = false"
      @success="fetchEmployees"
    />

    <PaySalaryModal
      :is-open="isPaySalaryOpen"
      :employee="selectedEmployee"
      @close="isPaySalaryOpen = false"
      @success="fetchSummary"
    />

    <AddLeaveModal
      :is-open="isAddLeaveOpen"
      :employee="selectedEmployee"
      @close="isAddLeaveOpen = false"
      @success="fetchSummary"
    />

    <EmployeeDetailModal
      :is-open="isDetailOpen"
      :employee="selectedEmployee"
      @close="isDetailOpen = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  UsersIcon,
  WalletIcon,
  TrendingUpIcon,
  ReceiptIcon,
  SearchIcon,
  PlusIcon,
  ChevronDownIcon,
  ClockIcon,
  EditIcon,
  Trash2Icon,
  ListIcon,
  CalendarIcon,
} from 'lucide-vue-next'

import AddEmployeeModal from './AddEmployeeModal.vue'
import EditEmployeeModal from './EditEmployeeModal.vue'
import PaySalaryModal from './PaySalaryModal.vue'
import AddLeaveModal from './AddLeaveModal.vue'
import EmployeeDetailModal from './EmployeeDetailModal.vue'
import { useAlert } from '@/composables/useAlert'

const { showError, showConfirm } = useAlert()

const BASE_URL = '/api'

// ── State ──────────────────────────────────────────────────
const isLoading = ref(false)
const employees = ref<any[]>([])
const searchText = ref('')
const filterStatus = ref<boolean | null>(null)

const isAddEmployeeOpen = ref(false)
const isEditEmployeeOpen = ref(false)
const isPaySalaryOpen = ref(false)
const isAddLeaveOpen = ref(false)
const isDetailOpen = ref(false)
const selectedEmployee = ref<any>(null)

const now = new Date()
const summaryMonth = ref(now.getMonth() + 1)
const summaryYear = ref(now.getFullYear())

const summary = ref({
  totalEmployees: 0,
  activeEmployees: 0,
  totalGrossSalary: 0,
  totalNetSalary: 0,
  totalEmployerCost: 0,
  totalTaxAndSGK: 0,
  totalLeaveDays: 0,
})

// ── Dropdown State ─────────────────────────────────────────
const isStatusDropdownOpen = ref(false)
const isMonthDropdownOpen = ref(false)
const isYearDropdownOpen = ref(false)

const toggleDropdown = (t: 'status' | 'month' | 'year') => {
  isStatusDropdownOpen.value = t === 'status' ? !isStatusDropdownOpen.value : false
  isMonthDropdownOpen.value = t === 'month' ? !isMonthDropdownOpen.value : false
  isYearDropdownOpen.value = t === 'year' ? !isYearDropdownOpen.value : false
}

// ── Statik Veriler ─────────────────────────────────────────
const months = [
  { value: 1, label: 'Ocak' },
  { value: 2, label: 'Şubat' },
  { value: 3, label: 'Mart' },
  { value: 4, label: 'Nisan' },
  { value: 5, label: 'Mayıs' },
  { value: 6, label: 'Haziran' },
  { value: 7, label: 'Temmuz' },
  { value: 8, label: 'Ağustos' },
  { value: 9, label: 'Eylül' },
  { value: 10, label: 'Ekim' },
  { value: 11, label: 'Kasım' },
  { value: 12, label: 'Aralık' },
]
const years = Array.from({ length: 5 }, (_, i) => now.getFullYear() - 2 + i)

const statusOptions = [
  { value: null, label: 'Tüm Personel' },
  { value: true, label: 'Sadece Aktif' },
  { value: false, label: 'Sadece Pasif' },
]

// ── Computed ───────────────────────────────────────────────
const selectedMonthLabel = computed(
  () => months.find((m) => m.value === summaryMonth.value)?.label ?? 'Ay',
)
const selectedStatusLabel = computed(
  () => statusOptions.find((o) => o.value === filterStatus.value)?.label ?? 'Tüm Personel',
)

const filteredEmployees = computed(() => {
  let list = employees.value
  if (filterStatus.value !== null) list = list.filter((e) => e.isActive === filterStatus.value)
  if (searchText.value.trim()) {
    const s = searchText.value.toLowerCase()
    list = list.filter(
      (e) => e.fullName.toLowerCase().includes(s) || e.position.toLowerCase().includes(s),
    )
  }
  return list
})

// ── Seçimler ───────────────────────────────────────────────
const selectStatus = (v: boolean | null) => {
  filterStatus.value = v
  isStatusDropdownOpen.value = false
}
const selectMonth = (v: number) => {
  summaryMonth.value = v
  isMonthDropdownOpen.value = false
  fetchSummary()
}
const selectYear = (v: number) => {
  summaryYear.value = v
  isYearDropdownOpen.value = false
  fetchSummary()
}

// ── Helpers ────────────────────────────────────────────────
const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(v ?? 0)

const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' })

// ── API ────────────────────────────────────────────────────
const fetchEmployees = async () => {
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${BASE_URL}/Employees`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (res.ok) employees.value = await res.json()
  } catch {
    console.error('Personeller çekilemedi.')
  } finally {
    isLoading.value = false
  }
}

const fetchSummary = async () => {
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(
      `${BASE_URL}/Employees/summary?month=${summaryMonth.value}&year=${summaryYear.value}`,
      { headers: { Authorization: `Bearer ${token}` } },
    )
    if (res.ok) summary.value = await res.json()
  } catch {
    console.error('Özet çekilemedi.')
  }
}

const handleDelete = async (emp: any) => {
  if (!(await showConfirm(`"${emp.fullName}" isimli personeli silmek istediğinize emin misiniz?`))) return
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${BASE_URL}/Employees/${emp.id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
    })
    const data = await res.json()
    if (res.ok) {
      fetchEmployees()
      fetchSummary()
    } else showError(data.message || 'Silme başarısız.')
  } catch {
    showError('Sunucuyla bağlantı kurulamadı.')
  }
}

// ── Modal Açıcılar ─────────────────────────────────────────
const openEdit = (emp: any) => {
  selectedEmployee.value = emp
  isEditEmployeeOpen.value = true
}
const openPaySalary = (emp: any) => {
  selectedEmployee.value = emp
  isPaySalaryOpen.value = true
}
const openLeave = (emp: any) => {
  selectedEmployee.value = emp
  isAddLeaveOpen.value = true
}
const openDetail = (emp: any) => {
  selectedEmployee.value = emp
  isDetailOpen.value = true
}

onMounted(() => {
  fetchEmployees()
  fetchSummary()
})
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
