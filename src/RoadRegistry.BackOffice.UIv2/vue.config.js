const { defineConfig } = require("@vue/cli-service");
const API_ENDPOINT = process.env.VUE_APP_API_ENDPOINT;
const API_OLDENDPOINT = process.env.VUE_APP_API_OLDENDPOINT;

module.exports = defineConfig({
  lintOnSave: false,
  transpileDependencies: true,
  configureWebpack: {
    devtool: "source-map",
    plugins: [],
  },
  devServer: {
    open: false,
    hot: true,
    liveReload: false,
    compress:false,
    https: false,
    allowedHosts: ["localhost:1234"],
    host: "localhost",
    port: 1234,
    proxy: {
      "/public/v1/": {
        target: API_ENDPOINT,
        changeOrigin: true,
        pathRewrite: {"/public/v1/": "/v1/"}
      },
      "/roads/v1/": {
        target: API_OLDENDPOINT,
        changeOrigin: true,
        pathRewrite: {"/roads/v1/": "/v1/"}
      },
      "/docs": {
        target: API_ENDPOINT,
        changeOrigin: true,
      }
    }
  }
});

