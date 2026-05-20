import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/login',
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/auth/Login.vue'),
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/auth/Register.vue'),
    },
    {
      path: '/dashboard',
      component: () => import('../layouts/AppLayout.vue'),
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('../views/dashboard/Dashboard.vue'),
        },
        {
          path: '/stocks',
          name: 'stocks',
          component: () => import('../views/stocks/Stocks.vue'),
        },
        {
          path: '/customers',
          name: 'customers',
          component: () => import('../views/Customers/Customers.vue'),
        },
        {
          path: '/sales',
          name: 'sales',
          component: () => import('../views/Sales/Sales.vue'),
        },
        {
          path: '/production',
          name: 'production',
          component: () => import('../views/Production/Production.vue'),
        },
        {
          path: '/expenses',
          name: 'expenses',
          component: () => import('../views/expenses/Expenses.vue'),
        },
        {
          path: '/employees',
          name: 'employees',
          component: () => import('../views/Employees/Employees.vue'),
        },
        {
          path: '/analytics',
          name: 'analytics',
          component: () => import('../views/Analytics/Analytics.vue'),
        },
      ],
    },
  ],
})

export default router
