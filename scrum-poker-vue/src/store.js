import { reactive } from 'vue'

export const store = reactive({
  sessionId: null,
  isHost: false,
  _initialized: false,
  isConnected : false,
  currentUserName: null,
  participants: [],
  votes: {},
  votedUsers: new Set(),
  isRevealed: false,
  currentUserVote: null,
  errorMessage: null,
})