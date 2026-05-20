<template>
  <div class="space-y-6">
    <!-- Üst bar: zaman filtresi -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-xl font-bold text-slate-800">Analiz Merkezi</h2>
        <p class="text-sm text-slate-400 mt-0.5">İşletmenizin tüm verilerini tek ekranda görüntüleyin</p>
      </div>
      <div class="flex items-center gap-1 bg-white border border-slate-200 rounded-xl p-1">
        <button
          v-for="r in ranges"
          :key="r.value"
          @click="selectedRange = r.value"
          class="px-4 py-2 rounded-lg text-sm font-medium transition-all duration-150"
          :class="selectedRange === r.value
            ? 'bg-blue-600 text-white shadow-sm'
            : 'text-slate-500 hover:text-slate-700 hover:bg-slate-50'"
        >
          {{ r.label }}
        </button>
      </div>
    </div>

    <!-- Sekme navigation -->
    <div class="flex items-center gap-1 bg-white border border-slate-200 rounded-xl p-1 overflow-x-auto">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        @click="activeTab = tab.value"
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all duration-150 whitespace-nowrap"
        :class="activeTab === tab.value
          ? 'bg-slate-800 text-white shadow-sm'
          : 'text-slate-500 hover:text-slate-700 hover:bg-slate-50'"
      >
        <component :is="tab.icon" :size="15" />
        {{ tab.label }}
      </button>
    </div>

    <!-- Aktif sekme içeriği -->
    <component :is="activeTabComponent" :range="selectedRange" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, defineAsyncComponent } from 'vue'
import {
  LayoutDashboardIcon,
  TrendingUpIcon,
  PackageIcon,
  UsersIcon,
  WalletIcon,
  LayersIcon,
  BriefcaseIcon,
} from 'lucide-vue-next'

const OverviewTab    = defineAsyncComponent(() => import('./tabs/OverviewTab.vue'))
const SalesTab       = defineAsyncComponent(() => import('./tabs/SalesTab.vue'))
const StockTab       = defineAsyncComponent(() => import('./tabs/StockTab.vue'))
const CustomerTab    = defineAsyncComponent(() => import('./tabs/CustomerTab.vue'))
const ExpensesTab    = defineAsyncComponent(() => import('./tabs/ExpensesTab.vue'))
const ProductionTab  = defineAsyncComponent(() => import('./tabs/ProductionTab.vue'))
const HRTab          = defineAsyncComponent(() => import('./tabs/HRTab.vue'))

const ranges = [
  { label: 'Son 7G',  value: '7d' },
  { label: 'Son 30G', value: '30d' },
  { label: 'Son 90G', value: '90d' },
  { label: 'Bu Yıl',  value: 'year' },
]

const tabs = [
  { label: 'Genel Bakış',  value: 'overview',    icon: LayoutDashboardIcon },
  { label: 'Satış',        value: 'sales',        icon: TrendingUpIcon },
  { label: 'Stok',         value: 'stock',        icon: PackageIcon },
  { label: 'Müşteri',      value: 'customer',     icon: UsersIcon },
  { label: 'Gider',        value: 'expenses',     icon: WalletIcon },
  { label: 'Üretim',       value: 'production',   icon: LayersIcon },
  { label: 'Personel',     value: 'hr',           icon: BriefcaseIcon },
]

const tabComponentMap: Record<string, ReturnType<typeof defineAsyncComponent>> = {
  overview:   OverviewTab,
  sales:      SalesTab,
  stock:      StockTab,
  customer:   CustomerTab,
  expenses:   ExpensesTab,
  production: ProductionTab,
  hr:         HRTab,
}

const selectedRange = ref('30d')
const activeTab = ref('overview')

const activeTabComponent = computed(() => tabComponentMap[activeTab.value])
</script>
