<script setup lang="ts">
import { ref, onMounted } from 'vue';
import * as api from '@/services/api';
import type { AxiosError } from 'axios';

const user = ref<api.UserProfile | null>(null);
const oldPassword = ref('');
const newPassword = ref('');
const confirmNewPassword = ref('');

const message = ref('');
const errorMessage = ref('');
const isLoading = ref(true);

const loadProfile = async () => {
  try {
    const response = await api.getUserProfile();
    user.value = response.data;
  } catch (err) {
    errorMessage.value = "Error al cargar el perfil.";
  } finally {
    isLoading.value = false;
  }
};

onMounted(loadProfile);

const handleUpdatePassword = async () => {
  message.value = '';
  errorMessage.value = '';

  if (newPassword.value !== confirmNewPassword.value) {
    errorMessage.value = "Las nuevas contraseñas no coinciden.";
    return;
  }
  if (!oldPassword.value || !newPassword.value) {
      errorMessage.value = "Por favor, rellena todos los campos.";
      return;
  }

  try {
    await api.updatePassword({ oldPassword: oldPassword.value, newPassword: newPassword.value });
    message.value = "Contraseña actualizada exitosamente.";
    // Limpiar campos
    oldPassword.value = '';
    newPassword.value = '';
    confirmNewPassword.value = '';
  } catch (err) {
    const error = err as AxiosError<{ message: string }>;
    errorMessage.value = error.response?.data?.message || "Error al actualizar la contraseña.";
  }
};

</script>

<template>
  <div class="container">
    <h1>Mi Perfil</h1>
    <div v-if="isLoading" class="text-center">Cargando...</div>
    <div v-else-if="user" class="profile-content">
      <div class="card">
        <h2>Información de Usuario</h2>
        <p><strong>Nombre:</strong> {{ user.nombre }}</p>
        <p><strong>Email:</strong> {{ user.email }}</p>
      </div>

      <div class="card">
        <h2>Cambiar Contraseña</h2>
        <form @submit.prevent="handleUpdatePassword">
          <label for="oldPassword">Contraseña Actual:</label>
          <input type="password" id="oldPassword" v-model="oldPassword" required />

          <label for="newPassword">Nueva Contraseña:</label>
          <input type="password" id="newPassword" v-model="newPassword" required />

          <label for="confirmNewPassword">Confirmar Nueva Contraseña:</label>
          <input type="password" id="confirmNewPassword" v-model="confirmNewPassword" required />

          <button type="submit" class="btn btn-primary">Actualizar Contraseña</button>

          <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>
          <div v-if="message" class="success-message">{{ message }}</div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.profile-content {
  display: grid;
  gap: 2rem;
}

.success-message {
  color: var(--color-primary);
  margin-top: 1rem;
}

label {
    display: block;
    margin-top: 1rem;
    margin-bottom: 0.5rem;
    font-weight: 600;
}
</style>
