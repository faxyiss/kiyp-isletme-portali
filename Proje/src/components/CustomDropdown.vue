<template>
  <div class="relative" :class="widthClass" @click.stop>
    <button
      @click="isOpen = !isOpen"
      type="button"
      class="w-full flex items-center justify-between px-3 py-2.5 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20 font-medium"
    >
      <span class="truncate">{{ selectedLabel }}</span>
      <ChevronDownIcon
        class="text-slate-400 transition-transform duration-200 shrink-0 ml-2"
        :class="{ 'rotate-180': isOpen }"
        :size="16"
      />
    </button>

    <div v-if="isOpen" class="fixed inset-0 z-40" @click="isOpen = false"></div>

    <div
      v-if="isOpen"
      class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-2 flex flex-col"
      :class="searchable ? 'space-y-2 max-h-64' : 'space-y-0.5'"
    >
      <div v-if="searchable" class="relative flex-shrink-0">
        <SearchIcon class="absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400" :size="14" />
        <input
          v-model="searchText"
          type="text"
          :placeholder="searchPlaceholder"
          class="w-full pl-8 pr-3 py-1.5 border border-slate-200 rounded-lg focus:outline-none focus:border-blue-500 text-xs transition-all"
          @click.stop
        />
      </div>

      <div
        class="overflow-y-auto flex-1 space-y-0.5"
        :class="searchable ? 'max-h-44 custom-scrollbar' : ''"
      >
        <button
          v-for="opt in filteredOptions"
          :key="String(opt.value)"
          @click="select(opt.value)"
          type="button"
          class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors truncate"
          :class="
            modelValue === opt.value
              ? 'bg-blue-50 text-blue-600 font-bold'
              : 'text-slate-600 hover:bg-slate-50'
          "
        >
          {{ opt.label }}
        </button>
        <div v-if="filteredOptions.length === 0" class="text-center py-3 text-xs text-slate-400">
          Sonuç bulunamadı.
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { ChevronDownIcon, SearchIcon } from 'lucide-vue-next'

const props = withDefaults(
  defineProps<{
    options: { label: string; value: any }[]
    modelValue: any
    placeholder?: string
    searchable?: boolean
    searchPlaceholder?: string
    widthClass?: string
  }>(),
  {
    placeholder: 'Seçiniz',
    searchable: false,
    searchPlaceholder: 'Ara...',
    widthClass: 'w-full',
  },
)

const emit = defineEmits<{ (e: 'update:modelValue', value: any): void }>()

const isOpen = ref(false)
const searchText = ref('')

const selectedLabel = computed(
  () => props.options.find((o) => o.value === props.modelValue)?.label ?? props.placeholder,
)

const filteredOptions = computed(() => {
  if (!props.searchable || !searchText.value) return props.options
  const q = searchText.value.toLowerCase()
  return props.options.filter((o) => o.label.toLowerCase().includes(q))
})

const select = (value: any) => {
  emit('update:modelValue', value)
  isOpen.value = false
  searchText.value = ''
}
</script>
