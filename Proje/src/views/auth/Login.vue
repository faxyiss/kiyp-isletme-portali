<template>
  <div ref="pageEl" class="login-page" :class="{ 'page-exit': isLeaving }">

    <!-- ═══════════════ HERO SECTION ═══════════════ -->
    <section
      class="relative min-h-screen flex overflow-hidden"
      style="background: linear-gradient(135deg, #060d1f 0%, #0d1f45 50%, #060d1f 100%)"
    >
      <div class="absolute top-1/4 left-1/3 w-125 h-125 bg-blue-600/15 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute bottom-1/4 right-1/4 w-80 h-80 bg-indigo-600/15 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute top-3/4 left-1/4 w-60 h-60 bg-cyan-600/10 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute inset-0 hero-grid opacity-20 pointer-events-none"></div>

      <!-- ─── Sol: Branding ─── -->
      <div class="flex-1 flex flex-col justify-center px-10 lg:px-20 z-10 py-16">
        <div class="flex items-center gap-3 mb-10 anim-up" style="--d:0.05s">
          <div class="w-13 h-13 bg-blue-600 rounded-2xl flex items-center justify-center shadow-lg shadow-blue-900/50 shrink-0" style="width:52px;height:52px">
            <span class="text-white font-black text-2xl leading-none">K</span>
          </div>
          <div>
            <p class="text-white font-black text-2xl leading-none tracking-tight">KIYP</p>
            <p class="text-blue-400 text-xs font-medium mt-1">Küçük İşletme Yönetim Portalı</p>
          </div>
        </div>

        <h1 class="text-4xl lg:text-5xl font-black text-white leading-tight mb-5 anim-up" style="--d:0.15s">
          İşletmenizi<br />
          <span class="gradient-text gradient-text-animate">akıllıca</span><br />
          yönetin.
        </h1>

        <p class="text-slate-400 text-base lg:text-lg mb-8 max-w-md leading-relaxed anim-up" style="--d:0.25s">
          Stoktan satışa, personelden üretime — tüm işletme verilerinizi tek ekrandan takip edin ve veri odaklı kararlar alın.
        </p>

        <!-- Feature chips — hover efektli -->
        <div class="flex flex-wrap gap-2 mb-10 anim-up" style="--d:0.35s">
          <span
            v-for="chip in featureChips" :key="chip"
            class="feature-chip px-3 py-1.5 bg-white/8 border border-white/10 text-white/70 text-xs font-medium rounded-full backdrop-blur-sm cursor-default"
          >{{ chip }}</span>
        </div>

        <!-- Stats — hover efektli -->
        <div class="flex gap-10 anim-up" style="--d:0.45s">
          <div v-for="stat in heroStats" :key="stat.label" class="hero-stat group cursor-default">
            <p class="text-2xl font-black text-white group-hover:text-transparent group-hover:bg-clip-text transition-all duration-300"
               :style="{ 'background-image': 'linear-gradient(135deg,#60a5fa,#a78bfa)', 'WebkitBackgroundClip': 'text' }">
              {{ stat.value }}
            </p>
            <p class="text-slate-500 text-xs mt-0.5 group-hover:text-slate-400 transition-colors duration-200">{{ stat.label }}</p>
          </div>
        </div>
      </div>

      <!-- ─── Sağ: Mock Dashboard (YATAY) ─── -->
      <div class="hidden lg:flex flex-1 items-center justify-center z-10 p-8 xl:p-12">
        <div class="mock-card w-full max-w-md animate-float">

          <!-- Başlık -->
          <div class="flex items-center justify-between mb-4">
            <p class="text-white font-bold text-sm">Genel Bakış</p>
            <span class="text-[10px] text-blue-300 bg-blue-500/15 px-2.5 py-1 rounded-full font-medium">Son 12 Ay</span>
          </div>

          <!-- Ana grid: sol=grafik, sağ=stat kartlar -->
          <div class="grid grid-cols-2 gap-4 mb-4">
            <!-- Bar chart -->
            <div>
              <div class="flex items-end gap-0.5 h-20 mb-1">
                <div v-for="(bar, i) in mockBars" :key="i" class="flex-1 rounded-t-sm mock-bar"
                  :style="{ height: bar.h + '%', background: bar.isExpense ? 'rgba(239,68,68,0.45)' : 'linear-gradient(to top,#1d4ed8,#60a5fa)' }">
                </div>
              </div>
              <div class="flex gap-0.5">
                <div v-for="(lbl, i) in ['O','Ş','M','N','M','H','T','A','E','E','K','A']" :key="i"
                  class="flex-1 text-center text-[7px] text-white/25">{{ lbl }}</div>
              </div>
              <div class="flex items-center gap-3 mt-2.5">
                <div class="flex items-center gap-1">
                  <div class="w-2 h-2 rounded-sm bg-blue-500"></div>
                  <span class="text-[8px] text-white/40">Gelir</span>
                </div>
                <div class="flex items-center gap-1">
                  <div class="w-2 h-2 rounded-sm bg-red-500/60"></div>
                  <span class="text-[8px] text-white/40">Gider</span>
                </div>
              </div>
            </div>

            <!-- 2×2 Stat kartlar -->
            <div class="grid grid-cols-2 gap-2">
              <div class="mock-stat-card rounded-xl p-2.5 border" style="background:rgba(16,185,129,0.08);border-color:rgba(16,185,129,0.2)">
                <p class="text-[8px] text-emerald-400 font-semibold mb-1">Net Kâr</p>
                <p class="text-white font-black text-xs leading-none">₺159.5K</p>
                <p class="text-emerald-400 text-[8px] mt-1">↑ %18.3</p>
              </div>
              <div class="mock-stat-card rounded-xl p-2.5 border" style="background:rgba(59,130,246,0.08);border-color:rgba(59,130,246,0.2)">
                <p class="text-[8px] text-blue-400 font-semibold mb-1">Stok</p>
                <p class="text-white font-black text-xs leading-none">2.847</p>
                <p class="text-blue-400 text-[8px] mt-1">342 çeşit</p>
              </div>
              <div class="mock-stat-card rounded-xl p-2.5 border" style="background:rgba(245,158,11,0.08);border-color:rgba(245,158,11,0.2)">
                <p class="text-[8px] text-amber-400 font-semibold mb-1">Satış</p>
                <p class="text-white font-black text-xs leading-none">1.204</p>
                <p class="text-amber-400 text-[8px] mt-1">Bu ay</p>
              </div>
              <div class="mock-stat-card rounded-xl p-2.5 border" style="background:rgba(139,92,246,0.08);border-color:rgba(139,92,246,0.2)">
                <p class="text-[8px] text-violet-400 font-semibold mb-1">Müşteri</p>
                <p class="text-white font-black text-xs leading-none">348</p>
                <p class="text-violet-400 text-[8px] mt-1">Aktif</p>
              </div>
            </div>
          </div>

          <!-- Alt grid: tablo + progress -->
          <div class="grid grid-cols-2 gap-4">
            <!-- Mini tablo -->
            <div class="rounded-xl overflow-hidden border" style="border-color:rgba(255,255,255,0.06);background:rgba(255,255,255,0.03)">
              <div class="px-2.5 py-1.5 border-b" style="border-color:rgba(255,255,255,0.06)">
                <p class="text-[8px] text-slate-400 font-bold uppercase tracking-widest">En Çok Satan</p>
              </div>
              <div v-for="(row, i) in mockTableRows" :key="i"
                class="mock-table-row flex items-center justify-between px-2.5 py-1.5 border-b last:border-0"
                style="border-color:rgba(255,255,255,0.04)">
                <div class="flex items-center gap-1.5">
                  <div class="w-3.5 h-3.5 rounded-md flex items-center justify-center text-[7px] font-black text-white shrink-0"
                    :style="{ background: row.color }">{{ i+1 }}</div>
                  <span class="text-white/65 text-[9px]">{{ row.name }}</span>
                </div>
                <span class="text-[9px] font-bold text-white/45">{{ row.amount }}</span>
              </div>
            </div>

            <!-- Progress bars -->
            <div class="space-y-2.5 flex flex-col justify-center">
              <div v-for="prog in mockProgress" :key="prog.label">
                <div class="flex justify-between mb-0.5">
                  <span class="text-[8px] text-white/40">{{ prog.label }}</span>
                  <span class="text-[8px] text-white/60 font-bold">%{{ prog.val }}</span>
                </div>
                <div class="h-1 bg-white/8 rounded-full overflow-hidden">
                  <div class="h-full rounded-full" :style="{ width: prog.val + '%', background: prog.color }"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Scroll indicator -->
      <div class="absolute bottom-8 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 scroll-bounce">
        <p class="text-slate-500 text-[10px] tracking-[0.2em] uppercase font-medium">Kaydırın</p>
        <div class="w-5 h-8 border border-white/20 rounded-full flex items-start justify-center p-1">
          <div class="w-1 h-2 bg-blue-400 rounded-full scroll-dot"></div>
        </div>
      </div>
    </section>

    <!-- ═══════════════ AUTH SECTION ═══════════════ -->
    <section class="min-h-screen flex items-center justify-center bg-slate-50 py-16 relative overflow-hidden">
      <div class="absolute top-0 right-0 w-96 h-96 bg-blue-100/50 rounded-full -translate-y-1/2 translate-x-1/2 pointer-events-none"></div>
      <div class="absolute bottom-0 left-0 w-72 h-72 bg-indigo-100/50 rounded-full translate-y-1/2 -translate-x-1/2 pointer-events-none"></div>
      <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-150 h-150 bg-blue-50/40 rounded-full blur-3xl pointer-events-none"></div>

      <div
        ref="loginCardEl"
        class="w-full max-w-5xl px-6 grid grid-cols-1 lg:grid-cols-2 gap-16 items-center transition-all duration-700 ease-out"
        :class="loginVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-16'"
      >
        <!-- ─── Sol: Neden KIYP? ─── -->
        <div class="hidden lg:block">
          <p class="text-xs font-bold text-blue-600 uppercase tracking-widest mb-3">Neden KIYP?</p>
          <h2 class="text-3xl font-black text-slate-800 leading-tight mb-3">
            İşletmenizin<br />
            <span class="text-transparent bg-clip-text" style="background-image:linear-gradient(135deg,#2563eb,#7c3aed)">
              tüm gücü
            </span><br />
            tek platformda.
          </h2>
          <p class="text-slate-400 text-sm leading-relaxed mb-8">
            Küçük ve orta ölçekli işletmeler için tasarlanmış, sezgisel ve hızlı bir yönetim deneyimi.
          </p>

          <div class="space-y-3">
            <div
              v-for="feat in authFeatures" :key="feat.title"
              class="auth-feature flex items-start gap-4 p-3.5 rounded-2xl border border-transparent hover:border-slate-200 hover:bg-white hover:shadow-md cursor-default group transition-all duration-250"
            >
              <div
                class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0 transition-all duration-250 group-hover:scale-110 group-hover:shadow-lg"
                :style="{ background: feat.bg, boxShadow: 'none' }"
                :class="'group-hover:shadow-[0_4px_12px_' + feat.glow + ']'"
              >
                <component :is="feat.icon" :size="18" class="text-white" />
              </div>
              <div>
                <p class="text-sm font-bold text-slate-700 group-hover:text-slate-900 transition-colors duration-200">{{ feat.title }}</p>
                <p class="text-xs text-slate-400 mt-0.5 leading-relaxed group-hover:text-slate-500 transition-colors duration-200">{{ feat.desc }}</p>
              </div>
            </div>
          </div>

          <!-- Mini istatistik rozetleri -->
          <div class="flex gap-3 mt-8">
            <div v-for="badge in authBadges" :key="badge.label"
              class="auth-badge flex-1 text-center p-3 rounded-2xl border border-slate-200 bg-white hover:border-blue-200 hover:shadow-md cursor-default transition-all duration-200">
              <p class="text-lg font-black badge-value" :style="{ backgroundImage: badge.gradient }">
                {{ badge.value }}
              </p>
              <p class="text-[10px] text-slate-400 mt-0.5 font-medium">{{ badge.label }}</p>
            </div>
          </div>
        </div>

        <!-- ─── Sağ: Form ─── -->
        <div>
          <div class="flex items-center gap-3 mb-6">
            <div class="w-10 h-10 bg-blue-600 rounded-xl flex items-center justify-center shadow shadow-blue-200 shrink-0">
              <span class="text-white font-black text-xl leading-none">K</span>
            </div>
            <div>
              <p class="text-slate-800 font-black text-xl leading-none">KIYP</p>
              <p class="text-slate-400 text-[11px] mt-0.5">Küçük İşletme Yönetim Portalı</p>
            </div>
          </div>

          <div class="bg-white rounded-3xl shadow-xl shadow-slate-200/80 border border-slate-100 p-8 overflow-hidden">
            <Transition :name="transitionDir" mode="out-in">

              <!-- GİRİŞ FORMU -->
              <div v-if="mode === 'login'" key="login">
                <h2 class="text-2xl font-black text-slate-800 mb-1">Hoş geldiniz</h2>
                <p class="text-slate-400 text-sm mb-6">Hesabınıza giriş yapın.</p>
                <form @submit.prevent="handleLogin" class="space-y-4">
                  <div>
                    <label class="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1.5">E-posta</label>
                    <input v-model="loginEmail" type="email" required autocomplete="email" placeholder="esnaf@ornek.com"
                      class="w-full px-4 py-2.5 border border-slate-200 rounded-2xl text-sm text-slate-700 placeholder:text-slate-300 bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400 transition-all" />
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1.5">Şifre</label>
                    <input v-model="loginPassword" type="password" required autocomplete="current-password" placeholder="••••••••"
                      class="w-full px-4 py-2.5 border border-slate-200 rounded-2xl text-sm text-slate-700 placeholder:text-slate-300 bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400 transition-all" />
                  </div>
                  <button type="submit"
                    class="form-btn w-full py-2.5 rounded-2xl text-sm font-bold text-white bg-blue-600 hover:bg-blue-700 transition-all duration-200 shadow-md shadow-blue-200 mt-1">
                    Giriş Yap
                  </button>
                </form>
                <div class="mt-5 pt-5 border-t border-slate-100 text-center text-sm text-slate-400">
                  Hesabınız yok mu?
                  <button @click="switchMode('register')" class="switch-link text-blue-600 font-bold ml-1">Kayıt Olun</button>
                </div>
              </div>

              <!-- KAYIT FORMU -->
              <div v-else key="register">
                <button @click="switchMode('login')"
                  class="flex items-center gap-1.5 text-xs font-semibold text-slate-400 hover:text-blue-600 mb-5 transition-colors group">
                  <span class="group-hover:-translate-x-1 transition-transform duration-200">←</span>
                  Geri dön
                </button>
                <h2 class="text-2xl font-black text-slate-800 mb-1">Hesap Oluştur</h2>
                <p class="text-slate-400 text-sm mb-6">İşletmeniz için hemen kaydolun.</p>
                <form @submit.prevent="handleRegister" class="space-y-4">
                  <div>
                    <label class="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1.5">İşletme Adı</label>
                    <input v-model="regBusinessName" type="text" required placeholder="Örn: Akıllı Market"
                      class="w-full px-4 py-2.5 border border-slate-200 rounded-2xl text-sm text-slate-700 placeholder:text-slate-300 bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400 transition-all" />
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1.5">E-posta</label>
                    <input v-model="regEmail" type="email" required autocomplete="email" placeholder="esnaf@ornek.com"
                      class="w-full px-4 py-2.5 border border-slate-200 rounded-2xl text-sm text-slate-700 placeholder:text-slate-300 bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400 transition-all" />
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1.5">İşletme Türü</label>
                    <div class="relative">
                      <button type="button" @click="isTypeOpen = !isTypeOpen"
                        class="w-full flex items-center justify-between px-4 py-2.5 border border-slate-200 rounded-2xl text-sm bg-slate-50 hover:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400 transition-all"
                        :class="regBusinessType ? 'text-slate-700' : 'text-slate-300'">
                        <span>{{ regBusinessType ? businessTypeLabel : 'Tür seçin' }}</span>
                        <svg class="w-4 h-4 text-slate-400 transition-transform duration-200" :class="{ 'rotate-180': isTypeOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                        </svg>
                      </button>
                      <div v-if="isTypeOpen" class="fixed inset-0 z-40" @click="isTypeOpen = false"></div>
                      <div v-if="isTypeOpen" class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-2xl shadow-xl z-50 p-1.5">
                        <button v-for="opt in businessTypeOptions" :key="opt.value" type="button"
                          @click="regBusinessType = opt.value; isTypeOpen = false"
                          class="w-full text-left px-3 py-2.5 rounded-xl text-sm transition-colors"
                          :class="regBusinessType === opt.value ? 'bg-blue-50 text-blue-600 font-semibold' : 'text-slate-700 hover:bg-slate-50'">
                          <p class="font-semibold">{{ opt.label }}</p>
                          <p class="text-[11px] mt-0.5" :class="regBusinessType === opt.value ? 'text-blue-400' : 'text-slate-400'">{{ opt.desc }}</p>
                        </button>
                      </div>
                    </div>
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1.5">Şifre</label>
                    <input v-model="regPassword" type="password" required autocomplete="new-password" placeholder="••••••••"
                      class="w-full px-4 py-2.5 border border-slate-200 rounded-2xl text-sm text-slate-700 placeholder:text-slate-300 bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400 transition-all" />
                  </div>
                  <button type="submit" :disabled="isRegLoading"
                    class="form-btn w-full py-2.5 rounded-2xl text-sm font-bold text-white bg-blue-600 hover:bg-blue-700 transition-all duration-200 shadow-md shadow-blue-200 mt-1 disabled:opacity-60 disabled:cursor-not-allowed">
                    {{ isRegLoading ? 'Oluşturuluyor...' : 'Hesap Oluştur' }}
                  </button>
                </form>
                <div class="mt-5 pt-5 border-t border-slate-100 text-center text-sm text-slate-400">
                  Zaten hesabınız var mı?
                  <button @click="switchMode('login')" class="switch-link text-blue-600 font-bold ml-1">Giriş Yapın</button>
                </div>
              </div>

            </Transition>
          </div>

          <div class="flex items-center justify-center gap-2 mt-5">
            <div class="w-1.5 h-1.5 rounded-full bg-emerald-400"></div>
            <p class="text-xs text-slate-400">JWT ile şifreli ve güvenli bağlantı</p>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  PackageIcon, TrendingUpIcon, UsersIcon, LayersIcon, ReceiptIcon,
} from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const router = useRouter()
const { showError, showSuccess } = useAlert()

