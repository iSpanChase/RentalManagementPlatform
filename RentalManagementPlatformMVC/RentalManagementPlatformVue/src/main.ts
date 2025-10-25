import { createApp } from 'vue'
import { createPinia } from 'pinia';
import App from './App.vue'
import router from './router'
import { config, library, dom } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faChevronUp, faChevronDown, faCreditCard, faLock} from '@fortawesome/free-solid-svg-icons'
import '@fortawesome/fontawesome-svg-core/styles.css'

// 讓 Font Awesome 不自動插入 <style>，避免與 Vite 衝突
config.autoAddCss = false
library.add(faChevronUp, faChevronDown, faCreditCard, faLock);
dom.watch()

const app = createApp(App)
const pinia = createPinia();

app.component('font-awesome-icon', FontAwesomeIcon)

app.use(pinia);
app.use(router)
app.use(createPinia());

app.mount('#app')
