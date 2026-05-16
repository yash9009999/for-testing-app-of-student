import { createRouter, createWebHistory } from 'vue-router'
import AboutView from '../views/AboutView.vue'
import ScoopsBuilderView from '../views/ScoopsBuilderView.vue'
import CartView from '../views/CartView.vue'
import OrderTrackingView from '../views/OrderTrackingView.vue'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import NotFoundView from '../views/NotFoundView.vue'
import UserView from '../views/UserView.vue'
import AuthCallbackView from '../views/AuthCallbackView.vue'
import { isAuthenticated } from '../services/authService'

const routes = [
  {
    path: '/',
    name: 'home',
    component: AboutView
  },
  {
    path: '/scoops-builder',
    name: 'scoops-builder',
    component: ScoopsBuilderView
  },
  {
    path: '/cart',
    name: 'cart',
    component: CartView,
    props: route => {
      let cart = JSON.parse(route.query.cart || '[]');
      let orderId = null;
      try { orderId = localStorage.getItem('s2g_orderId'); } catch(e) {}
      return { cart, orderId };
    }
  },
  {
  path: '/order-tracking',
  name: 'order-tracking',
  component: OrderTrackingView,
    props: route => ({ initialOrderNumber: route.query.orderNumber, initialEstimatedDeliveryTime: route.query.estimatedDeliveryTime })
  },
  {
    path: '/login',
    name: 'login',
    component: LoginView
  },
  {
    path: '/register',
    name: 'register',
    component: RegisterView
  },
  {
    path: '/auth/callback',
    name: 'auth-callback',
    component: AuthCallbackView
  },
  {
    path: '/user',
    name: 'user',
    component: UserView
  },
  {
    path: '/:catchAll(.*)',
    name: 'NotFound',
    component: NotFoundView
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

router.beforeEach((to) => {
  if (to.name === 'user' && !isAuthenticated()) {
    return { name: 'login', query: { redirect: to.fullPath } };
  }
})

export default router
