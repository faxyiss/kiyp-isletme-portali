<template>
  <div class="flex h-screen bg-slate-50 text-slate-800 antialiased">
    <aside class="w-64 bg-white border-r border-slate-100 flex flex-col justify-between p-6">
      <div>
        <div class="px-2 mb-8">
          <div class="flex items-center gap-3">
            <div class="bg-blue-600 text-white w-10 h-10 rounded-xl shadow-md shadow-blue-200 flex items-center justify-center font-black text-lg shrink-0">
              K
            </div>
            <div>
              <span class="font-black text-base tracking-tight text-slate-800">KIYP</span>
              <p class="text-[10px] text-slate-400 font-medium leading-tight">Küçük İşletme Yönetim Portalı</p>
            </div>
          </div>
          <div class="mt-3 px-1">
            <p class="text-[11px] text-slate-400 font-medium truncate">{{ profileData.companyName }}</p>
          </div>
        </div>

        <nav class="space-y-1">
          <router-link
            v-for="item in menuItems"
            :key="item.path"
            :to="item.path"
            class="flex items-center gap-3 px-4 py-3 rounded-xl font-medium text-sm transition-all duration-200 group"
            :class="
              $route.path === item.path
                ? 'bg-blue-50 text-blue-600'
                : 'text-slate-500 hover:bg-slate-50 hover:text-slate-800'
            "
          >
            <component
              :is="item.icon"
              :size="18"
              :class="
                $route.path === item.path
                  ? 'text-blue-600'
                  : 'text-slate-400 group-hover:text-slate-600'
              "
            />
            {{ item.name }}
          </router-link>
        </nav>
      </div>

      <div class="border-t border-slate-100 pt-4 flex items-center justify-between relative">
        <div class="flex items-center gap-3">
          <div
            class="w-10 h-10 rounded-xl bg-linear-to-r from-blue-500 to-indigo-500 text-white flex items-center justify-center font-bold shadow-md shadow-blue-100"
          >
            {{ profileData.initial }}
          </div>
          <div>
            <h4 class="text-xs font-bold text-slate-800">{{ profileData.fullName }}</h4>
            <span class="text-[10px] text-slate-400 font-medium">{{
              profileData.roleDisplay
            }}</span>
          </div>
        </div>

        <div class="flex items-center gap-0.5">
          <div class="relative">
            <button
              @click="isSettingsOpen = !isSettingsOpen"
              type="button"
              class="text-slate-400 hover:text-blue-600 p-2 rounded-lg hover:bg-blue-50 transition-colors"
              title="Ayarlar"
            >
              <SettingsIcon :size="18" />
            </button>

            <div
              v-if="isSettingsOpen"
              class="fixed inset-0 z-40"
              @click="isSettingsOpen = false"
            ></div>

            <div
              v-if="isSettingsOpen"
              class="absolute bottom-full right-0 mb-2 w-48 bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 space-y-0.5"
            >
              <button
                @click="handleUpdateProfile"
                type="button"
                class="w-full text-left px-3 py-2 rounded-lg text-xs font-medium text-slate-600 hover:bg-slate-50 hover:text-slate-800 transition-colors flex items-center gap-2"
              >
                <UserIcon :size="14" class="text-slate-400" />
                Profili Güncelle
              </button>

              <button
                @click="handleDeleteUser"
                type="button"
                class="w-full text-left px-3 py-2 rounded-lg text-xs font-medium text-red-600 hover:bg-red-50 hover:text-red-700 transition-colors flex items-center gap-2"
              >
                <UserXIcon :size="14" class="text-red-400" />
                Kullanıcıyı Sil
              </button>
            </div>
          </div>

          <router-link
            to="/login"
            class="text-slate-400 hover:text-red-500 p-2 rounded-lg hover:bg-red-50 transition-colors"
            @click="handleLogout"
          >
            <LogOutIcon :size="18" />
          </router-link>
        </div>
      </div>
    </aside>

    <main class="flex-1 flex flex-col h-screen overflow-hidden">
      <header
        class="h-16 bg-white border-b border-slate-100 flex items-center justify-between px-8"
      >
        <h1 class="text-lg font-bold text-slate-800 capitalize">
          {{ currentRouteName }}
        </h1>
        <div class="text-sm text-slate-400 font-medium">
          {{ currentDate }}
        </div>
      </header>

      <div class="flex-1 overflow-y-auto p-8">
        <router-view></router-view>
      </div>
    </main>

    <BusinessProfileModal />

    <UpdateProfileModal
      :is-open="isUpdateProfileOpen"
      :current-data="{ fullName: profileData.fullName, companyName: profileData.companyName }"
      @close="isUpdateProfileOpen = false"
      @success="fetchProfile"
    />

    <DeleteAccountModal :is-open="isDeleteAccountOpen" @close="isDeleteAccountOpen = false" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'
import { useRoute } from 'vue-router'
import {
  LayoutDashboardIcon,
  PackageIcon,
  ShoppingCartIcon,
  LayersIcon,
  UsersIcon,
  LogOutIcon,
  SettingsIcon,
  UserIcon,
  UserXIcon,
  WalletIcon,
  BriefcaseIcon,
  BarChart3Icon,
} from 'lucide-vue-next'

import BusinessProfileModal from '@/views/Components/BusinessProfileModal.vue'
import UpdateProfileModal from '@/views/Components/UpdateProfileModal.vue'
import DeleteAccountModal from '@/views/Components/DeleteAccountModal.vue'

const route = useRoute()

const isSettingsOpen = ref(false)
const isUpdateProfileOpen = ref(false)
const isDeleteAccountOpen = ref(false)

const profileData = ref({
  companyName: 'Yükleniyor...',
  fullName: 'Kullanıcı',
  roleDisplay: '...',
  initial: '',
})

const fetchProfile = async () => {
  try {
    const res = await axios.get('/BusinessProfile/my-profile', {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    profileData.value.companyName = res.data.companyName
    profileData.value.fullName = res.data.fullName
    profileData.value.roleDisplay = res.data.role === 'Admin' ? 'Yönetici' : 'Personel'
    profileData.value.initial = res.data.initial
  } catch (error) {
    console.error('Profil bilgileri yüklenirken hata oluştu:', error)
  }
}

onMounted(() => {
  fetchProfile()
})

const handleLogout = () => {
  localStorage.removeItem('token')
}

const handleUpdateProfile = () => {
  isSettingsOpen.value = false
  isUpdateProfileOpen.value = true
}

const handleDeleteUser = () => {
  isSettingsOpen.value = false
  isDeleteAccountOpen.value = true
}

const menuItems = [
  { name: 'Ana Sayfa', path: '/dashboard', icon: LayoutDashboardIcon },
  { name: 'Stok Yönetimi', path: '/stocks', icon: PackageIcon },
  { name: 'Satış İşlemleri', path: '/sales', icon: ShoppingCartIcon },
  { name: 'Hammadde ve Üretim', path: '/production', icon: LayersIcon },
  { name: 'Müşteri & Cari', path: '/customers', icon: UsersIcon },
  { name: 'Gider Yönetimi', path: '/expenses', icon: WalletIcon },
  { name: 'Personel Yönetimi', path: '/employees', icon: BriefcaseIcon },
  { name: 'Analiz Merkezi', path: '/analytics', icon: BarChart3Icon },
]

const currentRouteName = computed(() => {
  const item = menuItems.find((m) => m.path === route.path)
  return item ? item.name : 'Panel'
})

const currentDate = computed(() => {
  return new Date().toLocaleDateString('tr-TR', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
})
</script>

<style scoped>
/* Özel stiller */
</style>
