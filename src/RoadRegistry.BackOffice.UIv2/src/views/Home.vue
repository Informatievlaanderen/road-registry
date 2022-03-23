<template>
  <div>
    <h1>HOME PAGE</h1>
    <wr-go-to />
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
