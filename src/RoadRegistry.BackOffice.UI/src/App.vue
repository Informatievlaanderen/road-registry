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
import { defineComponent } from "vue";

export default defineComponent({});
</script>

<style lang="scss">
@import "./scss/base.scss";
</style>
