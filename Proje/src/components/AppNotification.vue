<template>
  <Teleport to="body">
    <Transition name="notification">
      <div
        v-if="notification.visible"
        class="fixed inset-0 z-[9999] flex items-center justify-center bg-gray-950/40 backdrop-blur-sm p-4"
        @click.self="_dismissNotification"
      >
        <div
          :class="[
            'relative w-full max-w-sm rounded-2xl border shadow-2xl p-6 text-center',
            config.containerClass,
          ]"
        >
          <div
            :class="['w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4', config.iconBgClass]"
          >
            <component :is="config.icon" :size="32" :class="config.iconClass" />
          </div>

          <h3 :class="['text-lg font-bold mb-2', config.titleClass]">{{ config.title }}</h3>
          <p :class="['text-sm leading-relaxed', config.messageClass]">{{ notification.message }}</p>

          <button
            @click="_dismissNotification"
            :class="['mt-5 w-full py-2.5 rounded-xl text-sm font-bold transition-colors', config.buttonClass]"
          >
            Tamam
          </button>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, watch, ref } from 'vue'
import { CheckCircleIcon, AlertTriangleIcon, XCircleIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { notification, _dismissNotification } = useAlert()

let autoCloseTimer: ReturnType<typeof setTimeout> | null = null

const config = computed(() => {
  switch (notification.type) {
    case 'success':
      return {
        containerClass: 'bg-emerald-50 border-emerald-200',
        iconBgClass: 'bg-emerald-100',
        iconClass: 'text-emerald-600',
        icon: CheckCircleIcon,
        title: 'Başarılı',
        titleClass: 'text-emerald-800',
        messageClass: 'text-emerald-700',
        buttonClass: 'bg-emerald-600 hover:bg-emerald-700 text-white',
      }
    case 'warning':
      return {
        containerClass: 'bg-amber-50 border-amber-200',
        iconBgClass: 'bg-amber-100',
        iconClass: 'text-amber-600',
        icon: AlertTriangleIcon,
        title: 'Uyarı',
        titleClass: 'text-amber-800',
        messageClass: 'text-amber-700',
        buttonClass: 'bg-amber-500 hover:bg-amber-600 text-white',
      }
    case 'error':
    default:
      return {
        containerClass: 'bg-red-50 border-red-200',
        iconBgClass: 'bg-red-100',
        iconClass: 'text-red-600',
        icon: XCircleIcon,
        title: 'Hata',
        titleClass: 'text-red-800',
        messageClass: 'text-red-700',
        buttonClass: 'bg-red-600 hover:bg-red-700 text-white',
      }
  }
})

watch(
  () => notification.visible,
  (val) => {
    if (autoCloseTimer) clearTimeout(autoCloseTimer)
    if (val) {
      autoCloseTimer = setTimeout(() => _dismissNotification(), 4000)
    }
  },
)
</script>

<style scoped>
.notification-enter-active,
.notification-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.notification-enter-from,
.notification-leave-to {
  opacity: 0;
  transform: scale(0.95);
}
</style>
