import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import DashboardView from '../views/DashboardView.vue'
import EventoDetalleView from '../views/EventoDetalleView.vue'
import ProfileView from '../views/ProfileView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/login' // Redirige la ruta raíz al login
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView
    },
    {
      path: '/dashboard',
      name: 'Dashboard', // Asegurarse que el nombre es consistente
      component: DashboardView,
      meta: { requiresAuth: true } // <-- 2. Añadimos una marca de ruta protegida
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView
    },
    {
      path: '/evento/:id',
      name: 'EventoDetalle', // Asegurarse que el nombre es consistente
      component: EventoDetalleView,
      meta: { requiresAuth: true }
    },
    {
      path: '/profile',
      name: 'Profile',
      component: ProfileView,
      meta: { requiresAuth: true }
    }
  ]
})
router.beforeEach((to, from, next) => {
  const isAuthenticated = !!localStorage.getItem('authToken');

  if (to.meta.requiresAuth && !isAuthenticated) {
    // Si la ruta requiere autenticación y el usuario no está logueado, lo mandamos al login
    next({ name: 'login' });
  } else {
    // Si no, dejamos que continúe
    next();
  }
});
export default router
