<template>
  <div id="app">
    <wr-header />

    <section class="vl-region">
      <div class="vl-layout">
        <router-view></router-view>
      </div>
    </section>

    <wr-footer />
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

@Component({
  components: {},
  data() {
    return {};
  },
  async mounted() {},
})
export default class App extends Vue {}
</script>

<style lang="scss">
@import "./scss/theme.scss";
</style>