const pageEl = ref<HTMLElement | null>(null)
const loginCardEl = ref<HTMLElement | null>(null)
const loginVisible = ref(false)
const isLeaving = ref(false)
const mode = ref<'login' | 'register'>('login')
const transitionDir = ref('slide-left')

const loginEmail = ref('')
const loginPassword = ref('')
const regBusinessName = ref('')
const regEmail = ref('')
const regBusinessType = ref('')
const regPassword = ref('')
const isRegLoading = ref(false)
const isTypeOpen = ref(false)

const businessTypeOptions = [
  { value: 'commerce', label: 'Satış / Ticaret', desc: 'Al-Sat, perakende, toptan' },
  { value: 'production', label: 'Üretim ve Satış', desc: 'İmalat, reçete odaklı üretim' },
]
const businessTypeLabel = computed(
  () => businessTypeOptions.find((o) => o.value === regBusinessType.value)?.label ?? '',
)

// ─── Statik mock / tanıtım verisi ───
const featureChips = ['Stok Takibi', 'Satış Yönetimi', 'Müşteri Portföyü', 'Üretim', 'Gider Analizi', 'Personel & Maaş', 'Analitik']
const heroStats = [
  { value: '7 Modül', label: 'Kapsamlı Yönetim' },
  { value: 'Gerçek Zamanlı', label: 'Veri Analizi' },
  { value: '2 Mod', label: 'Satış & Üretim' },
]
const mockBars = [
  { h: 55, isExpense: false }, { h: 30, isExpense: true }, { h: 70, isExpense: false },
  { h: 35, isExpense: true }, { h: 85, isExpense: false }, { h: 40, isExpense: true },
  { h: 60, isExpense: false }, { h: 28, isExpense: true }, { h: 75, isExpense: false },
  { h: 45, isExpense: true }, { h: 90, isExpense: false }, { h: 38, isExpense: true },
]
const mockTableRows = [
  { name: 'Ürün A', amount: '₺42.8K', color: '#2563eb' },
  { name: 'Ürün B', amount: '₺31.5K', color: '#7c3aed' },
  { name: 'Ürün C', amount: '₺18.2K', color: '#059669' },
]
const mockProgress = [
  { label: 'Stok Doluluk', val: 72, color: 'linear-gradient(90deg,#2563eb,#60a5fa)' },
  { label: 'Aylık Hedef', val: 88, color: 'linear-gradient(90deg,#059669,#34d399)' },
  { label: 'Memnuniyet', val: 94, color: 'linear-gradient(90deg,#7c3aed,#a78bfa)' },
]

