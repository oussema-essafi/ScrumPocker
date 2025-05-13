import * as signalR from '@microsoft/signalr'
import { store } from './store'
import router from './router'

let connection = null

export async function startConnection() {  
    if (store._initialized) return;
  connection = new signalR.HubConnectionBuilder()
    .withUrl(import.meta.env.VITE_HUB_URL, { withCredentials: true })
    .configureLogging(signalR.LogLevel.Information)
    .build()

  connection.on('Error', message => {
    store.errorMessage = message
  })

  connection.on('SessionCreated', sessionId => {
    store.sessionId = sessionId
    store.isHost = true
    store.participants.push({ userName: store.currentUserName, isHost: true })
    router.push(`/session/${sessionId}`)
  })

  connection.on('JoinedSession', participants => {
    store.participants = participants
    router.push(`/session/${store.sessionId}`)
  })

  connection.on('UserJoined', userName => {
    if (!store.participants.some(p => p.userName === userName)) {
      store.participants.push({ userName })
    }
  })

  connection.on('VoteReceived', (userName, vote) => {
    store.votedUsers.add(userName)
  })

  connection.on('VotesRevealed', votes => {
    store.votes = votes
    store.isRevealed = true
  })

  connection.on('VotesReset', () => {
    store.votes = {}
    store.votedUsers.clear()
    store.isRevealed = false
    store.currentUserVote = null
  })

  connection.on('UserLeft', userName => {
    store.participants = store.participants.filter(p => p.userName !== userName)
    store.votedUsers.delete(userName)
    delete store.votes[userName]
  })

  try {
    await connection.start()
    console.log('SignalR connection established')
    store._initialized = true;
  } catch (err) {
    console.error('SignalR connection failed:', err)
    store.errorMessage = 'Failed to connect to server'
  }
}

export async function createSession(userName) {
  store.currentUserName = userName
  await connection.invoke('CreateSession', userName)
}

export async function joinSession(sessionId, userName) {
  store.sessionId = sessionId
  store.currentUserName = userName
  await connection.invoke('JoinSession', sessionId, userName)
}

export async function submitVote(vote) {
  await connection.invoke('SubmitVote', store.sessionId, vote)
  store.currentUserVote = vote
}

export async function resetVotes() {
  if (store.isHost) {
    await connection.invoke('ResetVotes', store.sessionId)
  }
}

export async function revealVotes() {
  if (store.isHost) {
    await connection.invoke('RevealVotes', store.sessionId)
  }
}