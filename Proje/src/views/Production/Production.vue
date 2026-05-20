<template>
  <div class="space-y-5">
    <!-- Tab Başlıkları -->
    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-1.5 flex gap-1.5">
      <button
        @click="activeTab = 'raw-materials'"
        :class="[
          'flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-medium transition-all',
          activeTab === 'raw-materials'
            ? 'bg-blue-600 text-white shadow-sm'
            : 'text-slate-500 hover:bg-slate-100 hover:text-slate-700',
        ]"
        type="button"
      >
        📦 Hammadde Deposu
      </button>
      <button
        @click="activeTab = 'production'"
        :class="[
          'flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-medium transition-all',
          activeTab === 'production'
            ? 'bg-blue-600 text-white shadow-sm'
            : 'text-slate-500 hover:bg-slate-100 hover:text-slate-700',
        ]"
        type="button"
      >
        ⚙️ Üretim Paneli
      </button>
      <button
        @click="activeTab = 'history'"
        :class="[
          'flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-medium transition-all',
          activeTab === 'history'
            ? 'bg-blue-600 text-white shadow-sm'
            : 'text-slate-500 hover:bg-slate-100 hover:text-slate-700',
        ]"
        type="button"
      >
        🕓 Üretim Geçmişi
      </button>
    </div>

    <!-- Tab İçerikleri -->
    <RawMaterialTab v-if="activeTab === 'raw-materials'" />
    <ProductionTab v-if="activeTab === 'production'" />
    <ProductionHistoryTab v-if="activeTab === 'history'" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import RawMaterialTab from './RawMaterialTab.vue'
import ProductionTab from './ProductionTab.vue'
import ProductionHistoryTab from './ProductionHistoryTab.vue'

defineOptions({ name: 'ProductionView' })

const activeTab = ref('raw-materials')
</script>
