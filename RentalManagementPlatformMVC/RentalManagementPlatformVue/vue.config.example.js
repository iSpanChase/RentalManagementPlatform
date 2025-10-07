const { defineConfig } = require('@vue/cli-service')

module.exports = defineConfig({
  transpileDependencies: [],
  devServer: {
    proxy: {
      '^/api': {
        target: 'https://localhost:7230', // 你的 ASP.NET Core port
        changeOrigin: true,
        secure: false,                     // 自簽憑證時設 false
        pathRewrite: { '^/api': '' }       // 把前端的 /api 移除，轉給後端的根路徑
      }
    }
  }
})