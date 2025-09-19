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
    const response = await registerUser({
      nombre: nombre.value,
      email: email.value,
      password: password.value
    })

    console.log('Registro exitoso:', response.data)

    // Opcional: Mostrar un mensaje de éxito antes de redirigir
    alert('¡Registro exitoso! Ahora puedes iniciar sesión.')

    // Después de un registro exitoso, redirigimos al login
    router.push('/login')

  } catch (err) { // <-- Cambiamos 'error: any' por 'err'
    const error = err as AxiosError<{ message: string }>; // Le decimos a TS cómo es el error
    console.error('Error en el registro:', error.response?.data);
    errorMessage.value = error.response?.data?.message || 'Ocurrió un error durante el registro.';
  }
}
</script>

<template>
  <div>
    <h1>Registro de Nuevo Usuario</h1>
    <form @submit.prevent="handleRegister">
      <div>
        <label for="nombre">Nombre:</label>
        <input type="text" id="nombre" v-model="nombre" required />
      </div>
      <div>
        <label for="email">Email:</label>
        <input type="email" id="email" v-model="email" required />
      </div>
      <div>
        <label for="password">Contraseña:</label>
        <input type="password" id="password" v-model="password" required />
      </div>
      <button type="submit">Registrarse</button>

      <p v-if="errorMessage" style="color: red;">{{ errorMessage }}</p>
    </form>
    <p>
      ¿Ya tienes una cuenta? <router-link to="/login">Inicia sesión</router-link>
    </p>
  </div>
</template>

<style scoped>
form div {
  margin-bottom: 1rem;
}
</style>
