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
import Vue from "vue";

export default Vue.extend({});
</script>

<style lang="scss">
@import "./scss/theme.scss";
</style>
