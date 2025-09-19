<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { registerUser } from '@/services/api'
import type { AxiosError } from 'axios';

const nombre = ref('')
const email = ref('')
const password = ref('')
const errorMessage = ref('')
const router = useRouter()

const handleRegister = async () => {
  errorMessage.value = ''
  try {
    await registerUser({
      nombre: nombre.value,
      email: email.value,
      password: password.value
    })
    // Opcional: en lugar de un alert, podríamos tener un componente de notificación global
    alert('¡Registro exitoso! Ahora puedes iniciar sesión.')
    await router.push('/login')
  } catch (err) {
    const error = err as AxiosError<{ message: string }>;
    errorMessage.value = error.response?.data?.message || 'Ocurrió un error durante el registro.';
  }
}
</script>

<template>
  <div class="auth-container">
    <div class="container">
      <h1 class="text-center">Crear Cuenta</h1>
      <form @submit.prevent="handleRegister">
        
        <label for="nombre">Nombre:</label>
        <input type="text" id="nombre" v-model="nombre" required placeholder="Tu Nombre" />

        <label for="email">Email:</label>
        <input type="email" id="email" v-model="email" required placeholder="tu@email.com" />
        
        <label for="password">Contraseña:</label>
        <input type="password" id="password" v-model="password" required placeholder="••••••••" />
        
        <button type="submit" class="btn btn-primary btn-block">Registrarse</button>

        <div v-if="errorMessage" class="error-message text-center">{{ errorMessage }}</div>
      </form>
      <p class="text-center alternate-link">
        ¿Ya tienes una cuenta? <router-link to="/login">Inicia sesión</router-link>
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
