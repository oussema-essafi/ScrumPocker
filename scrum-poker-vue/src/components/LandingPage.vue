<template>
  <div class="container mt-5">
    <h1 class="text-center mb-4">Scrum Poker</h1>
    <div v-if="store.errorMessage" class="alert alert-danger">{{ store.errorMessage }}</div>
    <div class="row">
      <div class="col-md-6">
        <div class="card">
          <div class="card-body">
            <h3 class="card-title">Create Session</h3>
            <input v-model="createUserName" class="form-control mb-2" placeholder="Your Name" />
            <button class="btn btn-primary w-100" @click="handleCreateSession" :disabled="!createUserName">Create</button>
          </div>
        </div>
      </div>
      <div class="col-md-6">
        <div class="card">
          <div class="card-body">
            <h3 class="card-title">Join Session</h3>
            <input v-model="joinSessionId" class="form-control mb-2" placeholder="Session ID" />
            <input v-model="joinUserName" class="form-control mb-2" placeholder="Your Name" />
            <button class="btn btn-primary w-100" @click="handleJoinSession" :disabled="!joinSessionId || !joinUserName">Join</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { store } from '../store'
import { startConnection, createSession, joinSession } from '../signalr'
import { useRouter } from 'vue-router'

const router = useRouter()
const createUserName = ref('')
const joinSessionId = ref('')
const joinUserName = ref('')
const validJoinedUserName = computed(() => joinUserName.value.trim().length > 0 && joinUserName.value.length <= 50)
const validCreatedUserName = computed(() => createUserName.value.trim().length > 0 && createUserName.value.length <= 50)
onMounted(async () => {
  await startConnection()
})

async function handleCreateSession() {
      if (!validCreatedUserName.value) {
    store.errorMessage = 'Name must be 1-50 characters'
    return
  }
  store.errorMessage = null
  await createSession(createUserName.value.trim())
  if (!store.errorMessage) {
    router.push(`/session/${store.sessionId}`)
  }
}

async function handleJoinSession() {
    if (!validJoinedUserName.value) {
    store.errorMessage = 'Name must be 1-50 characters'
    return
  }
  store.errorMessage = null
  await joinSession(joinSessionId.value, joinUserName.value.trim())
  if (!store.errorMessage) {
    router.push(`/session/${store.sessionId}`)
  }
}
</script>