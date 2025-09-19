<script setup lang="ts">
import { ref, watch } from 'vue';
import { RouterView, RouterLink, useRoute, useRouter } from 'vue-router';

const route = useRoute();
const router = useRouter();

// Un ref para controlar la visibilidad del nav
const isLoggedIn = ref(!!localStorage.getItem('authToken'));

// Observa los cambios de ruta para actualizar el estado de login
watch(
  () => route.path,
  () => {
    isLoggedIn.value = !!localStorage.getItem('authToken');
  }
);

const logout = () => {
  localStorage.removeItem('authToken');
  isLoggedIn.value = false;
  router.push('/login');
};
</script>

<template>
  <header v-if="isLoggedIn" class="main-header">
    <nav class="main-nav">
      <div class="nav-links">
        <RouterLink to="/dashboard" class="nav-logo">Pagape</RouterLink>
        <RouterLink to="/dashboard">Dashboard</RouterLink>
        <RouterLink to="/profile">Mi Perfil</RouterLink>
      </div>
      <div class="nav-actions">
        <button @click="logout" class="btn btn-secondary">Cerrar Sesión</button>
      </div>
    </nav>
  </header>

  <main>
    <RouterView />
  </main>
</template>

<style scoped>
.main-header {
  background-color: var(--color-background-mute);
  padding: 0 2rem;
  border-bottom: 1px solid var(--color-border);
  margin-bottom: 2rem;
}

.main-nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 70px;
  max-width: 1280px;
  margin: 0 auto;
}

.nav-links {
  display: flex;
  align-items: center;
  gap: 2rem;
}

.nav-logo {
  font-size: 1.5rem;
  font-weight: bold;
  color: var(--color-primary);
}

.nav-links a {
  font-weight: 600;
  font-size: 1rem;
  color: var(--color-text);
  text-decoration: none;
  transition: color 0.2s;
}

.nav-links a:hover, .nav-links a.router-link-exact-active {
  color: var(--color-primary);
}
</style>
