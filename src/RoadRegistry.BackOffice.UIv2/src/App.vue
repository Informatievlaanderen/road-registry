<template>
  <div id="app">
    <!-- <img alt="Vue logo"> -->
    <HelloWorld msg="Welcome to Your Vue.js + TypeScript App"/>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import HelloWorld from './core/components/HelloWorld.vue';
import { PublicApi, BackOfficeApi } from "./services"

@Component({
  components: {
    HelloWorld,
  },
  data() {
    return {
    }
  },
  async mounted() {
  
    const result1 = await BackOfficeApi.ChangeFeed.getHead(10);
    const result = await PublicApi.ChangeFeed.getHead(10);
    console.log(result, result1);
  },
})
export default class App extends Vue {}
</script>

<style lang="scss">
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