const authFeatures = [
  {
    icon: PackageIcon, title: 'Lot Bazlı Stok Takibi',
    desc: 'Her ürünü lot numarasıyla takip edin, kritik stok uyarılarını anında görün.',
    bg: 'linear-gradient(135deg,#2563eb,#1d4ed8)', glow: 'rgba(37,99,235,0.3)',
  },
  {
    icon: TrendingUpIcon, title: 'Kârlılık & Başabaş Analizi',
    desc: 'Gerçek zamanlı kâr marjı, başabaş noktası ve gelir-gider karşılaştırması.',
    bg: 'linear-gradient(135deg,#7c3aed,#6d28d9)', glow: 'rgba(124,58,237,0.3)',
  },
  {
    icon: UsersIcon, title: 'Müşteri & Satış Yönetimi',
    desc: 'Müşteri portföyünüzü, alım geçmişini ve satış trendlerini tek ekranda görün.',
    bg: 'linear-gradient(135deg,#059669,#047857)', glow: 'rgba(5,150,105,0.3)',
  },
  {
    icon: LayersIcon, title: 'Reçete Odaklı Üretim',
    desc: 'Hammadde tüketimini otomatik hesaplayın, üretim maliyetini anlık takip edin.',
    bg: 'linear-gradient(135deg,#d97706,#b45309)', glow: 'rgba(217,119,6,0.3)',
  },
  {
    icon: ReceiptIcon, title: 'Gider & Personel Yönetimi',
    desc: 'Sabit/dönemsel giderler, maaş hesaplama ve SGK kesintilerini otomatik yapın.',
    bg: 'linear-gradient(135deg,#dc2626,#b91c1c)', glow: 'rgba(220,38,38,0.3)',
  },
]

