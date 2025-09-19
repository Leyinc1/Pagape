<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { loginUser } from '@/services/api'
import type { AxiosError } from 'axios';

const email = ref('')
const password = ref('')
const errorMessage = ref('')
const router = useRouter()

const handleLogin = async () => {
  errorMessage.value = ''
  try {
    const response = await loginUser({
      email: email.value,
      password: password.value
    })
    localStorage.setItem('authToken', response.data.token)
    // Redirige al dashboard principal
    await router.push({ name: 'Dashboard' })
  } catch (err) {
    const error = err as AxiosError<{ message: string }>;
    errorMessage.value = error.response?.data?.message || 'Ocurrió un error al iniciar sesión.';
  }
}
</script>

<template>
  <div class="auth-container">
    <div class="container">
      <h1 class="text-center">Iniciar Sesión</h1>
      <form @submit.prevent="handleLogin">
        
        <label for="email">Email:</label>
        <input type="email" id="email" v-model="email" required placeholder="tu@email.com" />
        
        <label for="password">Contraseña:</label>
        <input type="password" id="password" v-model="password" required placeholder="••••••••" />
        
        <button type="submit" class="btn btn-primary btn-block">Ingresar</button>

        <div v-if="errorMessage" class="error-message text-center">{{ errorMessage }}</div>
      </form>
      <p class="text-center alternate-link">
        ¿No tienes una cuenta? <router-link to="/register">Regístrate aquí</router-link>
      </p>
    </div>
  </div>
</template>

<style scoped>
.auth-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
}

.container {
  width: 100%;
  max-width: 400px;
}

.btn-block {
    display: block;
    width: 100%;
    margin-top: 1rem;
}

label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 600;
}

.alternate-link {
    margin-top: 2rem;
}
</style>
