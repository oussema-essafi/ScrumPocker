<template>
  <div class="container mt-5">
    <h1>Session: {{ store.sessionId }}</h1>
    <div v-if="store.errorMessage" class="alert alert-danger">{{ store.errorMessage }}</div>

    <!-- Name input form for guests accessing the session URL -->
    <div v-if="!store.currentUserName">
      <div class="card">
        <div class="card-body">
          <h3 class="card-title">Join Session</h3>
          <input v-model="joinUserName" class="form-control mb-2" placeholder="Your Name" />
          <button class="btn btn-primary w-100" @click="handleJoinSession" :disabled="!joinUserName">Join</button>
        </div>
      </div>
    </div>

    <!-- Session content for users who have joined -->
    <div v-else>
      <div class="row">
        <div class="col-md-4">
          <div class="card">
            <div class="card-body">
              <h3 class="card-title">Participants</h3>
              <ul class="list-group">
                <li v-for="participant in store.participants" :key="participant.userName" class="list-group-item">
                  {{ participant.userName }}
                  <!-- Display "Host" badge if the participant is the host -->
                  <span v-if="participant.isHost" class="badge bg-primary ms-2">Host</span>
                  <span v-if="store.votedUsers.has(participant.userName)" class="badge bg-success ms-2">Voted</span>
                </li>
              </ul>
            </div>
          </div>
        </div>
        <div class="col-md-8">
          <div class="card">
            <div class="card-body">
              <h3 class="card-title">Vote</h3>
              <div v-if="!store.isRevealed">
                <div class="d-flex flex-wrap gap-2">
                  <button v-for="vote in ['0', '1', '2', '3', '5', '8', '13', '21', '34', '?']" :key="vote"
                          class="btn btn-outline-primary"
                          :disabled="store.currentUserVote != null"
                          @click="submitVote(vote)">
                    {{ vote }}
                  </button>
                </div>
                <p v-if="store.currentUserVote" class="mt-2">Your vote: {{ store.currentUserVote }}</p>
              </div>
              <div v-else>
                <p>Your vote: {{ store.currentUserVote }}</p>
                <h4>Votes</h4>
                <ul class="list-group">
                  <li v-for="(vote, user) in store.votes" :key="user" class="list-group-item">{{ user }}: {{ vote }}</li>
                </ul>
              </div>
              <!-- Host-only controls -->
              <div v-if="store.isHost" class="mt-3">
                <button class="btn btn-warning me-2" @click="resetVotes">Reset Votes</button>
                <button class="btn btn-success" @click="revealVotes" :disabled="store.isRevealed">Reveal Votes</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { store } from '../store'
import { joinSession, submitVote, resetVotes, revealVotes, startConnection } from '../signalr'
import { useRoute } from 'vue-router'

const route = useRoute()
const joinUserName = ref('')
const validUserName = computed(() => joinUserName.value.trim().length > 0 && joinUserName.value.length <= 50)
// Set the session ID from the URL when the component mounts
onMounted(
async () => {
    if(!store.currentUserName)
        await startConnection()

  if (route.params.sessionId) {
    store.sessionId = route.params.sessionId
  }
})

// Handle joining the session when a guest submits their name
async function handleJoinSession() {
  if (!validUserName.value) {
    store.errorMessage = 'Name must be 1-50 characters'
    return
  }
  store.errorMessage = null
  await joinSession(store.sessionId, joinUserName.value)
  if (!store.errorMessage) {
    store.currentUserName = joinUserName.value
  }
}
</script>