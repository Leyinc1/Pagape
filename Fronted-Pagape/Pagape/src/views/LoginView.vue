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
  errorMessage.value = '' // Limpia errores previos
  try {
    const response = await loginUser({
      email: email.value,
      password: password.value
    })

    console.log('Login exitoso:', response.data)

    // Guarda el token en el almacenamiento local del navegador
    localStorage.setItem('authToken', response.data.token)

    // Redirige al usuario al dashboard (que crearemos pronto)
    router.push('/dashboard')

  } catch (err) { // <-- Cambiamos 'error: any' por 'err'
    const error = err as AxiosError<{ message: string }>; // Le decimos a TS cómo es el error
    console.error('Error en el login:', error.response?.data);
    errorMessage.value = error.response?.data?.message || 'Ocurrió un error al iniciar sesión.';
  }
}
</script>

<template>
  <div>
    <h1>Iniciar Sesión</h1>
    <form @submit.prevent="handleLogin">
      <div>
        <label for="email">Email:</label>
        <input type="email" id="email" v-model="email" required />
      </div>
      <div>
        <label for="password">Contraseña:</label>
        <input type="password" id="password" v-model="password" required />
      </div>
      <button type="submit">Ingresar</button>

      <p v-if="errorMessage" style="color: red;">{{ errorMessage }}</p>
    </form>
    <p>
      ¿No tienes una cuenta? <router-link to="/register">Regístrate aquí</router-link>
    </p>
  </div>
</template>

<style scoped>
/* Puedes añadir estilos aquí si quieres */
form div {
  margin-bottom: 1rem;
}
</style>
