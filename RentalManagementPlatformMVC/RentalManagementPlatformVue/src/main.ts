import { createApp } from 'vue'
import { createPinia } from 'pinia';
import App from './App.vue'
import router from './router'

// 先載 CSS
import 'bootstrap/dist/css/bootstrap.min.css'
// 再載 JS（很關鍵）
import 'bootstrap/dist/js/bootstrap.bundle.min.js'

import { config, library, dom } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { 
    faChevronUp, faChevronDown, faCreditCard, faLock, faCheck, faUser, 
    faLocationDot, faComment, faShieldHalved, faCircleInfo, faAlarmClock, 
    faHouse, faAddressBook, faStar, faCalendarDays, faUserGroup, faHashtag, faComments
} from '@fortawesome/free-solid-svg-icons'
import '@fortawesome/fontawesome-svg-core/styles.css'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'; // Import VueQueryPlugin and QueryClient


// 讓 Font Awesome 不自動插入 <style>，避免與 Vite 衝突
config.autoAddCss = false
library.add(
    faChevronUp, faChevronDown, faCreditCard, faLock, faCheck, faUser, 
    faLocationDot, faComment, faShieldHalved, faCircleInfo, faAlarmClock, 
    faHouse, faAddressBook, faStar, faCalendarDays, faUserGroup, faHashtag, faComments
);
dom.watch()

const app = createApp(App)
const pinia = createPinia();
const queryClient = new QueryClient();

app.component('font-awesome-icon', FontAwesomeIcon)

app.use(pinia);

app.use(router);
app.use(VueQueryPlugin, { queryClient });

app.mount('#app')