const authBadges = [
  { value: '7+', label: 'Modül', gradient: 'linear-gradient(135deg,#2563eb,#7c3aed)' },
  { value: '2', label: 'İş Modu', gradient: 'linear-gradient(135deg,#059669,#2563eb)' },
  { value: '∞', label: 'Veri', gradient: 'linear-gradient(135deg,#7c3aed,#ec4899)' },
]

// ─── Form geçişi ───
const switchMode = (target: 'login' | 'register') => {
  transitionDir.value = target === 'register' ? 'slide-left' : 'slide-right'
  mode.value = target
}

// ─── Scroll reveal ───
let observer: IntersectionObserver | null = null
onMounted(() => {
  if (!loginCardEl.value) return
  observer = new IntersectionObserver(
    (entries) => {
      if (entries[0]?.isIntersecting) { loginVisible.value = true; observer?.disconnect() }
    },
    { threshold: 0.15 },
  )
  observer.observe(loginCardEl.value)
})
onUnmounted(() => observer?.disconnect())

// ─── Login ───
const API_URL = '/api/auth'
const handleLogin = async () => {
  try {
    const res = await fetch(`${API_URL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: loginEmail.value, password: loginPassword.value }),
    })
    const data = await res.json()
    if (!res.ok) throw new Error(data.message || 'E-posta veya şifre hatalı.')
    localStorage.setItem('token', data.token)
    localStorage.setItem('user', JSON.stringify({ fullName: data.fullName, businessType: data.businessType, userId: data.userId }))
    isLeaving.value = true
    setTimeout(() => router.push('/dashboard'), 380)
  } catch (e: any) { showError(`Hata: ${e.message}`) }
}

// ─── Register ───
const handleRegister = async () => {
  if (isRegLoading.value) return
  if (!regBusinessType.value) { showError('Lütfen işletme türünü seçin.'); return }
  isRegLoading.value = true
  try {
    const res = await fetch(`${API_URL}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        fullName: regBusinessName.value,
        email: regEmail.value,
        password: regPassword.value,
        businessType: regBusinessType.value === 'commerce' ? 0 : 1,
      }),
    })
    const data = await res.json()
    if (!res.ok) throw new Error(data.message || 'Kayıt sırasında bir hata oluştu.')
    showSuccess('Hesap başarıyla oluşturuldu! Giriş yapabilirsiniz.')
    regBusinessName.value = ''; regEmail.value = ''; regBusinessType.value = ''; regPassword.value = ''
    switchMode('login')
  } catch (e: any) { showError(`Hata: ${e.message}`) }
  finally { isRegLoading.value = false }
}
</script>

