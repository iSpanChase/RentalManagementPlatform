// env.d.ts  (或 src/shims-vue.d.ts)
// 放在專案根的 src/ 或 根目錄都可以，關鍵是 tsconfig 要包含到（你的 tsconfig.app.json 已 include "env.d.ts"）
declare module '*.vue' {
    import { DefineComponent } from 'vue'
    const component: DefineComponent<{}, {}, any>
    export default component
}
// src/types/shims-js.d.ts
declare module '*.js' {
    const value: any;
    export default value;
}

// 這一行可順便加上 Vite types（可選，但建議）
/// <reference types="vite/client" />