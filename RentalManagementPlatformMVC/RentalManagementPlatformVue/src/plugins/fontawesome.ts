// src/plugins/fontawesome.ts
import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

// 想用哪些icon就import哪些
import { faUser, faChartLine } from '@fortawesome/free-solid-svg-icons'
import { faHeart } from '@fortawesome/free-regular-svg-icons'
import { faFacebook, faTwitter } from '@fortawesome/free-brands-svg-icons'

// 把icon加入library
library.add(faUser, faChartLine, faHeart, faFacebook, faTwitter)

export default FontAwesomeIcon