<style scoped>
.login-page { transform-origin: top center; }

@keyframes pageExit {
  0%   { transform: translateY(0) scale(1); opacity: 1; }
  100% { transform: translateY(60px) scale(0.97); opacity: 0; }
}
.page-exit { animation: pageExit 0.38s cubic-bezier(0.4,0,1,1) both; pointer-events: none; }

@keyframes fadeUp {
  from { opacity: 0; transform: translateY(28px); }
  to   { opacity: 1; transform: translateY(0); }
}
.anim-up { animation: fadeUp 0.7s cubic-bezier(0.22,1,0.36,1) both; animation-delay: var(--d, 0s); }

/* ── Gradient başlık ── */
.gradient-text {
  background: linear-gradient(135deg, #60a5fa 0%, #a78bfa 100%);
  background-size: 200% 100%;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}
.gradient-text-animate { transition: background-position 0.4s ease; }
.gradient-text-animate:hover { background-position: right center; }

/* ── Feature chips ── */
.feature-chip {
  transition: all 0.22s cubic-bezier(0.22, 1, 0.36, 1);
}
.feature-chip:hover {
  background: rgba(255,255,255,0.16);
  border-color: rgba(96,165,250,0.5);
  color: rgba(255,255,255,0.95);
  transform: scale(1.08) translateY(-2px);
  box-shadow: 0 0 18px rgba(96,165,250,0.22), 0 4px 12px rgba(0,0,0,0.2);
}

/* ── Hero stat — gradient on hover via group ── */
.hero-stat { transition: transform 0.2s; }
.hero-stat:hover { transform: translateY(-2px); }

/* ── Mock bar hover ── */
.mock-bar { transition: opacity 0.15s; }
.mock-bar:hover { opacity: 0.75; }

/* ── Mock stat card hover ── */
.mock-stat-card { transition: transform 0.2s, box-shadow 0.2s; cursor: default; }
.mock-stat-card:hover { transform: scale(1.04); box-shadow: 0 4px 16px rgba(0,0,0,0.2); }

/* ── Mock table row hover ── */
.mock-table-row { transition: background 0.15s; cursor: default; }
.mock-table-row:hover { background: rgba(255,255,255,0.06) !important; }

/* ── Auth feature item ── */
.auth-feature { transition: transform 0.25s cubic-bezier(0.22,1,0.36,1), background 0.2s, border-color 0.2s, box-shadow 0.2s; }
.auth-feature:hover { transform: translateX(6px); }

/* ── Auth badge ── */
.auth-badge { transition: all 0.2s; }
.auth-badge:hover { transform: translateY(-2px); }

.badge-value {
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

/* ── Form button lift ── */
.form-btn { transition: all 0.2s; }
.form-btn:not(:disabled):hover { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(37,99,235,0.35); }
.form-btn:not(:disabled):active { transform: translateY(0) scale(0.98); }

/* ── Switch link underline animation ── */
.switch-link {
  position: relative;
  transition: color 0.15s;
  text-decoration: none;
}
.switch-link::after {
  content: '';
  position: absolute;
  bottom: -1px; left: 0; right: 0;
  height: 1.5px;
  background: currentColor;
  transform: scaleX(0);
  transform-origin: left;
  transition: transform 0.25s cubic-bezier(0.22,1,0.36,1);
}
.switch-link:hover::after { transform: scaleX(1); }

/* ── Grid desen ── */
.hero-grid {
  background-image:
    linear-gradient(rgba(255,255,255,0.04) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255,255,255,0.04) 1px, transparent 1px);
  background-size: 48px 48px;
}

/* ── Mock kart ── */
.mock-card {
  background: rgba(255,255,255,0.05);
  backdrop-filter: blur(24px);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 24px;
  padding: 20px;
  box-shadow: 0 0 0 1px rgba(255,255,255,0.05), 0 32px 64px rgba(0,0,0,0.4), 0 0 80px rgba(37,99,235,0.12);
}
@keyframes float {
  0%,100% { transform: translateY(0); }
  50%      { transform: translateY(-10px); }
}
.animate-float { animation: float 5s ease-in-out infinite; }

/* ── Scroll indicator ── */
@keyframes scrollDot {
  0%   { transform: translateY(0); opacity: 1; }
  100% { transform: translateY(14px); opacity: 0; }
}
.scroll-dot { animation: scrollDot 1.6s ease-in-out infinite; }
@keyframes fadeInDown {
  from { opacity: 0; transform: translateY(-8px); }
  to   { opacity: 1; transform: translateY(0); }
}
.scroll-bounce { animation: fadeInDown 1s ease-out 1.2s both; }

/* ── Form switch geçişleri ── */
.slide-left-enter-active, .slide-left-leave-active,
.slide-right-enter-active, .slide-right-leave-active {
  transition: all 0.32s cubic-bezier(0.22,1,0.36,1);
}
.slide-left-enter-from  { opacity: 0; transform: translateX(32px); }
.slide-left-leave-to    { opacity: 0; transform: translateX(-32px); }
.slide-right-enter-from { opacity: 0; transform: translateX(-32px); }
.slide-right-leave-to   { opacity: 0; transform: translateX(32px); }
</style>
