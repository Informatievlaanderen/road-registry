<template>
  <div>
    <h1>HOME PAGE</h1>

    <div class="vl-alert" role="alertdialog">
      <div class="vl-alert__icon">
        <i class="vl-vi vl-vi-warning" aria-hidden="true"></i>
      </div>
      <div class="vl-alert__content">
        <p class="vl-alert__title">Opgelet!</p>
        <div class="vl-alert__message">
          <p>U heeft geen rechten om deze actie uit te voeren. <a href="#">Vraag rechten aan</a>.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { PublicApi, BackOfficeApi } from "../services";

export default Vue.extend({
  data() {
    return {};
  },
  async mounted() {
    const result1 = await BackOfficeApi.ChangeFeed.getHead(10);
    const result = await PublicApi.ChangeFeed.getHead(10);
    console.log(result, result1);
  },
});
</script>

<style lang="scss">
</style>
