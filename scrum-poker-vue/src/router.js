import { createRouter, createWebHistory } from 'vue-router'
import LandingPage from './components/LandingPage.vue'
import SessionPage from './components/SessionPage.vue'

const routes = [
  { path: '/', component: LandingPage },
  { path: '/session/:sessionId', component: SessionPage },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router